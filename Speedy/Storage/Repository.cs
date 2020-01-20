#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Speedy.Exceptions;
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
	internal class Repository<T, T2> : IDatabaseRepository, IRepository<T, T2> where T : Entity<T2>
	{
		#region Fields

		internal IList<EntityState<T, T2>> Cache;
		private T2 _currentKey;
		private IQueryable<T> _query;
		private readonly Type _type;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a repository for the provided database.
		/// </summary>
		/// <param name="database"> The database this repository is for. </param>
		public Repository(Database database)
		{
			_type = typeof(T);
			_currentKey = default;

			Cache = new List<EntityState<T, T2>>(4096);
			Database = database;

			UpdateCacheQuery();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Will keep the repository items in cache for the life cycle of the repository.
		/// </summary>
		public bool NeverClearCache { get; set; }

		/// <summary>
		/// The database this repository is for.
		/// </summary>
		protected Database Database { get; }

		/// <inheritdoc />
		Type IQueryable.ElementType => _query.ElementType;

		/// <inheritdoc />
		Expression IQueryable.Expression => _query.Expression;

		/// <inheritdoc />
		IQueryProvider IQueryable.Provider => _query.Provider;

		#endregion

		#region Methods

		/// <inheritdoc />
		public void Add(T entity)
		{
			if (Cache.Any(x => entity == x.Entity))
			{
				return;
			}

			var duplicateId = Cache.FirstOrDefault(x => !x.Entity.Id.Equals(default(T2)) && Equals(x.Entity.Id, entity.Id));
			if (duplicateId != null)
			{
				throw new InvalidOperationException($"The instance of entity type '{typeof(T).Name}' cannot be tracked because another instance with the same key value is already being tracked.");
			}

			Cache.Add(new EntityState<T, T2>(entity, CloneEntity(entity), EntityStateType.Added));
			OnUpdateEntityRelationships(entity);
		}

		/// <inheritdoc />
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
				Cache.Add(new EntityState<T, T2>(entity, CloneEntity(entity), EntityStateType.Unmodified));
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
			if (!(entity is T myEntity))
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

			if (!(entity is Entity<T2> item))
			{
				throw new ArgumentException("Entity is not for this repository.");
			}

			if (!item.IdIsSet())
			{
				var id = item.NewId(ref _currentKey);
				if (!Equals(id, default(T2)))
				{
					item.Id = id;
				}
			}

			if (!(entity is ISyncEntity syncableEntity))
			{
				return;
			}

			var maintainedEntity = Database.Options.UnmaintainEntities.All(x => x != entity.GetType());
			var maintainSyncId = maintainedEntity && Database.Options.MaintainSyncId;

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
		public int BulkRemove(Expression<Func<T, bool>> filter)
		{
			var count = 0;

			foreach (var item in this.Where(filter))
			{
				Remove(item);
				count++;
			}

			// Temporarily mark the permanently delete flag
			var value = Database.Options.PermanentSyncEntityDeletions;
			Database.Options.PermanentSyncEntityDeletions = true;

			SaveChanges();

			// Restore the previous permanently delete flag
			Database.Options.PermanentSyncEntityDeletions = value;

			return count;
		}

		/// <inheritdoc />
		public int BulkUpdate(Expression<Func<T, bool>> filter, Expression<Func<T, T>> update)
		{
			var count = 0;

			foreach (var item in this.Where(filter))
			{
				var updated = false;

				switch (update.Body)
				{
					case MemberInitExpression memberInitExpression:
					{
						for (var index = 0; index < memberInitExpression.Bindings.Count; index++)
						{
							var binding = memberInitExpression.Bindings[index];
							if (!(binding is MemberAssignment assignment))
							{
								continue;
							}

							var name = assignment.Member.Name;
							item.SetMemberValue(name, Expression.Lambda(assignment.Expression).Compile().DynamicInvoke());
							updated = true;
						}
						break;
					}
				}

				if (updated)
				{
					count++;
				}
			}

			return count;
		}

		/// <summary>
		/// Check to see if the repository contains this entity.
		/// </summary>
		/// <param name="entity"> The entity to test for. </param>
		/// <returns> True if the entity exist or false it otherwise. </returns>
		public bool Contains(T entity)
		{
			return Cache.Any(x => entity == x.Entity) || _query.Any(x => Equals(x.Id, entity.Id));
		}

		/// <inheritdoc />
		public int DiscardChanges()
		{
			var response = Cache.Count;
			ResetCache();
			return response;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			ResetCache();
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

		/// <inheritdoc />
		public bool HasDependentRelationship(object[] value, object id)
		{
			var foreignKeyFunction = (Func<T, object>) value[4];
			return this.Any(x => id.Equals(foreignKeyFunction.Invoke(x)));
		}

		public IIncludableQueryable<T, object> Include(Expression<Func<T, object>> include)
		{
			return new IncludableQueryable<T, object>(_query);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, T3> Include<T3>(Expression<Func<T, T3>> include)
		{
			return new IncludableQueryable<T, T3>(_query);
		}

		/// <inheritdoc />
		public IIncludableQueryable<T, object> Including(params Expression<Func<T, object>>[] includes)
		{
			return new IncludableQueryable<T, object>(_query);
		}

		public IIncludableQueryable<T, T3> Including<T3>(params Expression<Func<T, T3>>[] includes)
		{
			return new IncludableQueryable<T, T3>(_query);
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

			Cache.Insert(indexOf, new EntityState<T, T2>(entity, CloneEntity(entity), EntityStateType.Added));
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
			return state?.Entity;
		}

		/// <inheritdoc />
		public void Remove(T2 id)
		{
			var entity = Cache.FirstOrDefault(x => Equals(x.Entity.Id, id));

			if (entity == null)
			{
				var instance = Activator.CreateInstance<T>();
				instance.Id = id;
				entity = new EntityState<T, T2>(instance, CloneEntity(instance), EntityStateType.Removed);
				Cache.Add(entity);
			}

			entity.State = EntityStateType.Removed;
		}

		/// <inheritdoc />
		public void Remove(T entity)
		{
			Remove(entity.Id);
			ValidateEntities();
		}

		/// <inheritdoc />
		public void Remove(Expression<Func<T, bool>> filter)
		{
			Cache.Select(x => x.Entity)
				.Where(filter.Compile())
				.ForEach(Remove);
		}

		/// <inheritdoc />
		public void RemoveDependent(object[] value, object id)
		{
			var foreignKeyFunction = (Func<T, object>) value[4];
			Remove(x => id.Equals(foreignKeyFunction.Invoke(x)));
		}

		/// <inheritdoc />
		public int SaveChanges()
		{
			var changeCount = GetChanges().Count();
			var added = Cache.Where(x => x.State == EntityStateType.Added).ToList();
			var removed = Cache.Where(x => x.State == EntityStateType.Removed).ToList();
			var now = TimeService.UtcNow;

			foreach (var item in removed)
			{
				if (item.Entity is ISyncEntity syncableEntity)
				{
					if (syncableEntity.SyncId == Guid.Empty)
					{
						throw new InvalidOperationException("Cannot tombstone this entity because the sync ID has not been set.");
					}

					if (!Database.Options.PermanentSyncEntityDeletions)
					{
						syncableEntity.IsDeleted = true;
						syncableEntity.ModifiedOn = now;
						item.State = EntityStateType.Modified;
						continue;
					}
				}

				Cache.Remove(item);

				item.Entity.EntityDeleted();
			}

			foreach (var entry in Cache.ToList())
			{
				var entity = entry.Entity;
				var createdEntity = entity as ICreatedEntity;
				var modifiableEntity = entity as IModifiableEntity;
				var syncableEntity = entity as ISyncEntity;
				var maintainedEntity = Database.Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
				var maintainCreatedOnDate = maintainedEntity && Database.Options.MaintainCreatedOn;
				var maintainModifiedOnDate = maintainedEntity && Database.Options.MaintainModifiedOn;
				var maintainSyncId = maintainedEntity && Database.Options.MaintainSyncId;

				switch (entry.State)
				{
					case EntityStateType.Added:
						if (createdEntity != null && maintainCreatedOnDate)
						{
							createdEntity.CreatedOn = now;
						}

						if (modifiableEntity != null && maintainModifiedOnDate)
						{
							modifiableEntity.ModifiedOn = now;
						}

						entity.EntityAdded();
						break;

					case EntityStateType.Modified:
						if (!entity.CanBeModified())
						{
							UpdateEntity(entry.Entity, entry.OldEntity);
							entry.State = EntityStateType.Unmodified;
							changeCount--;
							continue;
						}

						if (createdEntity != null && maintainCreatedOnDate)
						{
							if (entry.OldEntity is ICreatedEntity oldCreatedEntity && oldCreatedEntity.CreatedOn != createdEntity.CreatedOn)
							{
								createdEntity.CreatedOn = oldCreatedEntity.CreatedOn;
							}
						}

						if (syncableEntity != null)
						{
							// Do not allow sync ID to change for entities.
							if (maintainSyncId)
							{
								if (entry.OldEntity is ISyncEntity oldSyncableEntity)
								{
									syncableEntity.SyncId = oldSyncableEntity.SyncId;
								}
							}
						}

						if (modifiableEntity != null && maintainModifiedOnDate)
						{
							modifiableEntity.ModifiedOn = now;
						}

						entity.EntityModified();
						break;
				}

				UpdateEntity(entry.OldEntity, entry.Entity);
				entry.State = EntityStateType.Unmodified;
			}

			OnCollectionChanged(added.Select(x => x.Entity).ToList(), removed.Where(x => x.State == EntityStateType.Removed).Select(x => x.Entity).ToList());

			return changeCount;
		}

		/// <inheritdoc />
		public void SetDependentToNull(object[] value, object id)
		{
			var entityExpression = (LambdaExpression) value[1];
			var foreignKeyExpression = (LambdaExpression) value[3];
			var foreignKeyFunction = (Func<T, object>) value[4];

			var values = this.Where(x => foreignKeyFunction.Invoke(x) == id);
			var entityName = entityExpression.GetExpressionName();
			var foreignKeyName = foreignKeyExpression.GetExpressionName();

			foreach (var v in values)
			{
				v.SetMemberValue(entityName, null);
				v.SetMemberValue(foreignKeyName, null);
			}
		}

		/// <inheritdoc />
		public void Sort()
		{
			Cache = Cache.OrderBy(x => x.Entity.Id).ToList();
			UpdateCacheQuery();
		}

		/// <summary>
		/// Sorts the repository by the provide key.
		/// </summary>
		public void Sort(Func<T, object> order)
		{
			Cache = Cache.OrderBy(x => order(x.Entity)).ToList();
			UpdateCacheQuery();
		}

		/// <inheritdoc />
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

			Cache.Where(x => x.State == EntityStateType.Added)
				.ToList()
				.ForEach(x => OnAddingEntity(x.Entity));

			Cache.Where(x => x.State == EntityStateType.Removed)
				.ToList()
				.ForEach(x => OnDeletingEntity(x.Entity));
		}

		/// <summary>
		/// Occurs when an entity is being deleted.
		/// </summary>
		/// <param name="obj"> The entity that was deleted. </param>
		protected virtual void OnAddingEntity(T obj)
		{
			var handler = AddingEntity;
			handler?.Invoke(obj);
		}

		/// <summary>
		/// Occurs when an entity is being deleted.
		/// </summary>
		/// <param name="obj"> The entity that was deleted. </param>
		protected virtual void OnDeletingEntity(T obj)
		{
			var handler = DeletingEntity;
			handler?.Invoke(obj);
		}

		/// <summary>
		/// Occurs when an entity relationships are updated.
		/// </summary>
		/// <param name="obj"> The entity that was updated. </param>
		protected virtual void OnUpdateEntityRelationships(T obj)
		{
			UpdateEntityRelationships?.Invoke(obj);
		}

		/// <summary>
		/// Occurs when an entity is validated.
		/// </summary>
		/// <param name="obj"> The entity that was validated. </param>
		protected virtual void OnValidateEntity(T obj)
		{
			var handler = ValidateEntity;
			handler?.Invoke(obj, this);
		}

		internal bool AnyNew(object entity, Func<T, bool> func)
		{
			return Cache.Any(x => !ReferenceEquals(x.Entity, entity) && func(x.OldEntity));
		}

		private T CloneEntity(T entity)
		{
			var constructorInfo = _type.GetConstructor(new Type[0]);
			if (constructorInfo == null)
			{
				throw new SpeedyException("Failed to create new instance...");
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

		private void OnCollectionChanged(IList added, IList removed)
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

		private void ResetCache()
		{
			Cache.Where(x => x.State == EntityStateType.Added)
				.ToList()
				.ForEach(x => Cache.Remove(x));

			Cache.ToList()
				.ForEach(x =>
				{
					UpdateEntity(x.Entity, x.OldEntity);
					x.State = EntityStateType.Unmodified;
				});
		}

		private void UpdateCacheQuery()
		{
			_query = Cache.Where(x => x.State != EntityStateType.Added).Select(x => x.Entity).AsQueryable();
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

		/// <summary>
		/// Occurs when an entity is being added.
		/// </summary>
		public event Action<T> AddingEntity;

		/// <inheritdoc />
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Occurs when an entity is being deleted.
		/// </summary>
		public event Action<T> DeletingEntity;

		/// <summary>
		/// Occurs when an entity relationships are updated.
		/// </summary>
		public event Action<T> UpdateEntityRelationships;

		/// <summary>
		/// Occurs when an entity is being validated.
		/// </summary>
		public event Action<T, IRepository<T, T2>> ValidateEntity;

		#endregion
	}
}