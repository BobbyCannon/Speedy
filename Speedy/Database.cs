#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Speedy.Configuration;
using Speedy.Exceptions;
using Speedy.Storage;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy database.
	/// </summary>
	[Serializable]
	public abstract class Database : ISyncableDatabase
	{
		#region Fields

		private readonly CollectionChangeTracker _collectionChangeTracker;
		private int _saveChangeCount;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the database class.
		/// </summary>
		/// <param name="options"> The options for this database. </param>
		protected Database(DatabaseOptions options)
		{
			OneToManyRelationships = new Dictionary<string, object[]>();
			Options = options?.DeepClone() ?? new DatabaseOptions();
			PropertyConfigurations = new Dictionary<string, IPropertyConfiguration>();
			Repositories = new Dictionary<string, IDatabaseRepository>();

			_collectionChangeTracker = new CollectionChangeTracker();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		internal Dictionary<string, IDatabaseRepository> Repositories { get; }

		private Dictionary<string, object[]> OneToManyRelationships { get; }

		private IDictionary<string, IPropertyConfiguration> PropertyConfigurations { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public T Add<T, T2>(T item) where T : Entity<T2>
		{
			GetRepository<T, T2>().Add(item);
			return item;
		}

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
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets the assembly that contains the entity mappings. Base implementation defaults to the implemented types assembly.
		/// </summary>
		public virtual Assembly GetMappingAssembly()
		{
			return GetType().Assembly;
		}

		/// <summary>
		/// Gets a read only repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T, T2> GetReadOnlyRepository<T, T2>() where T : Entity<T2>
		{
			return GetRepository<T, T2>();
		}

		/// <summary>
		/// Gets a repository for the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the item in the repository. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The repository for the type. </returns>
		public IRepository<T, T2> GetRepository<T, T2>() where T : Entity<T2>
		{
			var type = typeof(T);
			var key = type.ToAssemblyName();

			if (Repositories.ContainsKey(key))
			{
				return (Repository<T, T2>) Repositories[key];
			}

			var repository = CreateRepository<T, T2>();
			Repositories.Add(key, repository);
			return repository;
		}

		/// <inheritdoc />
		public IEnumerable<ISyncableRepository> GetSyncableRepositories(SyncOptions options)
		{
			var syncableRepositories = Repositories.Where(x => x.Value is ISyncableRepository).ToList();

			if (Options.SyncOrder.Length <= 0)
			{
				return syncableRepositories
					.Select(x => x.Value)
					.Cast<ISyncableRepository>()
					.Where(x => !options.ShouldFilterRepository(x.TypeName))
					.ToList();
			}

			var order = Options.SyncOrder.Reverse().ToList();
			var query = syncableRepositories
				.Where(x => !options.ShouldFilterRepository(x.Key))
				.OrderBy(x => x.Key == order[0]);

			var response = order
				.Skip(1)
				.Aggregate(query, (current, key) => current.ThenBy(x => x.Key == key))
				.Select(x => x.Value)
				.Cast<ISyncableRepository>()
				.ToList();

			return response;
		}

		/// <inheritdoc />
		public ISyncableRepository<T, T2> GetSyncableRepository<T, T2>() where T : SyncEntity<T2>
		{
			return GetSyncableEntityRepository<T, T2>();
		}

		/// <inheritdoc />
		public ISyncableRepository GetSyncableRepository(Type type)
		{
			var r = Repositories.FirstOrDefault(x => x.Key == type.ToAssemblyName());
			if (r.Key == null)
			{
				return null;
			}

			return r.Value as ISyncableRepository;
		}

		/// <summary>
		/// Creates a configuration that represent a required one to many relationship.
		/// </summary>
		/// <param name="required"> The value to determine if this property is required. </param>
		/// <param name="entity"> The entity to relate to. </param>
		/// <param name="collectionKey"> The collection on the entity that relates back to this entity. </param>
		/// <param name="foreignKey"> The ID for the entity to relate to. </param>
		/// <typeparam name="T1"> The entity that host the relationship. </typeparam>
		/// <typeparam name="T2"> The type of the entity key of the host. </typeparam>
		/// <typeparam name="T3"> The entity to build a relationship to. </typeparam>
		/// <typeparam name="T4"> The type of the entity key to build the relationship to. </typeparam>
		public PropertyConfiguration<T1, T2> HasRequired<T1, T2, T3, T4>(bool required, Expression<Func<T1, T3>> entity, Expression<Func<T1, object>> foreignKey, Expression<Func<T3, ICollection<T1>>> collectionKey = null)
			where T1 : Entity<T2>
			where T3 : Entity<T4>
		{
			var property = Property<T1, T2>(foreignKey).IsRequired(required);

			if (collectionKey != null)
			{
				var key = typeof(T3).Name + collectionKey.GetExpressionName();
				var repositoryFactory = GetRepository<T1, T2>();
				OneToManyRelationships.Add(key, new object[] { repositoryFactory, entity, entity.Compile(), foreignKey, foreignKey.Compile(), property });
			}

			return property;
		}

		/// <summary>
		/// Creates a configuration for an entity property.
		/// </summary>
		/// <param name="expression"> The expression for the property. </param>
		/// <typeparam name="T"> The entity for the configuration. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <returns> The configuration for the entity property. </returns>
		public PropertyConfiguration<T, T2> Property<T, T2>(Expression<Func<T, object>> expression) where T : Entity<T2>
		{
			var name = $"{typeof(T).ToAssemblyName()}.{expression.GetExpressionName()}";

			if (PropertyConfigurations.ContainsKey(name))
			{
				return (PropertyConfiguration<T, T2>) PropertyConfigurations[name];
			}

			var response = new PropertyConfiguration<T, T2>(expression);
			PropertyConfigurations.Add(name, response);
			return response;
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
				var first = _saveChangeCount == 1;
				if (first)
				{
					_collectionChangeTracker.Reset();
				}

				Repositories.Values.ForEach(x => x.ValidateEntities());
				// todo: I'm pretty sure this can be removed, need more testing, rolling with it in RC
				//Repositories.Values.ForEach(x => x.UpdateRelationships());
				Repositories.Values.ForEach(x => x.AssignKeys(new List<IEntity>()));
				Repositories.Values.ForEach(x => x.UpdateRelationships());

				var response = Repositories.Values.Sum(x => x.SaveChanges());

				if (Repositories.Any(x => x.Value.HasChanges()))
				{
					response += SaveChanges();
				}

				if (first)
				{
					OnCollectionChanged(_collectionChangeTracker.Added, _collectionChangeTracker.Removed);
				}

				return response;
			}
			finally
			{
				_saveChangeCount = 0;
			}
		}

		/// <summary>
		/// Save the data to the data store.
		/// </summary>
		/// <returns> The number of items saved. </returns>
		public virtual Task<int> SaveChangesAsync()
		{
			return Task.Run(SaveChanges);
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

		internal void OnCollectionChanged(IList added, IList removed)
		{
			if (CollectionChanged == null)
			{
				return;
			}

			NotifyCollectionChangedEventArgs eventArgs = null;

			if (added.Count > 0 && removed.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, added, removed);
			}
			else if (added.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, added);
			}
			else if (removed.Count > 0)
			{
				eventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed);
			}

			if (eventArgs != null)
			{
				CollectionChanged?.Invoke(this, eventArgs);
			}
		}

		internal void UpdateDependantIds(IEntity entity, List<IEntity> processed)
		{
			if (processed.Contains(entity))
			{
				return;
			}

			var entityType = entity.GetType();
			var properties = entityType.GetCachedProperties();

			processed.Add(entity);

			UpdateDependantIds(entity, properties, processed);
			UpdateDependentCollectionIds(entity, properties, processed);
		}

		private void AddingEntity<T2>(Entity<T2> entity)
		{
			// todo: add relationship check...
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

		private IEnumerable BuildRelationship(Type entityType, Type collectionType, IEntity entity, IEnumerable collection, string key)
		{
			var collectionTypeProperties = collectionType.GetCachedProperties();
			var collectionKey = collectionTypeProperties.First(x => x.Name == "Id").PropertyType;
			var entityProperties = entityType.GetCachedProperties();
			var entityKey = entityProperties.First(x => x.Name == "Id").PropertyType;
			var genericMethod = GetGenericMethod(nameof(BuildRelationship), new[] { entityType, entityKey, collectionType, collectionKey }, entityType, typeof(IEnumerable), typeof(string));
			return (IEnumerable) genericMethod.Invoke(this, new object[] { entity, collection, key });
		}

		/// <summary>
		/// Builds relationship repository for the entity provided.
		/// </summary>
		/// <typeparam name="T1"> The type of the entity with the relationship. </typeparam>
		/// <typeparam name="T1K"> The type of the key for the entity. </typeparam>
		/// <typeparam name="T2"> The type of the related collection. </typeparam>
		/// <typeparam name="T2K"> The type of the key for the collection. </typeparam>
		/// <param name="entity"> The entity to process. </param>
		/// <param name="collection"> The entities to add or update to the repository. </param>
		/// <param name="key"> The key of the relationship </param>
		/// <returns> The repository for the relationship. </returns>
		// ReSharper disable once UnusedMember.Local
		private RelationshipRepository<T2, T2K> BuildRelationship<T1, T1K, T2, T2K>(T1 entity, IEnumerable collection, string key)
			where T1 : Entity<T1K>
			where T2 : Entity<T2K>
		{
			if (!OneToManyRelationships.ContainsKey(key) || entity == null)
			{
				return null;
			}

			var value = OneToManyRelationships[key];
			var repository = (IRepository<T2, T2K>) value[0];
			var entityExpression = (Expression<Func<T2, T1>>) value[1]; 
			var entityFunction = (Func<T2, T1>) value[2];
			var foreignKeyExpression = (Expression<Func<T2, object>>) value[3];
			var foreignKeyFunction = (Func<T2, object>) value[4];

			var response = new RelationshipRepository<T2, T2K>(key, (Repository<T2, T2K>) repository, x =>
			{
				var invokedKey = foreignKeyFunction.Invoke(x);
				if (!Equals(invokedKey, default(T2K)) && invokedKey?.Equals(entity.Id) == true)
				{
					return true;
				}

				var invokedEntity = entityFunction.Invoke(x);
				return invokedEntity == entity;
			}, x => {
				if (entity.IdIsSet())
				{
					// Should I use SetMemberValue instead? which is faster?
					AssignNewValue(x, foreignKeyExpression, entity.Id);
				}
				else
				{
					// Should I use SetMemberValue instead? which is faster?
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
					invokedEntity.Id = (T1K) invokedKey;
				}
			});

			collection.ForEach(x => response.AddOrUpdate((T2) x));
			return response;
		}

		private Repository<T, T2> CreateRepository<T, T2>() where T : Entity<T2>
		{
			var repository = new Repository<T, T2>(this);
			repository.AddingEntity += AddingEntity;
			repository.CollectionChanged += RepositoryCollectionChanged;
			repository.DeletingEntity += DeletingEntity;
			repository.UpdateEntityRelationships += UpdateEntityRelationships;
			repository.ValidateEntity += ValidateEntity;
			return repository;
		}

		private SyncableRepository<T, T2> CreateSyncableRepository<T, T2>() where T : SyncEntity<T2>
		{
			var repository = new SyncableRepository<T, T2>(this);
			repository.AddingEntity += AddingEntity;
			repository.CollectionChanged += RepositoryCollectionChanged;
			repository.DeletingEntity += DeletingEntity;
			repository.UpdateEntityRelationships += UpdateEntityRelationships;
			repository.ValidateEntity += ValidateEntity;
			return repository;
		}

		private void DeletingEntity<T2>(Entity<T2> entity)
		{
			var key = entity.GetRealType().Name;

			foreach (var relationship in OneToManyRelationships.Where(x => x.Key.StartsWith(key)))
			{
				var repository = (IDatabaseRepository) relationship.Value[0];
				var configuration = (IPropertyConfiguration) relationship.Value[5];

				if (configuration.DeleteBehavior == RelationshipDeleteBehavior.Cascade)
				{
					repository.RemoveDependent(relationship.Value, entity.Id);
					continue;
				}

				if (configuration.DeleteBehavior == RelationshipDeleteBehavior.SetNull)
				{
					repository.SetDependentToNull(relationship.Value, entity.Id);
					continue;
				}

				if (!repository.HasDependentRelationship(relationship.Value, entity.Id))
				{
					continue;
				}

				var message = "The DELETE statement conflicted with the REFERENCE constraint.";
				throw new UpdateException(message, new InvalidOperationException());
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

		private SyncableRepository<T, T2> GetSyncableEntityRepository<T, T2>() where T : SyncEntity<T2>
		{
			var type = typeof(T);
			var key = type.ToAssemblyName();

			if (Repositories.ContainsKey(key))
			{
				return (SyncableRepository<T, T2>) Repositories[key];
			}

			var repository = CreateSyncableRepository<T, T2>();
			Repositories.Add(key, repository);
			return repository;
		}

		private void RepositoryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			_collectionChangeTracker.Add(e.NewItems);
			_collectionChangeTracker.Remove(e.OldItems);
		}

		private void UpdateDependantIds(IEntity entity, ICollection<PropertyInfo> properties, List<IEntity> processed)
		{
			var entityRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				if (!(entityRelationship.GetValue(entity, null) is IEntity expectedEntity))
				{
					continue;
				}

				if (processed.Contains(expectedEntity))
				{
					continue;
				}

				var collectionType = expectedEntity.GetType();
				if (!Repositories.ContainsKey(collectionType.ToAssemblyName()))
				{
					continue;
				}

				var repository = Repositories[collectionType.ToAssemblyName()];
				repository.AssignKey(expectedEntity, processed);
			}
		}

		private void UpdateDependentCollectionIds(IEntity entity, ICollection<PropertyInfo> properties, List<IEntity> processed)
		{
			var enumerableType = typeof(IEnumerable);
			var collectionRelationships = properties
				.Where(x => x.GetCachedAccessors()[0].IsVirtual)
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				if (!(relationship.GetValue(entity, null) is IEnumerable<IEntity> currentCollection))
				{
					continue;
				}

				var collectionType = relationship.PropertyType.GetGenericArguments()[0];
				if (!Repositories.ContainsKey(collectionType.ToAssemblyName()))
				{
					continue;
				}

				var repository = Repositories[collectionType.ToAssemblyName()];
				foreach (var item in currentCollection)
				{
					repository.AssignKey(item, processed);
				}
			}
		}

		private void UpdateEntityChildRelationships(IEntity item, IEntity entity)
		{
			var itemType = item.GetType();
			var itemProperties = itemType.GetCachedProperties();
			var entityType = entity.GetType();
			var entityProperties = entityType.GetCachedProperties();

			var entityRelationship = itemProperties.FirstOrDefault(x => x.Name == entityType.Name);
			entityRelationship?.SetValue(item, entity, null);

			var entityRelationshipId = itemProperties.FirstOrDefault(x => x.Name == entityType.Name + "Id");
			var entityKeyId = entityProperties.FirstOrDefault(x => x.Name == nameof(Entity<int>.Id));
			entityRelationshipId?.SetValue(item, entityKeyId?.GetValue(entity), null);
		}

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		private void UpdateEntityCollectionRelationships(IEntity entity)
		{
			var enumerableType = typeof(IEnumerable);
			var entityType = entity.GetType();
			var collectionRelationships = entityType
				.GetCachedVirtualProperties()
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				// Check to see if we have a repository for the generic type.
				var collectionType = relationship.PropertyType.GetCachedGenericArguments()[0];
				if (!Repositories.ContainsKey(collectionType.ToAssemblyName()))
				{
					continue;
				}

				// Converts the relationship to a relationship (filtered) repository.
				var currentCollection = (IEnumerable<IEntity>) relationship.GetValue(entity, null);
				var currentCollectionType = currentCollection.GetType();

				if (typeof(IRelationshipRepository).IsAssignableFrom(currentCollectionType))
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

		private void UpdateEntityDirectRelationships(IEntity entity)
		{
			var baseType = typeof(IEntity);
			var entityType = entity.GetType();
			var entityProperties = entityType.GetCachedProperties();
			var entityRelationships = entityType.GetCachedVirtualProperties().Where(x => baseType.IsAssignableFrom(x.PropertyType)).ToList();

			foreach (var entityRelationship in entityRelationships)
			{
				var otherEntity = entityRelationship.GetValue(entity, null) as IEntity;
				var entityRelationshipIdProperty = entityProperties.FirstOrDefault(x => x.Name == entityRelationship.Name + "Id");

				if (otherEntity == null && entityRelationshipIdProperty != null)
				{
					var otherEntityId = entityRelationshipIdProperty.GetValue(entity, null);
					var defaultValue = entityRelationshipIdProperty.PropertyType.GetDefault();

					if (!Equals(otherEntityId, defaultValue) && Repositories.ContainsKey(entityRelationship.PropertyType.ToAssemblyName()))
					{
						var repository = Repositories[entityRelationship.PropertyType.ToAssemblyName()];
						otherEntity = (IEntity) repository.Read(otherEntityId);
						entityRelationship.SetValue(entity, otherEntity, null);
					}
				}
				else if (otherEntity != null && entityRelationshipIdProperty != null)
				{
					var repository = Repositories[entityRelationship.PropertyType.ToAssemblyName()];
					var repositoryType = repository.GetType();

					// Check to see if this is a new child entity.
					if (!otherEntity.IdIsSet())
					{
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
					else
					{
						// Check to see if the entity already exists.
						var exists = (bool) repositoryType.CachedGetMethod("Contains").Invoke(repository, new object[] { otherEntity });
						if (!exists)
						{
							repositoryType.CachedGetMethod("Add", otherEntity.GetType()).Invoke(repository, new object[] { otherEntity });
						}
					}

					var otherEntityProperties = otherEntity.GetType().GetCachedProperties();
					var otherEntityIdProperty = otherEntityProperties.FirstOrDefault(x => x.Name == "Id");
					var entityId = entityRelationshipIdProperty.GetValue(entity, null);
					var otherId = otherEntityIdProperty?.GetValue(otherEntity);

					if (!Equals(entityId, otherId))
					{
						// resets entityId to entity.Id if it does not match
						entityRelationshipIdProperty.SetValue(entity, otherId, null);
					}

					var entityRelationshipSyncIdProperty = entityProperties.FirstOrDefault(x => x.Name == entityRelationship.Name + "SyncId");

					if (entityRelationship.GetValue(entity, null) is ISyncEntity syncEntity && entityRelationshipSyncIdProperty != null)
					{
						var otherEntitySyncId = (Guid?) entityRelationshipSyncIdProperty.GetValue(entity, null);
						if (otherEntitySyncId != syncEntity.SyncId)
						{
							// resets entitySyncId to entity.SyncId if it does not match
							entityRelationshipSyncIdProperty.SetValue(entity, syncEntity.SyncId, null);
						}
					}
				}
			}
		}

		private void UpdateEntityRelationships(IEntity entity)
		{
			UpdateEntityDirectRelationships(entity);
			UpdateEntityCollectionRelationships(entity);
		}

		private void ValidateEntity<T, T2>(T entity, IRepository<T, T2> repository) where T : Entity<T2>
		{
			if (Options.DisableEntityValidations)
			{
				return;
			}

			foreach (var configuration in PropertyConfigurations.Values.Where(x => x.IsMappingFor(entity)))
			{
				if (configuration is PropertyConfiguration<T, T2> validation)
				{
					validation.Validate(entity, repository);
				}
			}
		}

		#endregion

		#region Events

		/// <inheritdoc />
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion
	}
}