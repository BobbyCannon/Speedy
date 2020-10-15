#region References

using System;
using System.Collections.Generic;
using System.Runtime.Caching;

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
		private static readonly CacheItemPolicy _defaultCachePolicy;

		#endregion

		#region Constructors

		static CacheManager()
		{
			_cachedEntityId = new Dictionary<Type, MemoryCache>();
			_defaultCachePolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(15) };
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
				_cachedEntityId.Add(type, new MemoryCache(type.FullName ?? type.Name));
			}

			var cache = _cachedEntityId[type];

			cache.Set(syncId.ToString(), id, _defaultCachePolicy);
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
				_cachedEntityId.Add(type, new MemoryCache(type.FullName ?? type.Name));
			}

			var cache = _cachedEntityId[type];
			return cache.Get(syncId.ToString());
		}

		#endregion
	}
}