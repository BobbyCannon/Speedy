#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Speedy.Sync;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type contained in the repository. </typeparam>
	[Serializable]
	internal class Repository<T> : IRepository, IRepository<T> where T : Entity, new()
	{
		#region Fields

		protected readonly IList<EntityState> Cache;
		protected readonly IKeyValueRepository<T> Store;
		private readonly Database _database;
		private int _index;
		private readonly IQueryable<T> _query;
		private readonly Type _type;

		#endregion

		#region Constructors

		public Repository(Database database)
		{
			_database = database;
			Cache = new List<EntityState>(4096);
			_index = 0;
			_type = typeof(T);

			if (!string.IsNullOrWhiteSpace(_database.FilePath))
			{
				var options = new KeyValueRepositoryOptions { IgnoreVirtualMembers = true };
				Store = KeyValueRepository<T>.Create(_database.FilePath, typeof(T).Name, options);
				Store.OnEnumerated += OnUpdateEntityRelationships;
			}

			_query = Store?.Select(x => (T) Cache.FirstOrDefault(y => y.Entity.Id == x.Id)?.Entity ?? AddOrUpdateCache(x)).AsQueryable()
				?? Cache.Where(x => x.State != EntityStateType.Added).Select(x => (T) x.Entity).AsQueryable();
		}

		#endregion

		#region Properties

		Type IQueryable.ElementType => _query.ElementType;

		Expression IQueryable.Expression => _query.Expression;

		IQueryProvider IQueryable.Provider => _query.Provider;

		#endregion

		#region Methods

		/// <summary>
		/// Add an entity to the repository. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void Add(T entity)
		{
			if (Cache.Any(x => entity == x.Entity))
			{
				return;
			}

			Cache.Add(new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(T entity)
		{
			if (entity.Id == 0)
			{
				Add(entity);
				return;
			}

			var foundItem = Cache.FirstOrDefault(x => x.Entity.Id == entity.Id);
			if (foundItem == null)
			{
				Cache.Add(new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Unmodified });
				return;
			}

			UpdateEntity(foundItem.Entity, entity);
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(object entity)
		{
			var myEntity = entity as T;
			if (myEntity == null)
			{
				throw new ArgumentException("The entity is not the correct type.");
			}

			AddOrUpdate(myEntity);
		}

		/// <summary>
		/// Assign primary keys to all entities.
		/// </summary>
		public void AssignKeys()
		{
			foreach (var entityState in Cache.Where(entityState => entityState.Entity.Id == 0))
			{
				entityState.Entity.Id = Interlocked.Increment(ref _index);
			}
		}

		/// <summary>
		/// Dispose of the entity store and cleans up all dependencies.
		/// </summary>
		public void Dispose()
		{
			if (Store != null)
			{
				Cache.Clear();
				Store?.Dispose();
			}
			else
			{
				Cache.Where(x => x.State == EntityStateType.Added).ToList()
					.ForEach(x => Cache.Remove(x));

				Cache.ForEach(x =>
				{
					UpdateEntity(x.Entity, x.OldEntity);
					x.State = EntityStateType.Unmodified;
				});
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		/// <filterpriority> 1 </filterpriority>
		public IEnumerator<T> GetEnumerator()
		{
			return _query.GetEnumerator();
		}

		/// <summary>
		/// Returns a raw queryable.
		/// </summary>
		/// <param name="filter"> </param>
		/// <returns> </returns>
		public IQueryable<T> GetRawQueryable(Func<T, bool> filter)
		{
			return Cache
				.Select(x => (T) x.Entity)
				.Where(filter)
				.AsQueryable();
		}

		/// <summary>
		/// Determines if the repository has changes.
		/// </summary>
		/// <returns> </returns>
		public bool HasChanges()
		{
			return GetChanges().Any();
		}

		/// <summary>
		/// Configures the query to include related entities in the results.
		/// </summary>
		/// <param name="include"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IQueryable<T> Include(Expression<Func<T, object>> include)
		{
			return _query;
		}

		/// <summary>
		/// Configures the query to include multiple related entities in the results.
		/// </summary>
		/// <param name="includes"> The related entities to include. </param>
		/// <returns> The results of the query including the related entities. </returns>
		public IQueryable<T> Including(params Expression<Func<T, object>>[] includes)
		{
			return _query;
		}

		/// <summary>
		/// Initialize the repository.
		/// </summary>
		public void Initialize()
		{
			var keys = Store?.ReadKeys().ToList();
			_index = keys?.Count > 0 ? keys.Max(int.Parse) : 0;
		}

		/// <summary>
		/// Insert an entity to the repository before the provided entity. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		/// <param name="targetEntity"> The entity to locate insert point. </param>
		public void InsertBefore(T entity, T targetEntity)
		{
			var state = Cache.FirstOrDefault(x => x.Entity == targetEntity);
			var indexOf = Cache.IndexOf(state);

			if (indexOf < 0)
			{
				throw new ArgumentException("Could not find the target entity", nameof(targetEntity));
			}

			Cache.Insert(indexOf, new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
		}

		/// <summary>
		/// Get entity by ID.
		/// </summary>
		/// <param name="id"> </param>
		/// <returns> The entity or null. </returns>
		public Entity Read(int id)
		{
			var state = Cache.FirstOrDefault(x => x.Entity.Id == id);
			return state == null ? Store?.Read(id.ToString()) : state.Entity;
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(int id)
		{
			var entity = Cache.FirstOrDefault(x => x.Entity.Id == id);

			if (entity == null)
			{
				entity = new EntityState { Entity = new T { Id = id } };
				Cache.Add(entity);
			}

			entity.State = EntityStateType.Removed;
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="entity"> The entity to remove. </param>
		public void Remove(T entity)
		{
			Remove(entity.Id);
		}

		/// <summary>
		/// Removes a set of entities from the repository.
		/// </summary>
		/// <param name="filter"> The filter of the entities to remove. </param>
		public void RemoveRange(Expression<Func<T, bool>> filter)
		{
			Cache.Select(x => x.Entity)
				.Cast<T>()
				.Where(filter.Compile())
				.ForEach(Remove);
		}

		public int SaveChanges()
		{
			var changeCount = GetChanges().Count();
			var removed = Cache.Where(x => x.State == EntityStateType.Removed).ToList();

			foreach (var item in removed)
			{
				var syncableEntity = item.Entity as SyncEntity;
				if (syncableEntity != null)
				{
					_database.SyncTombstones?.Add(syncableEntity.ToSyncTombstone());
				}

				Store?.Remove(item.Entity.Id.ToString());
				Cache.Remove(item);
			}

			foreach (var entry in Cache)
			{
				var entity = entry.Entity;
				var modifiableEntity = entity as ModifiableEntity;
				var syncableEntity = entity as SyncEntity;

				switch (entry.State)
				{
					case EntityStateType.Added:
						if (_database.Options.MaintainDates)
						{
							// Make sure the modified on value matches created on for new items.
							entity.CreatedOn = DateTime.UtcNow;
						}

						if (syncableEntity != null)
						{
							if (_database.Options.MaintainSyncId && syncableEntity.SyncId == Guid.Empty)
							{
								syncableEntity.SyncId = Guid.NewGuid();
							}
						}

						if (modifiableEntity != null && _database.Options.MaintainDates)
						{
							modifiableEntity.ModifiedOn = entity.CreatedOn;
						}
						break;

					case EntityStateType.Modified:
						if (_database.Options.MaintainDates)
						{
							// Do not allow created on to change for entities.
							entity.CreatedOn = entry.OldEntity.CreatedOn;
						}

						if (syncableEntity != null)
						{
							// Do not allow sync ID to change for entities.
							if (_database.Options.MaintainSyncId)
							{
								syncableEntity.SyncId = ((SyncEntity) entry.OldEntity).SyncId;
							}
						}

						if (modifiableEntity != null && _database.Options.MaintainDates)
						{
							// Update modified to now for new entities.
							modifiableEntity.ModifiedOn = DateTime.UtcNow;
						}
						break;
				}

				if (Store != null)
				{
					Store?.Write(entry.Entity.Id.ToString(), (T) entry.Entity);
				}
				else
				{
					entry.OldEntity = CloneEntity(entry.Entity);
					entry.State = EntityStateType.Unmodified;
				}
			}

			if (Store != null)
			{
				Store.Save();
				Store.Flush();
				Cache.Clear();
			}

			return changeCount;
		}

		public void UpdateRelationships()
		{
			Cache.ToList().ForEach(x => OnUpdateEntityRelationships((T) x.Entity));
		}

		public void UpdateLocalSyncIds()
		{
			foreach (var entry in Cache)
			{
				// The local relationships may have changed. We need keep our sync IDs in sync with 
				// any relationships that may have changed.
				(entry.Entity as SyncEntity)?.UpdateLocalSyncIds();
			}
		}

		public void ValidateEntities()
		{
			Cache.Where(x => x.State == EntityStateType.Added || x.State == EntityStateType.Modified)
				.ToList()
				.ForEach(x => OnValidateEntity((T) x.Entity));
		}

		protected virtual void OnUpdateEntityRelationships(T obj)
		{
			var handler = UpdateEntityRelationships;
			handler?.Invoke(obj);
		}

		protected virtual void OnValidateEntity(T obj)
		{
			var handler = ValidateEntity;
			handler?.Invoke(obj);
		}

		private T AddOrUpdateCache(T entity)
		{
			AddOrUpdate(entity);
			return entity;
		}

		private Entity CloneEntity(Entity entity)
		{
			var constructorInfo = _type.GetConstructor(new Type[0]);
			if (constructorInfo == null)
			{
				throw new Exception("Failed to create new instance...");
			}

			var response = (T) constructorInfo.Invoke(null);
			var properties = GetPublicProperties().ToList();

			foreach (var property in properties)
			{
				var value = property.GetValue(entity, null);
				property.SetValue(response, value, null);
			}

			var enumerableType = typeof(IEnumerable);
			var collectionRelationships = _type.GetProperties()
				.Where(x => x.GetAccessors()[0].IsVirtual)
				.Where(x => enumerableType.IsAssignableFrom(x.PropertyType))
				.Where(x => x.PropertyType.IsGenericType)
				.ToList();

			foreach (var relationship in collectionRelationships)
			{
				var currentCollection = relationship.GetValue(entity, null);
				if (currentCollection == null)
				{
					continue;
				}

				var currentCollectionType = currentCollection.GetType();
				if (currentCollectionType.Name == typeof(RelationshipRepository<>).Name)
				{
					relationship.SetValue(response, currentCollection, null);
				}
			}

			return response;
		}

		private bool CompareEntity(Entity entity1, Entity entity2)
		{
			if (entity1 == null && entity2 == null)
			{
				return true;
			}

			if (entity1 == null || entity2 == null)
			{
				return false;
			}

			var properties = GetPublicProperties();
			foreach (var property in properties)
			{
				var value1 = property.GetValue(entity1, null);
				var value2 = property.GetValue(entity2, null);
				if (!Equals(value1, value2))
				{
					return false;
				}
			}

			return true;
		}

		private IEnumerable<EntityState> GetChanges()
		{
			foreach (var item in Cache.Where(x => x.State == EntityStateType.Unmodified))
			{
				if (!CompareEntity(item.Entity, item.OldEntity))
				{
					item.State = EntityStateType.Modified;
				}
			}

			return Cache.Where(x => x.State != EntityStateType.Unmodified).ToList();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<PropertyInfo> GetPublicProperties()
		{
			return _type.GetProperties()
				.Where(x =>
				{
					var accessors = x.GetAccessors();
					if (accessors.Length < 2)
					{
						return false;
					}

					if (accessors.Any(a => a.IsVirtual || !a.IsPublic))
					{
						return false;
					}

					return true;
				})
				.ToList();
		}

		private void UpdateEntity(Entity entity, Entity updatedEntity)
		{
			var properties = GetPublicProperties();
			foreach (var property in properties)
			{
				property.SetValue(entity, property.GetValue(updatedEntity, null), null);
			}
		}

		#endregion

		#region Events

		public event Action<T> UpdateEntityRelationships;
		public event Action<T> ValidateEntity;

		#endregion
	}
}