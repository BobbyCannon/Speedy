#region References

using System;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Represents an item for a memory cache.
	/// </summary>
	public class MemoryCacheItem
	{
		#region Fields

		private readonly MemoryCache _cache;
		private readonly TimeSpan? _timeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a memory cache item.
		/// </summary>
		/// <param name="cache"> The cache this item is for. </param>
		/// <param name="key"> The key of the item. </param>
		/// <param name="value"> The value of the item. </param>
		/// <param name="timeout"> The timeout of the item. </param>
		public MemoryCacheItem(MemoryCache cache, string key, object value, TimeSpan? timeout)
		{
			_cache = cache;
			_timeout = timeout;

			Key = key;
			Value = value;
			CreatedOn = TimeService.UtcNow;
			LastAccessed = CreatedOn;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The date and time the cached item was created.
		/// </summary>
		public DateTime CreatedOn { get; }

		/// <summary>
		/// The date and time the item will expire.
		/// </summary>
		public DateTime ExpirationDate =>
			Timeout == TimeSpan.MaxValue
				? DateTime.MaxValue
				: _cache?.SlidingExpiration == true
					? LastAccessed.Add(Timeout)
					: CreatedOn.Add(Timeout);

		/// <summary>
		/// Indicates if the item has expired.
		/// </summary>
		public bool HasExpired => TimeService.UtcNow >= ExpirationDate;

		/// <summary>
		/// The key of the item.
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		/// The last time the item was accessed.
		/// </summary>
		public DateTime LastAccessed { get; set; }

		/// <summary>
		/// The timeout value of the item.
		/// </summary>
		public TimeSpan Timeout => _timeout ?? _cache.DefaultTimeout;

		/// <summary>
		/// The value of the item.
		/// </summary>
		public object Value { get; set; }

		#endregion
	}
}