#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represent a memory cache.
	/// </summary>
	public class MemoryCache : IEnumerable<MemoryCacheItem>, ICollection
	{
		#region Fields

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
			_dictionary = new Dictionary<string, MemoryCacheItem>();

			DefaultTimeout = defaultTimeout;
			SlidingExpiration = true;
			SyncRoot = new object();
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public int Count
		{
			get
			{
				lock (SyncRoot)
				{
					return _dictionary.Count;
				}
			}
		}

		/// <summary>
		/// The default timeout for items when they are added.
		/// </summary>
		public TimeSpan DefaultTimeout { get; }

		/// <summary>
		/// Indicates whether or not the memory cache is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				lock (SyncRoot)
				{
					return _dictionary.Count <= 0;
				}
			}
		}

		/// <inheritdoc />
		public bool IsSynchronized => true;

		/// <summary>
		/// Determines if the expiration time should be extended when read from the cache.
		/// </summary>
		public bool SlidingExpiration { get; set; }

		/// <inheritdoc />
		public object SyncRoot { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Clear the memory cache.
		/// </summary>
		public void Clear()
		{
			lock (SyncRoot)
			{
				_dictionary.Clear();
			}
		}

		/// <inheritdoc />
		public void CopyTo(Array array, int index)
		{
			lock (SyncRoot)
			{
				Array.Copy(_dictionary.Values.ToArray(), 0, array, index, _dictionary.Count);
			}
		}

		/// <summary>
		/// Enumerator for the memory cache.
		/// </summary>
		/// <returns> The enumerator for the collection. </returns>
		/// <remarks>
		/// Enumeration should NOT be considered "accessing" items
		/// We only bump last accessed by direct access. This allows
		/// Enumeration of the item to check expiration
		/// </remarks>
		public IEnumerator<MemoryCacheItem> GetEnumerator()
		{
			lock (SyncRoot)
			{
				foreach (var value in _dictionary)
				{
					// See remark in method summary.
					//value.Value.LastAccessed = TimeService.UtcNow;
					yield return value.Value;
				}
			}
		}

		/// <summary>
		/// Remove an entry by the key name.
		/// </summary>
		/// <param name="key"> The name of the key. </param>
		public MemoryCacheItem Remove(string key)
		{
			lock (SyncRoot)
			{
				if (!_dictionary.ContainsKey(key))
				{
					return default;
				}

				var item = _dictionary[key];
				_dictionary.Remove(key);
				return item;
			}
		}

		/// <summary>
		/// Remove the entry from the cache.
		/// </summary>
		/// <param name="memoryCacheItem"> The item to remove from the cache. </param>
		public MemoryCacheItem Remove(MemoryCacheItem memoryCacheItem)
		{
			return Remove(memoryCacheItem?.Key);
		}

		/// <summary>
		/// Set a new entry with a custom timeout. This will add a new entry or update an existing one.
		/// </summary>
		/// <param name="key"> The key of the entry. </param>
		/// <param name="value"> The value of the entry. </param>
		/// <param name="timeout"> The custom timeout of the entry. </param>
		public void Set(string key, object value, TimeSpan? timeout = null)
		{
			lock (SyncRoot)
			{
				_dictionary.AddOrUpdate(key,
					() => new MemoryCacheItem(this, key, value, timeout),
					x =>
					{
						x.Key = key;
						x.Value = value;
						x.LastAccessed = TimeService.UtcNow;
						return x;
					}
				);
			}
		}

		/// <summary>
		/// Try to get an entry from the cache.
		/// </summary>
		/// <param name="key"> The key of the entry. </param>
		/// <param name="value"> The entry that was found or otherwise null. </param>
		/// <returns> True if the entry was found or otherwise false. </returns>
		public bool TryGet(string key, out MemoryCacheItem value)
		{
			lock (SyncRoot)
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
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}