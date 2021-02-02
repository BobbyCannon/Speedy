#region References

using System;
using System.Collections;
using System.Collections.Generic;
using Speedy.Extensions;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represent a memory cache.
	/// </summary>
	public class MemoryCache : IEnumerable<MemoryCacheItem>
	{
		#region Fields

		private readonly TimeSpan _defaultTimeout;
		private readonly Dictionary<string, MemoryCacheItem> _dictionary;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a memory cache.
		/// </summary>
		public MemoryCache() : this(TimeSpan.FromMinutes(15))
		{
		}

		/// <summary>
		/// Instantiates a memory cache.
		/// </summary>
		/// <param name="defaultTimeout"> The default timeout of new entries. </param>
		public MemoryCache(TimeSpan defaultTimeout)
		{
			_defaultTimeout = defaultTimeout;
			_dictionary = new Dictionary<string, MemoryCacheItem>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Indicates whether or not the memory cache is empty.
		/// </summary>
		public bool IsEmpty => _dictionary.Count <= 0;

		/// <summary>
		/// Determines if the expiration time should be extended when read from the cache.
		/// </summary>
		public bool SlidingExpiration { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Clear the memory cache.
		/// </summary>
		public void Clear()
		{
			_dictionary.Clear();
		}

		/// <inheritdoc />
		public IEnumerator<MemoryCacheItem> GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		/// <summary>
		/// Remove an entry by the key name.
		/// </summary>
		/// <param name="key"> The name of the key. </param>
		public void Remove(string key)
		{
			_dictionary.Remove(key);
		}

		/// <summary>
		/// Remove the entry from the cache.
		/// </summary>
		/// <param name="memoryCacheItem"> The item to remove from the cache. </param>
		public void Remove(MemoryCacheItem memoryCacheItem)
		{
			_dictionary.Remove(memoryCacheItem.Key);
		}

		/// <summary>
		/// Set a new entry. This will add a new entry or update an existing one.
		/// </summary>
		/// <param name="key"> The key of the entry. </param>
		/// <param name="value"> The value of the entry. </param>
		public void Set(string key, object value)
		{
			Set(key, value, _defaultTimeout);
		}

		/// <summary>
		/// Set a new entry with a custom timeout. This will add a new entry or update an existing one.
		/// </summary>
		/// <param name="key"> The key of the entry. </param>
		/// <param name="value"> The value of the entry. </param>
		/// <param name="timeout"> The custom timeout of the entry. </param>
		public void Set(string key, object value, TimeSpan timeout)
		{
			_dictionary.AddOrUpdate(key, () => new MemoryCacheItem(key, value, timeout), x =>
			{
				x.Key = key;
				x.Value = value;
				x.LastAccessed = TimeService.UtcNow;
				return x;
			});
		}

		/// <summary>
		/// Try to get an entry from the cache.
		/// </summary>
		/// <param name="key"> The key of the entry. </param>
		/// <param name="value"> The entry that was found or otherwise null. </param>
		/// <returns> True if the entry was found or otherwise false. </returns>
		public bool TryGet(string key, out MemoryCacheItem value)
		{
			if (!_dictionary.TryGetValue(key, out var cachedItem))
			{
				value = null;
				return false;
			}

			if (cachedItem.ExpirationDate <= TimeService.UtcNow)
			{
				value = null;
				Remove(cachedItem);
				return false;
			}

			value = cachedItem;
			value.LastAccessed = TimeService.UtcNow;
			return true;
		}

		internal void UpdateTimeout(TimeSpan value)
		{
			foreach (var item in _dictionary)
			{
				item.Value.Timeout = value;
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}