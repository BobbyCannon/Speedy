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
		#region Constructors

		/// <summary>
		/// Instantiates a memory cache item.
		/// </summary>
		/// <param name="key"> The key of the item. </param>
		/// <param name="value"> The value of the item. </param>
		/// <param name="timeout"> The timeout of the item. </param>
		public MemoryCacheItem(string key, object value, TimeSpan timeout)
		{
			Key = key;
			Value = value;
			CreatedOn = TimeService.UtcNow;
			LastAccessed = CreatedOn;
			Timeout = timeout;
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
		public DateTime ExpirationDate => Timeout == TimeSpan.MaxValue ? DateTime.MaxValue : LastAccessed.Add(Timeout);

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
		public TimeSpan Timeout { get; set; }

		/// <summary>
		/// The value of the item.
		/// </summary>
		public object Value { get; set; }

		#endregion
	}
}