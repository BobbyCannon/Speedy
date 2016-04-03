#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Speedy.Streams;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class Repository : Repository<string>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public Repository(string directory, string name, TimeSpan? timeout = null, int limit = 0)
			: base(directory, name, timeout, limit)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class Repository<T> : IRepository<T>
	{
		#region Fields

		private readonly Dictionary<string, Tuple<string, DateTime>> _cache;
		private readonly int _cacheLimit;
		private readonly TimeSpan _cacheTimeout;
		private readonly Dictionary<string, Tuple<string, DateTime>> _changes;
		private FileStream _fileStream;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public Repository(string directory, string name, TimeSpan? timeout = null, int limit = 0)
			: this(new DirectoryInfo(directory), name, timeout, limit)
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		private Repository(DirectoryInfo directoryInfo, string name, TimeSpan? timeout = null, int limit = 0)
		{
			DirectoryInfo = directoryInfo;
			Name = name;
			FileInfo = new FileInfo($"{DirectoryInfo.FullName}\\{Name}.speedy");
			TempFileInfo = new FileInfo($"{FileInfo.FullName}.temp");
			_cache = new Dictionary<string, Tuple<string, DateTime>>(limit);
			_cacheLimit = limit;
			_cacheTimeout = timeout ?? TimeSpan.Zero;
			_changes = new Dictionary<string, Tuple<string, DateTime>>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The number of items in the repository.
		/// </summary>
		public int Count => GetCount();

		/// <summary>
		/// The directory the repository will be located.
		/// </summary>
		public DirectoryInfo DirectoryInfo { get; }

		/// <summary>
		/// The name of the repository.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the file info of the repository file.
		/// </summary>
		private FileInfo FileInfo { get; }

		/// <summary>
		/// Gets the full path to the temporary repository file.
		/// </summary>
		private FileInfo TempFileInfo { get; }

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
				_fileStream.Close();
				_fileStream = null;
				FileInfo.SafeMove(new FileInfo(FileInfo.FullName + ".archive"));
				TempFileInfo.SafeDelete();
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
				_fileStream.SetLength(0);
				_fileStream.Flush(true);
			}
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public static IRepository<T> Create(DirectoryInfo directoryInfo, string name, TimeSpan? timeout = null, int limit = 0)
		{
			return Create(directoryInfo.FullName, name, timeout, limit);
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout">
		/// The amount of time to cache items in memory before persisting to disk. Defaults to null and then
		/// TimeSpan.Zero is used.
		/// </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public static IRepository<T> Create(string directory, string name, TimeSpan? timeout = null, int limit = 0)
		{
			var repository = new Repository<T>(directory, name, timeout, limit);
			repository.Initialize();
			return repository;
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
				_fileStream.Close();
				_fileStream = null;
				FileInfo.SafeDelete();
				TempFileInfo.SafeDelete();
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
				try
				{
					SaveRepository(DateTime.MaxValue);
				}
				finally
				{
					TempFileInfo.SafeDelete();
				}
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
			lock (_changes)
			{
				_fileStream.Position = _fileStream.Length;
				var writer = new NoCloseStreamWriter(_fileStream);

				foreach (var item in items)
				{
					writer.WriteLine(item.Key + "|" + item.Value);
				}

				writer.Flush();
				writer.Dispose();
			}
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
				foreach (var item in _cache.OrderBy(x => x.Value.Item2).Where(x => x.Value.Item1 != null))
				{
					yield return new KeyValuePair<string, T>(item.Key, item.Value.Item1.FromJson<T>());
				}

				_fileStream.Position = 0;
				var reader = new StreamReader(_fileStream);

				while (reader.Peek() > 0)
				{
					var line = reader.ReadLine();
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					var delimiter = line.IndexOf("|");
					if (delimiter <= 0)
					{
						continue;
					}

					var readKey = line.Substring(0, delimiter);
					if (_cache.ContainsKey(readKey))
					{
						// Skip this item because it's in the cache.
						continue;
					}

					yield return new KeyValuePair<string, T>(readKey, line.Substring(delimiter + 1, line.Length - delimiter - 1).FromJson<T>());
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
				throw new KeyNotFoundException("Could not find the entry with the key.");
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
				_changes.AddOrUpdate(key, new Tuple<string, DateTime>(null, DateTime.UtcNow));
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

				var threshold = DateTime.UtcNow - _cacheTimeout;
				if (_cache.Count <= _cacheLimit && !_cache.Any(x => x.Value.Item2 <= threshold))
				{
					return;
				}

				try
				{
					// We have items to persist to disk.
					SaveRepository(threshold);
				}
				finally
				{
					TempFileInfo.SafeDelete();
				}
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
				value = default(T);
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
				_changes.AddOrUpdate(key, new Tuple<string, DateTime>(value.ToJson(), DateTime.UtcNow));
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

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
				_fileStream?.Dispose();
				_fileStream = null;
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
				return Read().Count();
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
		/// Initializes the path the repository is to be located.
		/// </summary>
		private void Initialize()
		{
			lock (_changes)
			{
				DirectoryInfo.SafeCreate();
				FileInfo.SafeCreate();

				_fileStream = FileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

				// Check to see if we were in the middle of a save.
				TempFileInfo.Refresh();
				if (TempFileInfo.Exists)
				{
					// Finish our save.
					SaveRepository(DateTime.UtcNow);
				}
			}
		}

		/// <summary>
		/// Saves items to the repository. Including items over the cache limit and any that have expired due to the cache timeout.
		/// </summary>
		/// <param name="threshold"> The date time threshold that was calculated from the cache timeout. </param>
		private void SaveRepository(DateTime threshold)
		{
			FileInfo.Refresh();
			if (!FileInfo.Exists || _fileStream == null)
			{
				return;
			}

			TempFileInfo.Refresh();
			if (!TempFileInfo.Exists)
			{
				File.Copy(FileInfo.FullName, TempFileInfo.FullName);
			}

			using (var tempStream = TempFileInfo.OpenFile())
			{
				_fileStream.SetLength(0);
				_fileStream.Flush(true);

				var reader = new NoCloseStreamReader(tempStream);
				var writer = new NoCloseStreamWriter(_fileStream);

				// Append the expired cache.
				var expiredCache = _cache
					.Where(x => x.Value.Item2 <= threshold && x.Value.Item1 != null)
					.OrderBy(x => x.Value.Item2)
					.ToDictionary(x => x.Key, x => x.Value);

				// Append all add / updates items over the cache limit.
				var overLimit = _cache
					.Where(x => x.Value.Item1 != null)
					.Where(x => !expiredCache.ContainsKey(x.Key))
					.OrderBy(x => x.Value.Item2)
					.Take(_cache.Count - _cacheLimit)
					.ToDictionary(x => x.Key, x => x.Value);

				while (reader.Peek() > 0)
				{
					var line = reader.ReadLine();
					if (string.IsNullOrWhiteSpace(line))
					{
						continue;
					}

					var delimiter = line.IndexOf("|");
					if (delimiter <= 0)
					{
						continue;
					}

					var key = line.Substring(0, delimiter);

					// See if this was already in the over limit group.
					if (expiredCache.ContainsKey(key) || overLimit.ContainsKey(key))
					{
						// We've already written these items so skip it.
						continue;
					}

					// Check to see if we have an update or delete.
					if (!_cache.ContainsKey(key))
					{
						// We do not have an update or delete so keep the line.
						writer.WriteLine(line);
					}
				}

				// Write all items that have expired in the cache.
				foreach (var change in expiredCache)
				{
					writer.WriteLine(change.Key + "|" + change.Value.Item1);
					_cache.Remove(change.Key);
				}

				// Write all items that are over the cache limit.
				foreach (var change in overLimit)
				{
					writer.WriteLine(change.Key + "|" + change.Value.Item1);
					_cache.Remove(change.Key);
				}

				// Clear all "remove" actions.
				foreach (var item in _cache.Where(x => x.Value.Item1 == null).ToList())
				{
					_cache.Remove(item.Key);
				}

				reader.Dispose();
				writer.Flush();
				writer.Dispose();
			}
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

		public event Action<T> OnEnumerated;

		#endregion
	}
}