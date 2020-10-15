#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Manager for managing items that are cached in Speedy.
	/// </summary>
	public static class CacheManager
	{
		#region Fields

		private static readonly Dictionary<Type, MemoryCache> _cachedEntityId;
		private static readonly TimeSpan _defaultTimeout;

		#endregion

		#region Constructors

		static CacheManager()
		{
			_cachedEntityId = new Dictionary<Type, MemoryCache>();
			_defaultTimeout = TimeSpan.FromMinutes(15);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Cache an entity ID for the entity Sync ID.
		/// </summary>
		/// <param name="type"> The type of the entity. </param>
		/// <param name="syncId"> The sync ID of the entity. </param>
		/// <param name="id"> The ID of the entity. </param>
		public static void CacheEntityId(Type type, Guid syncId, object id)
		{
			if (id == null)
			{
				return;
			}

			if (!_cachedEntityId.ContainsKey(type))
			{
				_cachedEntityId.Add(type, new MemoryCache(_defaultTimeout));
			}

			var cache = _cachedEntityId[type];
			cache.Set(syncId.ToString(), id);
		}

		/// <summary>
		/// Cleanup the cache by removing old entries and empty collections.
		/// </summary>
		public static void Cleanup()
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
		public static void Clear()
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
		public static object GetEntityId(Type type, Guid syncId)
		{
			if (!_cachedEntityId.ContainsKey(type))
			{
				_cachedEntityId.Add(type, new MemoryCache(_defaultTimeout));
			}

			var cache = _cachedEntityId[type];
			return cache.TryGet(syncId.ToString(), out var cachedItem) ? cachedItem.Value : null;
		}

		#endregion
	}
}