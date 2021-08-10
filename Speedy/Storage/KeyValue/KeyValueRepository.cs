#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Streams;

#endregion

namespace Speedy.Storage.KeyValue
{
	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class KeyValueRepository : KeyValueRepository<string>
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
		public KeyValueRepository(string directory, string name, TimeSpan? timeout = null, int limit = 0)
			: base(directory, name, timeout, limit)
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		public KeyValueRepository(string directory, string name, KeyValueRepositoryOptions options)
			: base(directory, name, options)
		{
		}

		#endregion
	}

	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class KeyValueRepository<T> : IKeyValueRepository<T>
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
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		public KeyValueRepository(string directory, string name, KeyValueRepositoryOptions options)
			: this(new DirectoryInfo(directory), name, options)
		{
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
		public KeyValueRepository(string directory, string name, TimeSpan? timeout = null, int limit = 0)
			: this(directory, name, new KeyValueRepositoryOptions { Limit = limit, Timeout = timeout ?? TimeSpan.Zero })
		{
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		protected KeyValueRepository(DirectoryInfo directoryInfo, string name, KeyValueRepositoryOptions options)
		{
			_options = options;
			_cache = new Dictionary<string, Tuple<string, DateTime, ulong>>(_options.Limit == int.MaxValue ? 4096 : _options.Limit);
			_changes = new Dictionary<string, Tuple<string, DateTime, ulong>>();
			_settings = new SerializerSettings(false, false, false, true);
			_writeIndex = 0;

			DirectoryInfo = directoryInfo;
			Name = name;
			FileInfo = new FileInfo($"{DirectoryInfo.FullName}\\{Name}.speedy");
			TempFileInfo = new FileInfo($"{FileInfo.FullName}.temp");
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

				using (var stream = OpenFile(FileInfo, false))
				{
					stream.SetLength(0);
					stream.Flush(true);
				}
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
		/// <returns> The repository. </returns>
		public static IKeyValueRepository<T> Create(DirectoryInfo directoryInfo, string name, TimeSpan? timeout = null, int limit = 0)
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
		/// <returns> The repository. </returns>
		public static IKeyValueRepository<T> Create(string directory, string name, TimeSpan? timeout = null, int limit = 0)
		{
			var options = new KeyValueRepositoryOptions { Limit = limit, Timeout = timeout ?? TimeSpan.Zero };
			return Create(directory, name, options);
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="options"> The options for the repository. </param>
		public static IKeyValueRepository<T> Create(string directory, string name, KeyValueRepositoryOptions options)
		{
			var repository = new KeyValueRepository<T>(directory, name, options);
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
				using (var stream = OpenFile(FileInfo, false))
				{
					stream.Position = stream.Length;
					var writer = new NoCloseStreamWriter(stream);

					foreach (var item in items)
					{
						writer.WriteLine(item.Key + "|" + item.Value.ToJson(_settings));
					}

					writer.Flush();
					writer.Dispose();
				}
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
				foreach (var item in _cache.OrderBy(x => x.Value.Item3).Where(x => x.Value.Item1 != null))
				{
					yield return new KeyValuePair<string, T>(item.Key, item.Value.Item1.FromJson<T>());
				}

				if (_options.Limit == int.MaxValue)
				{
					yield break;
				}

				using (var stream = OpenFile(FileInfo, true))
				{
					var reader = new StreamReader(stream);

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

						var item = line.Substring(delimiter + 1, line.Length - delimiter - 1).FromJson<T>();
						(item as IEntity)?.TrySetId(readKey);

						yield return new KeyValuePair<string, T>(readKey, item);
					}
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
		/// Reads items from disk into the cache. This will not check the keys so we can speed up the loading of items.
		/// </summary>
		public void Refresh()
		{
			lock (_changes)
			{
				using var stream = OpenFile(FileInfo, true);
				var reader = new StreamReader(stream);

				while (_cache.Count <= _options.Limit && reader.Peek() > 0)
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
					_cache.AddOrUpdate(readKey, new Tuple<string, DateTime, ulong>(line.Substring(delimiter + 1, line.Length - delimiter - 1), TimeService.UtcNow, _writeIndex++));
				}
			}
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

				var threshold = TimeService.UtcNow - _options.Timeout;
				if (_cache.Count <= _options.Limit && !_cache.Any(x => x.Value.Item2 <= threshold))
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
		/// Initializes the path the repository is to be located.
		/// </summary>
		private void Initialize()
		{
			lock (_changes)
			{
				DirectoryInfo.SafeCreate();
				FileInfo.SafeCreate();

				// Check to see if we were in the middle of a save.
				TempFileInfo.Refresh();
				if (TempFileInfo.Exists)
				{
					// Finish our save.
					SaveRepository(TimeService.UtcNow);
				}

				// Load everything from disk.
				Refresh();
			}
		}

		private FileStream OpenFile(FileInfo info, bool reading)
		{
			var fileAccess = reading ? FileAccess.Read : FileAccess.ReadWrite;
			var fileShare = reading ? FileShare.Read : FileShare.None;
			return UtilityExtensions.Retry(() => info.Open(FileMode.OpenOrCreate, fileAccess, fileShare), (int) _options.Timeout.TotalMilliseconds, 50);
		}

		private void ProcessCache(NoCloseStreamReader reader, NoCloseStreamWriter writer)
		{
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

				// Check to see if we have an update or delete.
				if (!_cache.ContainsKey(key))
				{
					// We do not have an update or delete so keep the line.
					writer.WriteLine(line);
				}
			}

			// Write all items in the cache.
			foreach (var change in _cache)
			{
				writer.WriteLine(change.Key + "|" + change.Value.Item1);
			}

			// Clear all "remove" actions.
			foreach (var item in _cache.Where(x => x.Value.Item1 == null).ToList())
			{
				_cache.Remove(item.Key);
			}
		}

		private void ProcessLimitedCache(DateTime threshold, NoCloseStreamReader reader, NoCloseStreamWriter writer)
		{
			// Append the expired cache.
			var expiredCache = _cache
				.Where(x => x.Value.Item2 <= threshold && x.Value.Item1 != null)
				.OrderBy(x => x.Value.Item3)
				.ToDictionary(x => x.Key, x => x.Value);

			// Append all add / updates items over the cache limit.
			var overLimit = _cache
				.Where(x => x.Value.Item1 != null)
				.Where(x => !expiredCache.ContainsKey(x.Key))
				.OrderBy(x => x.Value.Item3)
				.Take(_cache.Count - _options.Limit)
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
		}

		/// <summary>
		/// Saves items to the repository. Including items over the cache limit and any that have expired due to the cache timeout.
		/// </summary>
		/// <param name="threshold"> The date time threshold that was calculated from the cache timeout. </param>
		private void SaveRepository(DateTime threshold)
		{
			if (_options.ReadOnly)
			{
				return;
			}

			FileInfo.Refresh();

			if (!FileInfo.Exists)
			{
				// Will only happen if someone has deleted the repository.
				return;
			}

			using (var fileStream = OpenFile(FileInfo, false))
			{
				var tempStream = TempFileInfo.Exists ? TempFileInfo.OpenRead() : fileStream.OpenAndCopyTo(TempFileInfo, (int) _options.Timeout.TotalMilliseconds);

				using (tempStream)
				{
					fileStream.SetLength(0);
					fileStream.Flush(true);

					var reader = new NoCloseStreamReader(tempStream);
					var writer = new NoCloseStreamWriter(fileStream);

					if (_options.Limit >= int.MaxValue)
					{
						ProcessCache(reader, writer);
					}
					else
					{
						ProcessLimitedCache(threshold, reader, writer);
					}

					reader.Dispose();
					writer.Flush();
					writer.Dispose();
				}
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

		/// <summary>
		/// Will be called on each item when this repository is enumerated.
		/// </summary>
		public event Action<T> OnEnumerated;

		#endregion
	}
}