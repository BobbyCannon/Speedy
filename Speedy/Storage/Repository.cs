#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// This collection servers as an in memory repository for unit testing.
	/// </summary>
	/// <typeparam name="T"> The type contained in the repository. </typeparam>
	[Serializable]
	internal class Repository<T> : IRepository, IRepository<T> where T : Entity, new()
	{
		#region Fields

		private readonly IList<EntityState> _cache;
		private readonly Database _database;
		private int _index;
		private readonly IQueryable<T> _query;
		private readonly IKeyValueRepository<T> _store;
		private readonly Type _type;

		#endregion

		#region Constructors

		public Repository(Database database)
		{
			_database = database;
			_cache = new List<EntityState>(4096);
			_index = 0;
			_type = typeof (T);

			if (!string.IsNullOrWhiteSpace(_database.FilePath))
			{
				var options = new KeyValueRepositoryOptions { IgnoreVirtualMembers = true };
				_store = KeyValueRepository<T>.Create(_database.FilePath, typeof (T).Name, options);
				_store.OnEnumerated += OnUpdateEntityRelationships;
			}

			_query = _store?.Select(x => (T) _cache.FirstOrDefault(y => y.Entity.Id == x.Id)?.Entity ?? AddOrUpdateCache(x)).AsQueryable()
				?? _cache.Where(x => x.State != EntityStateType.Added).Select(x => (T) x.Entity).AsQueryable();
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
			if (_cache.Any(x => entity == x.Entity))
			{
				return;
			}

			_cache.Add(new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
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

			var foundItem = _cache.FirstOrDefault(x => x.Entity.Id == entity.Id);
			if (foundItem == null)
			{
				_cache.Add(new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Unmodified });
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
			foreach (var entityState in _cache.Where(entityState => entityState.Entity.Id == 0))
			{
				entityState.Entity.Id = Interlocked.Increment(ref _index);
			}
		}

		/// <summary>
		/// Dispose of the entity store and cleans up all dependencies.
		/// </summary>
		public void Dispose()
		{
			if (_store != null)
			{
				_cache.Clear();
				_store?.Dispose();
			}
			else
			{
				_cache.Where(x => x.State == EntityStateType.Added).ToList()
					.ForEach(x => _cache.Remove(x));

				_cache.ForEach(x =>
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
			return _cache
				.Select(x => (T) x.Entity)
				.Where(filter)
				.AsQueryable();
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
			var keys = _store?.ReadKeys().ToList();
			_index = keys?.Count > 0 ? keys.Max(int.Parse) : 0;
		}

		/// <summary>
		/// Insert an entity to the repository before the provided entity. The ID of the entity must be the default value.
		/// </summary>
		/// <param name="entity"> The entity to be added. </param>
		/// <param name="targetEntity"> The entity to locate insert point. </param>
		public void InsertBefore(T entity, T targetEntity)
		{
			var state = _cache.FirstOrDefault(x => x.Entity == targetEntity);
			var indexOf = _cache.IndexOf(state);

			if (indexOf < 0)
			{
				throw new ArgumentException("Could not find the target entity", nameof(targetEntity));
			}

			_cache.Insert(indexOf, new EntityState { Entity = entity, OldEntity = CloneEntity(entity), State = EntityStateType.Added });
		}

		/// <summary>
		/// Get entity by ID.
		/// </summary>
		/// <param name="id"> </param>
		/// <returns> The entity or null. </returns>
		public Entity Read(int id)
		{
			var state = _cache.FirstOrDefault(x => x.Entity.Id == id);
			return state == null ? _store.Read(id.ToString()) : state.Entity;
		}

		/// <summary>
		/// Removes an entity from the repository.
		/// </summary>
		/// <param name="id"> The ID of the entity to remove. </param>
		public void Remove(int id)
		{
			var entity = _cache.FirstOrDefault(x => x.Entity.Id == id);

			if (entity == null)
			{
				entity = new EntityState { Entity = new T { Id = id } };
				_cache.Add(entity);
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
			_cache.Select(x => x.Entity)
				.Cast<T>()
				.Where(filter.Compile())
				.ForEach(Remove);
		}

		public int SaveChanges()
		{
			var changeCount = GetChanges().Count();

			var removed = _cache.Where(x => x.State == EntityStateType.Removed).ToList();
			foreach (var item in removed)
			{
				_store?.Remove(item.Entity.Id.ToString());
				_cache.Remove(item);
			}

			foreach (var entry in _cache)
			{
				if (_database.Options.MaintainDates)
				{
					var entity = entry.Entity;
					var modifiableEntity = entity as ModifiableEntity;

					// Check to see if the entity was added.
					if (entry.State == EntityStateType.Added)
					{
						// Make sure the modified on value matches created on for new items.
						entity.CreatedOn = DateTime.UtcNow;

						if (modifiableEntity != null)
						{
							modifiableEntity.ModifiedOn = entity.CreatedOn;
						}
					}

					// Check to see if the entity was modified.
					if (entry.State == EntityStateType.Modified)
					{
						// Do not allow created on to change for entities.
						entity.CreatedOn = entry.OldEntity.CreatedOn;

						if (modifiableEntity != null)
						{
							// Update modified to now for new entities.
							modifiableEntity.ModifiedOn = DateTime.UtcNow;
						}
					}
				}

				if (_store != null)
				{
					_store?.Write(entry.Entity.Id.ToString(), (T) entry.Entity);
				}
				else
				{
					entry.OldEntity = CloneEntity(entry.Entity);
					entry.State = EntityStateType.Unmodified;
				}
			}

			if (_store != null)
			{
				_store.Save();
				_store.Flush();
				_cache.Clear();
			}

			return changeCount;
		}

		public void UpdateRelationships()
		{
			_cache.ToList().ForEach(x => OnUpdateEntityRelationships((T) x.Entity));
		}

		public void ValidateEntities()
		{
			_cache.Where(x => x.State == EntityStateType.Added || x.State == EntityStateType.Modified)
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

			var enumerableType = typeof (IEnumerable);
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
				if (currentCollectionType.Name == typeof (RelationshipRepository<>).Name)
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
			foreach (var item in _cache.Where(x => x.State == EntityStateType.Unmodified))
			{
				if (!CompareEntity(item.Entity, item.OldEntity))
				{
					item.State = EntityStateType.Modified;
				}
			}

			return _cache.Where(x => x.State != EntityStateType.Unmodified).ToList();
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