#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.Configuration;
using Speedy.Storage;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy database.
	/// </summary>
	[Serializable]
	public abstract class Database : IDatabase
	{
		#region Fields

		private int _saveChangeCount;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database class.
		/// </summary>
		/// <param name="filePath"> The path to store the database files. </param>
		/// <param name="options"> The options for this database. </param>
		protected Database(string filePath, DatabaseOptions options)
		{
			FilePath = filePath;
			Options = options ?? DatabaseOptions.GetDefaults();
			Mappings = new List<IPropertyConfiguration>();
			Repositories = new Dictionary<string, IRepository>();
			Relationships = new Dictionary<string, object[]>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		internal string FilePath { get; }

		internal IRepository<SyncTombstone> SyncTombstones { get; private set; }

		private ICollection<IPropertyConfiguration> Mappings { get; }

		private Dictionary<string, object[]> Relationships { get; }

		private Dictionary<string, IRepository> Repositories { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Repositories.Values.ForEach(x => x.Dispose());
		}

		/// <summary>
		/// Gets a read only repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T> GetReadOnlyRepository<T>() where T : Entity, new()
		{
			return GetEntityRepository<T>();
		}

		/// <summary>
		/// Gets a repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T> GetRepository<T>() where T : Entity, new()
		{
			return GetEntityRepository<T>();
		}

		/// <summary>
		/// Gets a list of syncable repositories.
		/// </summary>
		/// <returns> The list of syncable repositories. </returns>
		public IEnumerable<ISyncableRepository> GetSyncableRepositories()
		{
			var syncableRepositories = Repositories.Where(x => x.Value is ISyncableRepository).Select(x => x.Value);

			if (Options.SyncOrder.Length <= 0)
			{
				return syncableRepositories.Cast<ISyncableRepository>();
			}

			var order = Options.SyncOrder.Reverse().ToList();
			var query = Repositories
				.Where(x => x.Value is ISyncableRepository)
				.OrderBy(x => x.Key == order[0]);

			query = order.Skip(1).Aggregate(query, (current, key) => current.ThenBy(x => x.Key == key));
			return query.Select(x => x.Value).Cast<ISyncableRepository>();
		}

		/// <summary>
		/// Gets a syncable repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository for the type. </returns>
		public ISyncableRepository<T> GetSyncableRepository<T>() where T : SyncEntity, new()
		{
			return GetSyncableEntityRepository<T>();
		}

		/// <summary>
		/// Gets a syncable repository of the requested entity.
		/// </summary>
		/// <returns> The repository of entities requested. </returns>
		public ISyncableRepository GetSyncableRepository(Type type)
		{
			return Repositories.FirstOrDefault(x => x.Key == type.FullName).Value as ISyncableRepository;
		}

		/// <summary>
		/// Gets a list of sync tombstones that represent deleted entities.
		/// </summary>
		/// <param name="filter"> </param>
		/// <returns> The list of sync tombstones. </returns>
		public IQueryable<SyncTombstone> GetSyncTombstones(Expression<Func<SyncTombstone, bool>> filter)
		{
			return SyncTombstones.Where(filter);
		}

		/// <summary>
		/// Removes sync tombstones that represent match the filter.
		/// </summary>
		/// <param name="filter"> The filter to use. </param>
		public void RemoveSyncTombstones(Expression<Func<SyncTombstone, bool>> filter)
		{
			SyncTombstones.Remove(filter);
		}

		/// <summary>
		/// Save the data to the data store.
		/// </summary>
		/// <returns> The number of items saved. </returns>
		public virtual int SaveChanges()
		{
			if (_saveChangeCount++ > 2)
			{
				throw new OverflowException("Database save changes stuck in a processing loop.");
			}

			try
			{
				Repositories.Values.ForEach(x => x.ValidateEntities());
				Repositories.Values.ForEach(x => x.UpdateRelationships());
				Repositories.Values.ForEach(x => x.AssignKeys());
				Repositories.Values.ForEach(x => x.UpdateLocalSyncIds());
				Repositories.Values.ForEach(x => x.UpdateRelationships());

				var response = Repositories.Values.Sum(x => x.SaveChanges());

				if (Repositories.Any(x => x.Value.HasChanges()))
				{
					response += SaveChanges();
				}

				return response;
			}
			finally
			{
				_saveChangeCount = 0;
			}
		}

		/// <summary>
		/// Gets a repository to track deleted entities.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository to track deletions. </returns>
		protected IRepository<T> GetSyncTombstonesRepository<T>() where T : SyncTombstone, new()
		{
			if (SyncTombstones != null)
			{
				throw new InvalidOperationException("The sync tombstone repository has already been set.");
			}

			var repository = GetEntityRepository<T>();
			SyncTombstones = (IRepository<SyncTombstone>) repository;
			return repository;
		}

		/// <summary>
		/// Creates a configuration that represent a one to many relationship.
		/// </summary>
		/// <param name="entity"> The entity to relate to. </param>
		/// <param name="foreignKey"> The ID for the entity to relate to. </param>
		/// <param name="collectionKey"> The collection on the entity that relates back to this entity. </param>
		/// <typeparam name="T1"> The entity that host the relationship. </typeparam>
		/// <typeparam name="T2"> The entity to build a relationship to. </typeparam>
		protected void HasMany<T1, T2>(Expression<Func<T1, T2>> entity, Expression<Func<T1, int>> foreignKey, Expression<Func<T2, ICollection<T1>>> collectionKey)
			where T1 : Entity
			where T2 : Entity
		{
			var key = typeof(T2).Name + (collectionKey as dynamic).Body.Member.Name;
			var repository = Repositories.FirstOrDefault(x => x.Key == typeof(T1).FullName).Value;
			Relationships.Add(key, new object[] { repository, entity, entity.Compile(), foreignKey, foreignKey.Compile() });
		}

		/// <summary>
		/// Creates a configuration for an entity property.
		/// </summary>
		/// <param name="expression"> The expression for the property. </param>
		/// <typeparam name="T"> The entity for the configuration. </typeparam>
		/// <returns> The configuration for the entity property. </returns>
		protected PropertyConfiguration<T> Property<T>(Expression<Func<T, object>> expression) where T : Entity
		{
			var response = new PropertyConfiguration<T>(expression);

			Mappings.Add(response);

			return response;
		}

		private static void AssignNewValue<T1, T2>(T1 obj, Expression<Func<T1, T2>> expression, T2 value)
		{
			var valueParameterExpression = Expression.Parameter(typeof(T2));
			var targetExpression = (expression.Body as UnaryExpression)?.Operand ?? expression.Body;

			var assign = Expression.Lambda<Action<T1, T2>>
				(
					Expression.Assign(targetExpression, Expression.Convert(valueParameterExpression, targetExpression.Type)),
					expression.Parameters.Single(), valueParameterExpression
				);

			assign.Compile().Invoke(obj, value);
		}

		private IEnumerable<Entity> BuildRelationship(Type entityType, Type collectionType, Entity entity, IEnumerable collection, string key)
		{
			var genericMethod = GetGenericMethod("BuildRelationship", new[] { entityType, collectionType }, entityType, typeof(IEnumerable), typeof(string));
			return (IEnumerable<Entity>) genericMethod.Invoke(this, new object[] { entity, collection, key });
		}

		/// <summary>
		/// Builds relationship repository for the entity provided.
		/// </summary>
		/// <typeparam name="T1"> The type of the entity with the relationship. </typeparam>
		/// <typeparam name="T2"> The type of the related collection. </typeparam>
		/// <param name="entity"> The entity to process. </param>
		/// <param name="collection"> The entities to add or update to the repository. </param>
		/// <param name="key"> The key of the relationship </param>
		/// <returns> The repository for the relationship. </returns>
		// ReSharper disable once UnusedMember.Local
		private RelationshipRepository<T2> BuildRelationship<T1, T2>(T1 entity, IEnumerable collection, string key)
			where T1 : Entity, new()
			where T2 : Entity, new()
		{
			if (!Relationships.ContainsKey(key))
			{
				return null;
			}

			var value = Relationships[key];
			var repository = (IRepository<T2>) value[0];
			var entityExpression = (Expression<Func<T2, T1>>) value[1];
			var entityFunction = (Func<T2, T1>) value[2];
			var foreignKeyExpression = (Expression<Func<T2, int>>) value[3];
			var foreignKeyFunction = (Func<T2, int>) value[4];

			var response = new RelationshipRepository<T2>((Repository<T2>) repository, x =>
			{
				var invokedKey = foreignKeyFunction.Invoke(x);
				if (invokedKey == entity.Id)
				{
					return true;
				}

				var invokedEntity = entityFunction.Invoke(x);
				return invokedEntity == entity;
			}, x =>
			{
				if (entity?.Id > 0)
				{
					AssignNewValue(x, foreignKeyExpression, entity.Id);
				}
				else
				{
					AssignNewValue(x, entityExpression, entity);
				}
			}, x =>
			{
				var invokedEntity = entityFunction.Invoke(x);
				if (invokedEntity == null)
				{
					return;
				}

				var invokedKey = foreignKeyFunction.Invoke(x);
				if (invokedKey != invokedEntity.Id)
				{
					invokedEntity.Id = invokedKey;
				}
			});

			collection.ForEach(x => response.AddOrUpdate((T2) x));
			return response;
		}

		private void DeletingEntity(Entity entity)
		{
			var key = entity.GetRealType().Name;

			foreach (var relationship in Relationships.Where(x => x.Key.StartsWith(key)))
			{
				var repository = (IRepository) relationship.Value[0];
				if (!repository.HasDependentRelationship(relationship.Value, entity.Id))
				{
					continue;
				}

				var message = "The operation failed: The relationship could not be changed because one or more of the foreign-key properties is non-nullable. When a change is made to a relationship, the related foreign-key property is set to a null value. If the foreign-key does not support null values, a new relationship must be defined, the foreign-key property must be assigned another non-null value, or the unrelated object must be deleted.";
				throw new InvalidOperationException(message);
			}
		}

		private Repository<T> GetEntityRepository<T>() where T : Entity, new()
		{
			var type = typeof(T);
			var key = type.FullName;

			if (Repositories.ContainsKey(key))
			{
				return (Repository<T>) Repositories[key];
			}

			var repository = new Repository<T>(this);
			repository.DeletingEntity += DeletingEntity;
			repository.UpdateEntityRelationships += UpdateEntityRelationships;
			repository.ValidateEntity += ValidateEntity;
			repository.Initialize();

			Repositories.Add(key, repository);
			return repository;
		}

		private static MethodInfo GetGenericMethod(string methodName, Type[] typeArgs, params Type[] argTypes)
		{
			var myType = typeof(Database);
			var methodInfos = myType.GetCachedMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			var methods = methodInfos.Where(m => m.Name == methodName
				&& typeArgs.Length == m.GetCachedGenericArguments().Count
				&& argTypes.Length == m.GetCachedParameters().Count)
				.ToList();

			foreach (var method in methods)
			{
				var m = method.CachedMakeGenericMethod(typeArgs);
				if (m.GetCachedParameters().Select((p, i) => p.ParameterType == argTypes[i]).All(x => x))
				{
					return m;
				}
			}

			return null;
		}

		private SyncableRepository<T> GetSyncableEntityRepository<T>() where T : SyncEntity, new()
		{
			var type = typeof(T);
			var key = type.FullName;

			if (Repositories.ContainsKey(key))
			{
				return (SyncableRepository<T>) Repositories[key];
			}

			var repository = new SyncableRepository<T>(this);
			repository.DeletingEntity += DeletingEntity;
			repository.UpdateEntityRelationships += UpdateEntityRelationships;
			repository.ValidateEntity += ValidateEntity;
			repository.Initialize();

			Repositories.Add(key, repository);
			return repository;
		}

		private void UpdateEntityChildRelationships(Entity item, Entity entity)
		{
			var itemType = item.GetType();
			var entityType = entity.GetType();
			var properties = itemType.GetCachedProperties();

			var entityRelationship = properties.FirstOrDefault(x => x.Name == entityType.Name);
			entityRelationship?.SetValue(item, entity, null);

			var entityRelationshipId = properties.FirstOrDefault(x => x.Name == entityType.Name + "Id");
			entityRelationshipId?.SetValue(item, entity.Id, null);
		}

		private void UpdateEntityCollectionRelationships(Entity entity, Type entityType, IEnumerable<PropertyInfo> properties)
		{
			var enumerableType = typeof(IEnumerable);
			var collectionRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				// Check to see if we have a repository for the generic type.
				var collectionType = relationship.PropertyType.GetCachedGenericArguments()[0];
				if (!Repositories.ContainsKey(collectionType.FullName))
				{
					continue;
				}

				// Converts the relationship to a relationship (filtered) repository.
				var currentCollection = (IEnumerable<Entity>) relationship.GetValue(entity, null);
				var currentCollectionType = currentCollection.GetType();

				if (currentCollectionType.Name == typeof(RelationshipRepository<>).Name)
				{
					// We are already a relationship repository so just update the relationships
					continue;
				}

				// Add any existing entities to the new filtered collection.
				foreach (var item in currentCollection)
				{
					//relationshipFilter.AddOrUpdate(item);
					UpdateEntityChildRelationships(item, entity);
					UpdateEntityDirectRelationships(item);
				}

				// See if the entity has a relationship filter.
				var key1 = entityType.Name + collectionType.Name;
				var key2 = entityType.Name + relationship.Name;

				var relationshipFilter = BuildRelationship(entityType, collectionType, entity, currentCollection, key1)
					?? BuildRelationship(entityType, collectionType, entity, currentCollection, key2);

				// Check to see if the custom memory context has a filter method.
				if (relationshipFilter == null)
				{
					// No filter so there's nothing to do.
					continue;
				}

				// Update relationship collection to the new filtered collection.
				relationship.SetValue(entity, relationshipFilter, null);
			}
		}

		private void UpdateEntityDirectRelationships(Entity entity)
		{
			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			UpdateEntityDirectRelationships(entity, entityType, properties);
		}

		private void UpdateEntityDirectRelationships(Entity entity, Type entityType, ICollection<PropertyInfo> properties)
		{
			var baseEntityType = typeof(Entity);
			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => baseEntityType.IsAssignableFrom(x.PropertyType))
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var otherEntity = entityRelationship.GetValue(entity, null) as Entity;
				var entityRelationshipIdProperty = properties.FirstOrDefault(x => x.Name == entityRelationship.Name + "Id");

				if (otherEntity == null && entityRelationshipIdProperty != null)
				{
					var otherEntityId = (int?) entityRelationshipIdProperty.GetValue(entity, null);
					if (otherEntityId.HasValue && otherEntityId != 0 && Repositories.ContainsKey(entityRelationship.PropertyType.FullName))
					{
						var repository = Repositories[entityRelationship.PropertyType.FullName];
						otherEntity = repository.Read(otherEntityId.Value);
						entityRelationship.SetValue(entity, otherEntity, null);
					}
				}
				else if (otherEntity != null && entityRelationshipIdProperty != null)
				{
					// Check to see if this is a new child entity.
					if (otherEntity.Id == 0)
					{
						var repository = Repositories[entityRelationship.PropertyType.FullName];
						var repositoryType = repository.GetType();

						if (otherEntity.GetType() == entityType)
						{
							repositoryType.CachedGetMethod("InsertBefore").Invoke(repository, new object[] { otherEntity, entity });
						}
						else
						{
							// Still adding 400ms per 10000 items, why?
							repositoryType.CachedGetMethod("Add", otherEntity.GetType()).Invoke(repository, new object[] { otherEntity });
						}
					}

					var otherEntityId = (int?) entityRelationshipIdProperty.GetValue(entity, null);
					if (otherEntityId != otherEntity.Id)
					{
						// resets entityId to entity.Id if it does not match
						entityRelationshipIdProperty.SetValue(entity, otherEntity.Id, null);
					}
				}
			}
		}

		private void UpdateEntityRelationships(Entity entity)
		{
			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			UpdateEntityDirectRelationships(entity, entityType, properties);
			UpdateEntityCollectionRelationships(entity, entityType, properties);
		}

		private void ValidateEntity(Entity entity)
		{
			foreach (var validation in Mappings.Where(x => x.IsMappingFor(entity)))
			{
				validation.Validate(entity);
			}
		}

		#endregion
	}
}