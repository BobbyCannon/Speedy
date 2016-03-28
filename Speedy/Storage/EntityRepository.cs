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
	internal class EntityRepository<T> : IEntityRepository, IEntityRepository<T> where T : Entity, new()
	{
		#region Fields

		private int _index;
		private readonly IQueryable<T> _query;
		private readonly EntityStore<T> _store;
		private readonly Type _type;

		#endregion

		#region Constructors

		public EntityRepository(string directory)
		{
			Cache = new List<EntityState>();

			_index = 0;
			_type = typeof (T);

			if (!string.IsNullOrWhiteSpace(directory))
			{
				_store = new EntityStore<T>($"{directory}\\{typeof (T).Name}", this);
				_store.UpdateEntityRelationships += OnUpdateEntityRelationships;
			}

			_query = _store?.AsQueryable() ?? Cache
				.Where(x => x.State != EntityStateType.Added)
				.Select(x => (T)x.Entity)
				.AsQueryable();
		}

		#endregion

		#region Properties

		public IList<EntityState> Cache { get; }

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
			var entry = Cache.FirstOrDefault(x => x.Entity == entity);
			if (entry != null)
			{
				// The entity is already in the cache, just make sure it's marked as added.
				entry.State = EntityStateType.Added;
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

		public void Reset()
		{
			Cache.Where(x => x.State == EntityStateType.Added).ToList()
				.ForEach(x => Cache.Remove(x));

			if (_store != null)
			{
				Cache.Clear();
			}
			else
			{
				Cache.ForEach(x =>
				{
					UpdateEntity(x.Entity, x.OldEntity);
					x.State = EntityStateType.Unmodified;
				});
			}
		}

		public int SaveChanges()
		{
			var changeCount = GetChanges().Count();

			var removed = Cache.Where(x => x.State == EntityStateType.Removed).ToList();
			foreach (var item in removed)
			{
				_store?.Remove(item.Entity.Id);
				Cache.Remove(item);
			}

			foreach (var item in Cache)
			{
				if (item.OldEntity != null)
				{
					item.Entity.CreatedOn = item.OldEntity.CreatedOn;
				}

				if (_store != null)
				{
					_store?.Write(item.Entity);
				}
				else
				{
					item.OldEntity = CloneEntity(item.Entity);
					item.State = EntityStateType.Unmodified;
				}
			}

			if (_store != null)
			{
				Cache.Clear();
			}

			return changeCount;
		}

		public void UpdateRelationships()
		{
			Cache.ToList().ForEach(x => OnUpdateEntityRelationships((T) x.Entity));
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