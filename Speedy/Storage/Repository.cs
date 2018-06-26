#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Speedy.Sync;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type contained in the repository. </typeparam>
	/// <typeparam name="T2"> The type of the entity key. </typeparam>
	[Serializable]
	internal class Repository<T, T2> : IRepository, IRepository<T, T2> where T : Entity<T2>, new()
	{
		#region Fields

		protected readonly IList<EntityState<T, T2>> Cache;
		protected readonly IKeyValueRepository<T> Store;
		private T2 _currentKey;
		private readonly Database _database;
		private readonly IQueryable<T> _query;
		private readonly Type _type;

		#endregion

		#region Constructors

		public Repository(Database database)
		{
			_database = database;
			Cache = new List<EntityState<T, T2>>(4096);
			_type = typeof(T);
			_currentKey = default(T2);

			if (!string.IsNullOrWhiteSpace(_database.Directory))
			{
				var options = new KeyValueRepositoryOptions { IgnoreVirtualMembers = true, Timeout = database.Options.Timeout, Limit = int.MaxValue };
				Store = KeyValueRepository<T>.Create(_database.Directory, typeof(T).Name, options);
				Store.OnEnumerated += OnUpdateEntityRelationships;
			}

			_query = Store?.Select(x => Cache.FirstOrDefault(y => Equals(y.Entity.Id, x.Id))?.Entity ?? AddOrUpdateCache(x)).AsQueryable()
				?? Cache.Where(x => x.State != EntityStateType.Added).Select(x => x.Entity).AsQueryable();
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

			Cache.Add(new EntityState<T, T2> { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
			OnUpdateEntityRelationships(entity);
		}

		/// <summary>
		/// Adds or updates an entity in the repository. The ID of the entity must be the default value to add and a value to
		/// update.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		public void AddOrUpdate(T entity)
		{
			if (!entity.IdIsSet())
			{
				Add(entity);
				return;
			}

			var foundItem = Cache.FirstOrDefault(x => Equals(x.Entity.Id, entity.Id));
			if (foundItem == null)
			{
				Cache.Add(new EntityState<T, T2> { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Unmodified });
				OnUpdateEntityRelationships(entity);
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

		/// <inheritdoc />
		public void AssignKey(IEntity entity, List<IEntity> processed)
		{
			if (processed?.Contains(entity) == true)
			{
				return;
			}

			var item = entity as Entity<T2>;
			if (item == null)
			{
				throw new ArgumentException("Entity is not for this repository.");
			}

			//_database.UpdateDependantIds(item, processed ?? new List<IEntity>());

			if (!item.IdIsSet())
			{
				var id = item.NewId(ref _currentKey);
				if (!Equals(id, default(T2)))
				{
					item.Id = id;
				}
			}

			var syncableEntity = entity as SyncEntity;
			if (syncableEntity == null)
			{
				return;
			}

			var maintainedEntity = _database.Options.UnmaintainEntities.All(x => x != entity.GetType());
			var maintainSyncId = maintainedEntity && _database.Options.MaintainSyncId;

			if (maintainSyncId && syncableEntity.SyncId == Guid.Empty)
			{
				syncableEntity.SyncId = Guid.NewGuid();
			}
		}

		/// <inheritdoc />
		public void AssignKeys(List<IEntity> processed)
		{
			foreach (var entityState in Cache)
			{
				AssignKey(entityState.Entity, processed);
			}
		}

		/// <inheritdoc />
		public int DiscardChanges()
		{
			var response = Cache.Count;
			Cache.Clear();
			return response;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (Store != null)
			{
				Cache.Clear();
				Store?.Dispose();
			}
			else
			{
				Cache.Where(x => x.State == EntityStateType.Added)
					.ToList()
					.ForEach(x => Cache.Remove(x));

				Cache.ForEach(x =>
				{
					UpdateEntity(x.Entity, x.OldEntity);
					x.State = EntityStateType.Unmodified;
				});
			}
		}

		/// <inheritdoc />
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
				.Select(x => x.Entity)
				.Where(filter)
				.AsQueryable();
		}

		/// <inheritdoc />
		public bool HasChanges()
		{
			return GetChanges().Any();
		}

		public bool HasDependentRelationship(object[] value, object id)
		{
			var foreignKeyFunction = (Func<T, object>) value[4];
			return this.Any(x => id.Equals(foreignKeyFunction.Invoke(x)));
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include)
		{
			return _query.Include(include);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes)
		{
			return (IIncludableQueryable<T, T3>) includes.Aggregate(_query, (current, include) => current.Include(include));
		}

		/// <summary>
		/// Initialize the repository.
		/// </summary>
		public void Initialize()
		{
			var lastItem = Store?.LastOrDefault();

			if (lastItem != null)
			{
				_currentKey = lastItem.Id;
			}
		}

		/// <summary>
		/// Insert an entity to the repository before the provided entity. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		/// <param name="targetEntity"> The entity to locate insert point. </param>
		public void InsertBefore(T entity, T targetEntity)
		{
			if (Cache.Any(x => entity == x.Entity))
			{
				return;
			}

			var state = Cache.FirstOrDefault(x => x.Entity == targetEntity);
			var indexOf = Cache.IndexOf(state);

			if (indexOf < 0)
			{
				throw new ArgumentException("Could not find the target entity", nameof(targetEntity));
			}

			Cache.Insert(indexOf, new EntityState<T, T2> { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
		}

		/// <inheritdoc />
		public object Read(object id)
		{
			return Read((T2) id);
		}

		/// <summary>
		/// Get entity by ID.
		/// </summary>
		/// <param name="id"> </param>
		/// <returns> The entity or null. </returns>
		public T Read(T2 id)
		{
			var state = Cache.FirstOrDefault(x => Equals(x.Entity.Id, id));
			return state == null ? Store?.Read(id.ToString()) : state.Entity;
		}

		/// <inheritdoc />
		public void Remove(T2 id)
		{
			var entity = Cache.FirstOrDefault(x => Equals(x.Entity.Id, id));

			if (entity == null)
			{
				entity = new EntityState<T, T2> { Entity = new T { Id = id } };
				Cache.Add(entity);
			}

			entity.State = EntityStateType.Removed;
		}

		/// <inheritdoc />
		public void Remove(T entity)
		{
			Remove(entity.Id);
		}

		/// <inheritdoc />
		public void Remove(Expression<Func<T, bool>> filter)
		{
			Cache.Select(x => x.Entity)
				.Where(filter.Compile())
				.ForEach(Remove);
		}

		/// <inheritdoc />
		public int SaveChanges()
		{
			var changeCount = GetChanges().Count();
			var removed = Cache.Where(x => x.State == EntityStateType.Removed).ToList();

			foreach (var item in removed)
			{
				var syncableEntity = item.Entity as SyncEntity;
				if (syncableEntity != null)
				{
					if (syncableEntity.SyncId == Guid.Empty)
					{
						throw new InvalidOperationException("Cannot tombstone this entity because the sync ID has not been set.");
					}

					_database.SyncTombstones?.Add(syncableEntity.ToSyncTombstone(_database.Options.SyncTombstoneReferenceId));
				}

				Store?.Remove(item.Entity.Id.ToString());
				Cache.Remove(item);
			}

			foreach (var entry in Cache)
			{
				var entity = entry.Entity;
				var createdEntity = entity as ICreatedEntity;
				var modifiableEntity = entity as IModifiableEntity;
				var syncableEntity = entity as SyncEntity;
				var maintainedEntity = _database.Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
				var maintainDates = maintainedEntity && _database.Options.MaintainDates;
				var maintainSyncId = maintainedEntity && _database.Options.MaintainSyncId;
				var now = DateTime.UtcNow;

				switch (entry.State)
				{
					case EntityStateType.Added:
						entity.EntityAdded();
						if (createdEntity != null && maintainDates)
						{
							createdEntity.CreatedOn = now;
						}

						if (modifiableEntity != null && maintainDates)
						{
							modifiableEntity.ModifiedOn = now;
						}
						break;

					case EntityStateType.Modified:
						entity.EntityModified();

						if (createdEntity != null && maintainDates)
						{
							var oldCreatedEntity = entry.OldEntity as ICreatedEntity;
							if (oldCreatedEntity != null && oldCreatedEntity.CreatedOn != createdEntity.CreatedOn)
							{
								createdEntity.CreatedOn = oldCreatedEntity.CreatedOn;
							}
						}

						if (syncableEntity != null)
						{
							// Do not allow sync ID to change for entities.
							if (maintainSyncId)
							{
								var oldSyncableEntity = entry.OldEntity as SyncEntity;
								if (oldSyncableEntity != null)
								{
									syncableEntity.SyncId = oldSyncableEntity.SyncId;
								}
							}
						}

						if (modifiableEntity != null && maintainDates)
						{
							modifiableEntity.ModifiedOn = now;
						}
						break;
				}

				if (Store != null)
				{
					Store?.Write(entry.Entity.Id.ToString(), entry.Entity);
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

		public void UpdateLocalSyncIds()
		{
			foreach (var entry in Cache)
			{
				// The local relationships may have changed. We need keep our sync IDs in sync with 
				// any relationships that may have changed.
				(entry.Entity as SyncEntity)?.UpdateLocalSyncIds();
			}
		}

		public void UpdateRelationships()
		{
			Cache.ToList().ForEach(x => OnUpdateEntityRelationships(x.Entity));
		}

		/// <inheritdoc />
		public void ValidateEntities()
		{
			Cache.Where(x => x.State == EntityStateType.Added || x.State == EntityStateType.Modified)
				.ToList()
				.ForEach(x => OnValidateEntity(x.Entity));

			Cache.Where(x => x.State == EntityStateType.Removed)
				.ToList()
				.ForEach(x => OnDeletingEntity(x.Entity));
		}

		protected virtual void OnDeletingEntity(T obj)
		{
			var handler = DeletingEntity;
			handler?.Invoke(obj);
		}

		protected virtual void OnUpdateEntityRelationships(T obj)
		{
			var handler = UpdateEntityRelationships;
			handler?.Invoke(obj);
		}

		protected virtual void OnValidateEntity(T obj)
		{
			var handler = ValidateEntity;
			handler?.Invoke(obj, this);
		}

		private T AddOrUpdateCache(T entity)
		{
			AddOrUpdate(entity);
			return entity;
		}

		private T CloneEntity(T entity)
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
			var collectionRelationships = _type.GetCachedProperties()
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
				if (currentCollectionType.Name == typeof(RelationshipRepository<T, T2>).Name)
				{
					relationship.SetValue(response, currentCollection, null);
				}
			}

			return response;
		}

		private bool CompareEntity(T entity1, T entity2)
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

		private IEnumerable<EntityState<T, T2>> GetChanges()
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

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<PropertyInfo> GetPublicProperties()
		{
			return _type.GetCachedProperties()
				.Where(x =>
				{
					if (x.Name == "Id")
					{
						return true;
					}

					var accessors = x.GetAccessors();
					if (accessors.Length < 2)
					{
						return false;
					}

					if (accessors.Any(a => (a.IsVirtual || !a.IsPublic) && !a.IsFinal))
					{
						return false;
					}

					return true;
				})
				.ToList();
		}

		/// <summary>
		/// Update the entity with the new values.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="updatedEntity"> The new values to update the entity with. </param>
		private void UpdateEntity(Entity<T2> entity, Entity<T2> updatedEntity)
		{
			var properties = GetPublicProperties();
			foreach (var property in properties)
			{
				property.SetValue(entity, property.GetValue(updatedEntity, null), null);
			}
		}

		#endregion

		#region Events

		public event Action<T> DeletingEntity;
		public event Action<T> UpdateEntityRelationships;
		public event Action<T, IRepository<T, T2>> ValidateEntity;

		#endregion
	}
}