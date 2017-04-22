#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represents a collection of entities for a Speedy database.
	/// </summary>
	/// <typeparam name="T"> The type contained in the repository. </typeparam>
	[Serializable]
	internal class Repository<T, T2> : IRepository, IRepository<T, T2> 
		where T : Entity<T2>, new() 
		where T2 : new()
	{

	#region Fields

	protected readonly IList<EntityState<T, T2>> Cache;
	protected readonly IKeyValueRepository<T> Store;
	private readonly Database _database;
	private readonly IQueryable<T> _query;
	private readonly Type _type;
	private T2 _currentKey;

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
			var options = new KeyValueRepositoryOptions { IgnoreVirtualMembers = true, Timeout = database.Options.Timeout };
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

	/// <summary>
	/// Assign primary keys to all entities.
	/// </summary>
	public void AssignKeys()
	{
		foreach (var entityState in Cache.Where(entityState => !entityState.Entity.IdIsSet()))
		{
			_database.UpdateDependantIds<T, T2>(entityState.Entity, x => x.NewId(ref _currentKey), new List<Entity<T2>>());

			if (!entityState.Entity.IdIsSet())
			{
				var id = entityState.Entity.NewId(ref _currentKey);
				if (!Equals(id, default(T2)))
				{
					entityState.Entity.Id = id;
				}
			}
		}
	}

	/// <summary>
	/// Discard all changes made in this context to the underlying database.
	/// </summary>
	public int DiscardChanges()
	{
		var response = Cache.Count;
		Cache.Clear();
		return response;
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
			.Select(x => x.Entity)
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

	public bool HasDependentRelationship(object[] value, object id)
	{
		var foreignKeyFunction = (Func<T, object>) value[4];
		return this.Any(x => foreignKeyFunction.Invoke(x).Equals(id));
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
		// todo: support existing records? How?
		_currentKey = default(T2);
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

	/// <summary>
	/// Removes an entity from the repository.
	/// </summary>
	/// <param name="id"> The ID of the entity to remove. </param>
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
	public void Remove(Expression<Func<T, bool>> filter)
	{
		Cache.Select(x => x.Entity)
			.Cast<T>()
			.Where(filter.Compile())
			.ForEach(Remove);
	}

	/// <summary>
	/// Save the data to the data store.
	/// </summary>
	/// <returns> The number of items saved. </returns>
	public int SaveChanges()
	{
		var changeCount = GetChanges().Count();
		var removed = Cache.Where(x => x.State == EntityStateType.Removed).ToList();

		foreach (var item in removed)
		{
			Store?.Remove(item.Entity.Id.ToString());
			Cache.Remove(item);
		}

		foreach (var entry in Cache)
		{
			var entity = entry.Entity;
			var createdEntity = entity as ICreatedEntity;
			var modifiableEntity = entity as IModifiableEntity;
			var maintainedEntity = _database.Options.UnmaintainEntities.All(x => x != entry.Entity.GetType());
			var maintainDates = maintainedEntity && _database.Options.MaintainDates;
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

	public void UpdateRelationships()
	{
		Cache.ToList().ForEach(x => OnUpdateEntityRelationships(x.Entity));
	}

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

	private void UpdateEntity<T>(Entity<T> entity, Entity<T> updatedEntity)
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