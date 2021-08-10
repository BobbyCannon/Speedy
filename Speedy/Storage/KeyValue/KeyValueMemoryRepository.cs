#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Storage.KeyValue
{
	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class KeyValueMemoryRepository : KeyValueMemoryRepository<string>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public KeyValueMemoryRepository(string name, TimeSpan? timeout = null, int limit = 0)
			: base(name, timeout, limit)
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		public KeyValueMemoryRepository(string name, KeyValueRepositoryOptions options)
			: base(name, options)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class KeyValueMemoryRepository<T> : IKeyValueRepository<T>
	{
		#region Fields

		private readonly Dictionary<string, Tuple<string, DateTime, ulong>> _cache;
		private readonly Dictionary<string, Tuple<string, DateTime, ulong>> _changes;
		private readonly KeyValueRepositoryOptions _options;
		private readonly SerializerSettings _settings;
		private ulong _writeIndex;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public KeyValueMemoryRepository(string name, TimeSpan? timeout = null, int limit = 0)
			: this(name, new KeyValueRepositoryOptions { Limit = limit, Timeout = timeout ?? TimeSpan.Zero })
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		public KeyValueMemoryRepository(string name, KeyValueRepositoryOptions options)
		{
			_options = options;
			_cache = new Dictionary<string, Tuple<string, DateTime, ulong>>(_options.Limit == int.MaxValue ? 4096 : _options.Limit);
			_changes = new Dictionary<string, Tuple<string, DateTime, ulong>>();
			_settings = new SerializerSettings(false, false, false, true);
			_writeIndex = 0;

			Name = name;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The number of items in the repository.
		/// </summary>
		public int Count => GetCount();

		/// <summary>
		/// The name of the repository.
		/// </summary>
		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Archives the repository.
		/// </summary>
		public void Archive()
		{
			lock (_changes)
			{
				_cache.Clear();
				_changes.Clear();

				Archived?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Clears the repository.
		/// </summary>
		public void Clear()
		{
			lock (_changes)
			{
				_cache.Clear();
				_changes.Clear();
			}
		}

		/// <summary>
		/// Delete the repository.
		/// </summary>
		public void Delete()
		{
			lock (_changes)
			{
				_cache.Clear();
				_changes.Clear();

				Deleted?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Flushes all cached items to storage.
		/// </summary>
		public void Flush()
		{
			lock (_changes)
			{
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			foreach (var item in Read())
			{
				OnEnumerated?.Invoke(item.Value);
				yield return item.Value;
			}
		}

		/// <summary>
		/// Loads items directly into the repository. This will not check the keys so we can
		/// speed up the loading of items.
		/// </summary>
		/// <param name="items"> The items to load into the repository. </param>
		/// <remarks> Will not be cached. These items will be written directly to disk. </remarks>
		/// <remarks> If you need key protection then use Write instead. </remarks>
		public void Load(Dictionary<string, T> items)
		{
			Write(items);
			Save();
		}

		/// <summary>
		/// Read the repository using an enumerator.
		/// </summary>
		/// <returns> The list of key value pairs to enumerate. </returns>
		/// <remarks>
		/// Must be IEnumerable so we can yield the return.
		/// </remarks>
		public IEnumerable<KeyValuePair<string, T>> Read()
		{
			lock (_changes)
			{
				foreach (var item in _cache.OrderBy(x => x.Value.Item3).Where(x => x.Value.Item1 != null))
				{
					yield return new KeyValuePair<string, T>(item.Key, item.Value.Item1.FromJson<T>());
				}
			}
		}

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <returns> The value for the key. </returns>
		/// <exception cref="KeyNotFoundException"> Could not find the entry with the key. </exception>
		public T Read(string key)
		{
			var response = Read(new HashSet<string> { key }).FirstOrDefault();
			if (response.Key == null)
			{
				throw new KeyNotFoundException(SpeedyException.KeyNotFound);
			}

			return response.Value;
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to read. </param>
		/// <returns> The value for the keys. </returns>
		public IEnumerable<KeyValuePair<string, T>> Read(HashSet<string> keys)
		{
			return Read(keys.Contains);
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="condition"> The condition to test each key against. </param>
		/// <returns> The value for the keys that match the condition. </returns>
		public IEnumerable<KeyValuePair<string, T>> Read(Func<string, bool> condition)
		{
			lock (_changes)
			{
				foreach (var item in Read().Where(item => condition.Invoke(item.Key)))
				{
					yield return item;
				}
			}
		}

		/// <summary>
		/// Read all the keys from the repository.
		/// </summary>
		/// <returns> The keys for the repository. </returns>
		public IEnumerable<string> ReadKeys()
		{
			return Read().Select(x => x.Key);
		}

		/// <summary>
		/// Removes an item from the repository by the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to remove. </param>
		public void Remove(string key)
		{
			lock (_changes)
			{
				_changes.AddOrUpdate(key, new Tuple<string, DateTime, ulong>(null, TimeService.UtcNow, _writeIndex++));
			}
		}

		/// <summary>
		/// Removes items from the repository by the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to remove. </param>
		public void Remove(HashSet<string> keys)
		{
			foreach (var key in keys)
			{
				Remove(key);
			}
		}

		/// <summary>
		/// Save the changes to the repository (Loads, Writes, Removes, etc).
		/// </summary>
		public void Save()
		{
			lock (_changes)
			{
				UpdateCache();
			}
		}

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <param name="value"> The value read. </param>
		/// <returns> True if the key was found or false if otherwise.. </returns>
		public bool TryRead(string key, out T value)
		{
			var response = Read(new HashSet<string> { key }).FirstOrDefault();
			if (response.Key != key)
			{
				value = default;
				return false;
			}

			value = response.Value;
			return true;
		}

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key"> The key of the item to write. </param>
		/// <param name="value"> The value of the item to write. </param>
		public void Write(string key, T value)
		{
			lock (_changes)
			{
				_changes.AddOrUpdate(key, new Tuple<string, DateTime, ulong>(value.ToJson(_settings), TimeService.UtcNow, _writeIndex++));
			}
		}

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key"> The key of the item to write. </param>
		/// <param name="value"> The value of the item to write. </param>
		public void Write(string key, string value)
		{
			lock (_changes)
			{
				_changes.AddOrUpdate(key, new Tuple<string, DateTime, ulong>($"\"{value}\"", TimeService.UtcNow, _writeIndex++));
			}
		}

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items"> The list of items to add to the repository. </param>
		public void Write(Dictionary<string, T> items)
		{
			foreach (var item in items)
			{
				Write(item.Key, item.Value);
			}
		}

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items"> The list of items to add to the repository. </param>
		public void Write(Dictionary<string, string> items)
		{
			foreach (var item in items)
			{
				Write(item.Key, item.Value);
			}
		}

		/// <summary>
		/// Disposes of the repository.
		/// </summary>
		/// <param name="disposing"> True to dispose managed objects. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
			}
		}

		/// <summary>
		/// Gets the count if items in the repository.
		/// </summary>
		/// <returns> The number of items in the repository. </returns>
		private int GetCount()
		{
			lock (_changes)
			{
				return _options.Limit == int.MaxValue ? _cache.Count : Read().Count();
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Updates the cache with the changes.
		/// </summary>
		private void UpdateCache()
		{
			foreach (var change in _changes.ToList())
			{
				_cache.AddOrUpdate(change.Key, change.Value);
				_changes.Remove(change.Key);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Will be called on archive request.
		/// </summary>
		public event EventHandler Archived;

		/// <summary>
		/// Will be called on deletion request.
		/// </summary>
		public event EventHandler Deleted;

		/// <summary>
		/// Will be called on each item when this repository is enumerated.
		/// </summary>
		public event Action<T> OnEnumerated;

		#endregion
	}
}