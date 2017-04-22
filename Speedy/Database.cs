#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.Configuration;
using Speedy.Storage;

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
		/// <param name="directory"> The directory to store the database files. </param>
		/// <param name="options"> The options for this database. </param>
		protected Database(string directory, DatabaseOptions options)
		{
			Directory = directory;
			OneToManyRelationships = new Dictionary<string, object[]>();
			Options = options ?? new DatabaseOptions();
			PropertyConfigurations = new List<IPropertyConfiguration>();
			Repositories = new Dictionary<string, IRepository>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		internal string Directory { get; }

		private Dictionary<string, object[]> OneToManyRelationships { get; }

		private ICollection<IPropertyConfiguration> PropertyConfigurations { get; }

		private Dictionary<string, IRepository> Repositories { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Discard all changes made in this context to the underlying database.
		/// </summary>
		public int DiscardChanges()
		{
			return Repositories.Values.Sum(x => x.DiscardChanges());
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets a read only repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T, T2> GetReadOnlyRepository<T, T2>()
			where T : Entity<T2>, new()
			where T2 : new()
		{
			return GetRepository<T, T2>();
		}

		/// <summary>
		/// Gets a repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T, T2> GetRepository<T, T2>()
			where T : Entity<T2>, new()
			where T2 : new()
		{
			var type = typeof(T);
			var key = type.FullName;

			if (Repositories.ContainsKey(key))
			{
				return (Repository<T, T2>) Repositories[key];
			}

			var repository = CreateRepository<T, T2>();
			Repositories.Add(key, repository);
			return repository;
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
				//Repositories.Values.ForEach(x => x.UpdateLocalSyncIds());
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
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing"> true if managed resources should be disposed; otherwise, false. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Repositories.Values.ForEach(x => x.Dispose());
			}
		}

		/// <summary>
		/// Creates a configuration that represent an optional one to many relationship.
		/// </summary>
		/// <param name="entity"> The entity to relate to. </param>
		/// <param name="foreignKey"> The ID for the entity to relate to. </param>
		/// <param name="collectionKey"> The collection on the entity that relates back to this entity. </param>
		/// <typeparam name="T1"> The entity that host the relationship. </typeparam>
		/// <typeparam name="T2"> The entity to build a relationship to. </typeparam>
		protected void HasOptional<T1, T2, T3>(Expression<Func<T1, T2>> entity, Expression<Func<T1, object>> foreignKey, Expression<Func<T2, ICollection<T1>>> collectionKey = null)
			where T1 : Entity<T3>, new()
			where T2 : Entity<T3>
			where T3 : new()
		{
			Property<T1, T3>(foreignKey).IsOptional();

			if (collectionKey != null)
			{
				var key = typeof(T2).Name + (collectionKey as dynamic).Body.Member.Name;
				var repositoryFactory = GetRepository<T1, T3>();
				OneToManyRelationships.Add(key, new object[] { repositoryFactory, entity, entity.Compile(), foreignKey, foreignKey.Compile() });
			}
		}

		/// <summary>
		/// Creates a configuration that represent a required one to many relationship.
		/// </summary>
		/// <param name="entity"> The entity to relate to. </param>
		/// <param name="collectionKey"> The collection on the entity that relates back to this entity. </param>
		/// <param name="foreignKey"> The ID for the entity to relate to. </param>
		/// <typeparam name="T1"> The entity that host the relationship. </typeparam>
		/// <typeparam name="T2"> The entity to build a relationship to. </typeparam>
		protected void HasRequired<T1, T2, T3>(Expression<Func<T1, T2>> entity, Expression<Func<T1, object>> foreignKey, Expression<Func<T2, ICollection<T1>>> collectionKey = null)
			where T1 : Entity<T3>, new()
			where T2 : Entity<T3>
			where T3 : new()
		{
			Property<T1, T3>(foreignKey).IsRequired();

			if (collectionKey != null)
			{
				var key = typeof(T2).Name + (collectionKey as dynamic).Body.Member.Name;
				var repositoryFactory = GetRepository<T1, T3>();
				OneToManyRelationships.Add(key, new object[] { repositoryFactory, entity, entity.Compile(), foreignKey, foreignKey.Compile() });
			}
		}

		/// <summary>
		/// Creates a configuration for an entity property.
		/// </summary>
		/// <param name="expression"> The expression for the property. </param>
		/// <typeparam name="T"> The entity for the configuration. </typeparam>
		/// <returns> The configuration for the entity property. </returns>
		protected PropertyConfiguration<T, T2> Property<T, T2>(Expression<Func<T, object>> expression) where T : Entity<T2>
		{
			var response = new PropertyConfiguration<T, T2>(expression);
			PropertyConfigurations.Add(response);
			return response;
		}

		internal void UpdateDependantIds<T, T2>(Entity<T2> entity, Func<Entity<T2>, T2> action, List<Entity<T2>> processed) where T : Entity<T2>
		{
			if (processed.Contains(entity))
			{
				return;
			}

			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			processed.Add(entity);

			UpdateDependantIds<T, T2>(entity, properties, action, processed);
			UpdateDependantCollectionIds<T, T2>(entity, properties, action, processed);
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

		private IEnumerable BuildRelationship<T, T2>(Type entityType, Type collectionType, T entity, IEnumerable collection, string key)
			where T : Entity<T2>, new()
		{
			var genericMethod = GetGenericMethod("BuildRelationship", new[] { entityType, collectionType, typeof(T2) }, entityType, typeof(IEnumerable), typeof(string));
			return (IEnumerable) genericMethod.Invoke(this, new object[] { entity, collection, key });
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
		private RelationshipRepository<T2, T3> BuildRelationship<T1, T2, T3>(T1 entity, IEnumerable collection, string key)
			where T1 : Entity<T3>, new()
			where T2 : Entity<T3>, new()
			where T3 : new()
		{
			if (!OneToManyRelationships.ContainsKey(key) || entity == null)
			{
				return null;
			}

			var value = OneToManyRelationships[key];
			var repository = (IRepository<T2, T3>) value[0];
			var entityExpression = (Expression<Func<T2, T1>>) value[1];
			var entityFunction = (Func<T2, T1>) value[2];
			var foreignKeyExpression = (Expression<Func<T2, object>>) value[3];
			var foreignKeyFunction = (Func<T2, object>) value[4];

			var response = new RelationshipRepository<T2, T3>((Repository<T2, T3>) repository, x =>
			{
				var invokedKey = foreignKeyFunction.Invoke(x);
				if (invokedKey?.Equals(entity.Id) == true)
				{
					return true;
				}

				var invokedEntity = entityFunction.Invoke(x);
				return invokedEntity == entity;
			}, x =>
			{
				if (entity.IdIsSet())
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
				if (invokedKey != null && !invokedKey.Equals(invokedEntity.Id))
				{
					invokedEntity.Id = (T3) invokedKey;
				}
			});

			collection.ForEach(x => response.AddOrUpdate((T2) x));
			return response;
		}

		private Repository<T, T2> CreateRepository<T, T2>()
			where T : Entity<T2>, new()
			where T2 : new()
		{
			var repository = new Repository<T, T2>(this);
			repository.DeletingEntity += DeletingEntity;
			repository.UpdateEntityRelationships += UpdateEntityRelationships<T, T2>;
			repository.ValidateEntity += ValidateEntity;
			repository.Initialize();
			return repository;
		}

		private void DeletingEntity<T2>(Entity<T2> entity)
		{
			var key = entity.GetRealType().Name;

			foreach (var relationship in OneToManyRelationships.Where(x => x.Key.StartsWith(key)))
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

		private static MethodInfo GetGenericMethod(string methodName, Type[] typeArgs, params Type[] argTypes)
		{
			var myType = typeof(Database);
			var methodInfos = myType.GetCachedMethods(BindingFlags.NonPublic | BindingFlags.Instance);

			var methods = methodInfos.Where(m => m.Name == methodName).ToList();

			foreach (var method in methods)
			{
				var gCount = method.GetCachedGenericArguments().Count;
				var pCount = method.GetCachedParameters().Count;

				if (typeArgs.Length != gCount || argTypes.Length != pCount)
				{
					continue;
				}

				var m = method.CachedMakeGenericMethod(typeArgs);
				if (m.GetCachedParameters().Select((p, i) => p.ParameterType == argTypes[i]).All(x => x))
				{
					return m;
				}
			}

			return null;
		}

		private void UpdateDependantCollectionIds<T, T2>(Entity<T2> entity, ICollection<PropertyInfo> properties, Func<Entity<T2>, T2> action, List<Entity<T2>> processed) where T : Entity<T2>
		{
			var entityType = typeof(T);
			var enumerableType = typeof(IEnumerable);
			var collectionRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				var currentCollection = (IEnumerable<Entity<T2>>) relationship.GetValue(entity, null);
				var currentCollectionType = relationship.PropertyType.GetGenericArguments()[0];

				foreach (var item in currentCollection)
				{
					if (currentCollectionType == entityType)
					{
						if (!item.IdIsSet())
						{
							item.Id = action(item);
						}
					}
					else
					{
						UpdateDependantIds<T, T2>(item, action, processed);
					}
				}
			}
		}

		private void UpdateDependantIds<T, T2>(Entity<T2> entity, ICollection<PropertyInfo> properties, Func<Entity<T2>, T2> action, List<Entity<T2>> processed) where T : Entity<T2>
		{
			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var expectedEntity = entityRelationship.GetValue(entity, null) as T;
				if (expectedEntity != null)
				{
					if (!expectedEntity.IdIsSet())
					{
						expectedEntity.Id = action(expectedEntity);
					}

					continue;
				}

				var otherEntity = entityRelationship.GetValue(entity, null) as Entity<T2>;
				if (otherEntity != null)
				{
					UpdateDependantIds<T, T2>(otherEntity, action, processed);
				}
			}
		}

		private void UpdateEntityChildRelationships<T>(Entity<T> item, Entity<T> entity)
		{
			var itemType = item.GetType();
			var entityType = entity.GetType();
			var properties = itemType.GetCachedProperties();

			var entityRelationship = properties.FirstOrDefault(x => x.Name == entityType.Name);
			entityRelationship?.SetValue(item, entity, null);

			var entityRelationshipId = properties.FirstOrDefault(x => x.Name == entityType.Name + "Id");
			entityRelationshipId?.SetValue(item, entity.Id, null);
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private void UpdateEntityCollectionRelationships<T, T2>(T entity, Type entityType, IEnumerable<PropertyInfo> properties)
			where T : Entity<T2>, new() 
			where T2 : new()
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
				var currentCollection = (IEnumerable<Entity<T2>>) relationship.GetValue(entity, null);
				var currentCollectionType = currentCollection.GetType();

				if (currentCollectionType.Name == typeof(RelationshipRepository<T, T2>).Name)
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

				var relationshipFilter = BuildRelationship<T, T2>(entityType, collectionType, entity, currentCollection, key1)
					?? BuildRelationship<T, T2>(entityType, collectionType, entity, currentCollection, key2);

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

		private void UpdateEntityDirectRelationships<T>(Entity<T> entity)
		{
			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			UpdateEntityDirectRelationships(entity, entityType, properties);
		}

		private void UpdateEntityDirectRelationships<T>(Entity<T> entity, Type entityType, ICollection<PropertyInfo> properties)
		{
			var baseEntityType = typeof(Entity<T>);
			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => baseEntityType.IsAssignableFrom(x.PropertyType))
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var otherEntity = entityRelationship.GetValue(entity, null) as Entity<T>;
				var entityRelationshipIdProperty = properties.FirstOrDefault(x => x.Name == entityRelationship.Name + "Id");

				if (otherEntity == null && entityRelationshipIdProperty != null)
				{
					var otherEntityId = entityRelationshipIdProperty.GetValue(entity, null);
					if (otherEntityId is T && Repositories.ContainsKey(entityRelationship.PropertyType.FullName))
					{
						var repository = Repositories[entityRelationship.PropertyType.FullName];
						otherEntity = (Entity<T>) repository.Read(otherEntityId);
						entityRelationship.SetValue(entity, otherEntity, null);
					}
				}
				else if (otherEntity != null && entityRelationshipIdProperty != null)
				{
					// Check to see if this is a new child entity.
					if (!otherEntity.IdIsSet())
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

					var otherEntityId = entityRelationshipIdProperty.GetValue(entity, null);
					if (!Equals(otherEntityId, otherEntity.Id))
					{
						// resets entityId to entity.Id if it does not match
						entityRelationshipIdProperty.SetValue(entity, otherEntity.Id, null);
					}
				}
			}
		}

		private void UpdateEntityRelationships<T, T2>(T entity)
			where T : Entity<T2>, new() 
			where T2 : new()
		{
			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			UpdateEntityDirectRelationships(entity, entityType, properties);
			UpdateEntityCollectionRelationships<T, T2>(entity, entityType, properties);
		}

		private void ValidateEntity<T, T2>(T entity, IRepository<T, T2> repository) where T : Entity<T2>
		{
			foreach (var validation in PropertyConfigurations.Where(x => x.IsMappingFor(entity)))
			{
				validation.Validate(entity, repository);
			}
		}

		#endregion
	}
}