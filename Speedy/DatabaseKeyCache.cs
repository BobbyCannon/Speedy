#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Speedy.Extensions;
using Speedy.Storage;
using Speedy.Sync;

#endregion

namespace Speedy
{
	/// <summary>
	/// Cache for managing database keys. This allows for caching of entities ID and Sync IDs.
	/// </summary>
	public class DatabaseKeyCache
	{
		#region Fields

		private readonly Dictionary<Type, MemoryCache> _cachedEntityId;
		private readonly TimeSpan _cacheTimeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the database key cache.
		/// </summary>
		public DatabaseKeyCache() : this(TimeSpan.MaxValue)
		{
		}

		/// <summary>
		/// Instantiate an instance of the database key cache.
		/// </summary>
		/// <param name="cacheTimeout"> The timeout for removing an item from the cache. </param>
		public DatabaseKeyCache(TimeSpan cacheTimeout)
		{
			_cachedEntityId = new Dictionary<Type, MemoryCache>();
			_cacheTimeout = cacheTimeout;

			SyncEntitiesToCache = Array.Empty<Type>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the list of entities to cache the keys (ID, Sync ID). If the collection is empty
		/// then cache all sync entities.
		/// </summary>
		public Type[] SyncEntitiesToCache { get; set; }

		/// <summary>
		/// The total types tracked.
		/// </summary>
		public int Count => _cachedEntityId.Count;

		/// <summary>
		/// The total count for all items tracked.
		/// </summary>
		public int TotalCachedItems => _cachedEntityId.Sum(x => x.Value.Count());

		#endregion

		#region Methods

		/// <summary>
		/// Cache an entity ID for the sync entity.
		/// </summary>
		/// <param name="entity"> The entity to be cached. </param>
		public void AddEntity(ISyncEntity entity)
		{
			AddEntityId(entity.GetRealType(), entity.SyncId, entity.GetEntityId());
		}

		/// <summary>
		/// Cache an entity ID for the entity Sync ID.
		/// </summary>
		/// <param name="type"> The type of the entity. </param>
		/// <param name="syncId"> The sync ID of the entity. </param>
		/// <param name="id"> The ID of the entity. </param>
		public void AddEntityId(Type type, Guid syncId, object id)
		{
			if (id == null)
			{
				return;
			}

			if (SyncEntitiesToCache.Length > 0 && !SyncEntitiesToCache.Contains(type))
			{
				// We are filtering what sync entities to cache and this entity is not in the list
				return;
			}

			if (!_cachedEntityId.ContainsKey(type))
			{
				_cachedEntityId.Add(type, new MemoryCache(_cacheTimeout));
			}

			var cache = _cachedEntityId[type];
			cache.Set(syncId.ToString(), id);
		}

		/// <summary>
		/// Cleanup the cache by removing old entries and empty collections.
		/// </summary>
		public void Cleanup()
		{
			foreach (var entry in _cachedEntityId)
			{
				entry.Value
					.Where(x => x.HasExpired)
					.ToList()
					.ForEach(x => entry.Value.Remove(x));
			}

			_cachedEntityId
				.Where(x => x.Value.IsEmpty)
				.ToList()
				.ForEach(x => _cachedEntityId.Remove(x.Key));
		}

		/// <summary>
		/// Clear all caches from the manager
		/// </summary>
		public void Clear()
		{
			_cachedEntityId.ForEach(x => x.Value.Clear());
			_cachedEntityId.Clear();
		}

		/// <summary>
		/// Get the entity ID for the sync ID.
		/// </summary>
		/// <param name="type"> The type of the entity. </param>
		/// <param name="syncId"> The sync ID of the entity. </param>
		/// <returns> The ID of the entity. </returns>
		public object GetEntityId(Type type, Guid syncId)
		{
			if (!_cachedEntityId.ContainsKey(type))
			{
				_cachedEntityId.Add(type, new MemoryCache(_cacheTimeout));
			}

			var cache = _cachedEntityId[type];
			return cache.TryGet(syncId.ToString(), out var cachedItem) ? cachedItem.Value : null;
		}

		/// <summary>
		/// Initializes the default key cache.
		/// </summary>
		/// <param name="database"> The syncable database. </param>
		public void Initialize(ISyncableDatabase database)
		{
			var syncOptions = new SyncOptions();
			var repositories = database.GetSyncableRepositories(syncOptions).ToList();

			foreach (var repository in repositories)
			{
				var type = repository.RealType;

				if (SyncEntitiesToCache.Length > 0 && !SyncEntitiesToCache.Contains(type))
				{
					// We are filtering what sync entities to cache and this entity is not in the list
					continue;
				}

				var keys = repository.ReadAllKeys();
				keys.ForEach(x => AddEntityId(type, x.Key, x.Value));
			}
		}

		/// <summary>
		/// Remove an entity ID for the entity Sync ID.
		/// </summary>
		/// <param name="entity"> The entity to be un-cached. </param>
		public void RemoveEntity(ISyncEntity entity)
		{
			if (entity == null)
			{
				return;
			}

			RemoveEntityId(entity.GetRealType(), entity.SyncId);
		}

		/// <summary>
		/// Remove an entity ID for the entity Sync ID.
		/// </summary>
		/// <param name="type"> The type of the entity. </param>
		/// <param name="syncId"> The sync ID of the entity. </param>
		public void RemoveEntityId(Type type, Guid syncId)
		{
			if (!_cachedEntityId.ContainsKey(type))
			{
				return;
			}

			var cache = _cachedEntityId[type];
			cache.Remove(syncId.ToString());
		}

		/// <summary>
		/// Get a detailed string of cached entities
		/// </summary>
		/// <returns> A detailed string. </returns>
		public string ToDetailedString()
		{
			var builder = new StringBuilder();
			foreach (var test in _cachedEntityId)
			{
				builder.AppendLine($"\t{test.Key.FullName}");

				foreach (var test2 in test.Value)
				{
					builder.AppendLine($"\t\t{test2.Key}-{test2.Value}");
				}
			}
			return builder.ToString();
		}

		#endregion
	}
}