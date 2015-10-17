#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a memory / file repository of key value pairs.
	/// </summary>
	public class Repository : IRepository
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
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout"> The amount of time to cache items in memory before persisting to disk. Defaults to null and then TimeSpan.Zero is used. </param>
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

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout"> The amount of time to cache items in memory before persisting to disk. Defaults to null and then TimeSpan.Zero is used. </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		private Repository(string directory, string name, TimeSpan? timeout = null, int limit = 0)
			: this(new DirectoryInfo(directory), name, timeout, limit)
		{
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
		/// <param name="timeout"> The amount of time to cache items in memory before persisting to disk. Defaults to null and then TimeSpan.Zero is used. </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public static IRepository Create(DirectoryInfo directoryInfo, string name, TimeSpan? timeout = null, int limit = 0)
		{
			return Create(directoryInfo.FullName, name, timeout, limit);
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		/// <param name="timeout"> The amount of time to cache items in memory before persisting to disk. Defaults to null and then TimeSpan.Zero is used. </param>
		/// <param name="limit"> The maximum limit of items to be cached in memory. Defaults to a limit of 0. </param>
		public static IRepository Create(string directory, string name, TimeSpan? timeout = null, int limit = 0)
		{
			var repository = new Repository(directory, name, timeout, limit);
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
		/// Check the provided keys and returns any keys that are missing.
		/// </summary>
		/// <param name="keys"> The key that will be tested. </param>
		/// <returns> A list of key value pairs that were not found in the repository. </returns>
		public HashSet<string> FindMissingKeys(HashSet<string> keys)
		{
			lock (_changes)
			{
				_fileStream.Position = 0;
				var reader = new StreamReader(_fileStream);
				var foundKeys = _cache.Keys.ToList();

				while (reader.Peek() > 0)
				{
					var line = reader.ReadLine();
					if (line == null)
					{
						continue;
					}

					var delimiter = line.IndexOf("|");
					if (delimiter <= 0)
					{
						continue;
					}

					var readKey = line.Substring(0, delimiter);
					if (keys.Contains(readKey))
					{
						foundKeys.Add(readKey);
					}
				}

				return new HashSet<string>(keys.Except(foundKeys));
			}
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
		/// Loads items directly into the repository. This will not check the keys so we can
		/// speed up the loading of items.
		/// </summary>
		/// <param name="items"> The items to load into the repository. </param>
		/// <remarks> Will not be cached. These items will be written directly to disk. </remarks>
		/// <remarks> If you need key protection then use Write instead. </remarks>
		public void Load(Dictionary<string, string> items)
		{
			lock (_changes)
			{
				_fileStream.Position = _fileStream.Length;
				var writer = new StreamWriter(_fileStream);

				foreach (var item in items)
				{
					writer.WriteLine(item.Key + "|" + item.Value);
				}

				writer.Flush();
			}
		}

		/// <summary>
		/// Read the repository using an enumerator.
		/// </summary>
		/// <returns> The list of key value pairs to enumerate. </returns>
		/// <remarks>
		/// Must be IEnumerable so we can yield the return.
		/// </remarks>
		public IEnumerable<KeyValuePair<string, string>> Read()
		{
			lock (_changes)
			{
				foreach (var item in _cache.Where(x => x.Value.Item1 != null))
				{
					yield return new KeyValuePair<string, string>(item.Key, item.Value.Item1);
				}

				_fileStream.Position = 0;
				var reader = new StreamReader(_fileStream);

				while (reader.Peek() > 0)
				{
					var line = reader.ReadLine();
					if (line == null)
					{
						continue;
					}

					var delimiter = line.IndexOf("|");
					if (delimiter <= 0)
					{
						continue;
					}

					var readKey = line.Substring(0, delimiter);
					yield return new KeyValuePair<string, string>(readKey, line.Substring(delimiter + 1, line.Length - delimiter - 1));
				}
			}
		}

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <returns> The value for the key. </returns>
		/// <exception cref="KeyNotFoundException"> Could not find the entry with the key. </exception>
		public string Read(string key)
		{
			var response = Read(new HashSet<string> { key }).ToList();
			if (response.Count <= 0)
			{
				throw new KeyNotFoundException("Could not find the entry with the key.");
			}

			return response.First().Value;
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to read. </param>
		/// <returns> The value for the keys. </returns>
		public IEnumerable<KeyValuePair<string, string>> Read(HashSet<string> keys)
		{
			return Read(keys.Contains);
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="condition"> The condition to test each key against. </param>
		/// <returns> The value for the keys that match the condition. </returns>
		public IEnumerable<KeyValuePair<string, string>> Read(Func<string, bool> condition)
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
		public bool TryRead(string key, out string value)
		{
			var response = Read(new HashSet<string> { key }).FirstOrDefault();
			if (response.Key != key)
			{
				value = string.Empty;
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
		public void Write(string key, string value)
		{
			lock (_changes)
			{
				_changes.AddOrUpdate(key, new Tuple<string, DateTime>(value, DateTime.UtcNow));
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

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Flush();
				_fileStream.Dispose();
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
				_fileStream.Position = 0;
				var reader = new StreamReader(_fileStream);
				var count = _cache.Count;

				while (reader.Peek() > 0)
				{
					reader.ReadLine();
					count++;
				}

				return count;
			}
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
			if (!FileInfo.Exists)
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
				var expiredCache = _cache.Where(x => x.Value.Item2 <= threshold && x.Value.Item1 != null).ToDictionary(x => x.Key, x => x.Value);
				foreach (var change in expiredCache)
				{
					writer.WriteLine(change.Key + "|" + change.Value.Item1);
					_cache.Remove(change.Key);
				}

				// Append all items over the cache limit.
				var overLimit = _cache.Skip(_cacheLimit).Where(x => x.Value.Item1 != null).ToDictionary(x => x.Key, x => x.Value);
				foreach (var change in overLimit)
				{
					writer.WriteLine(change.Key + "|" + change.Value.Item1);
					_cache.Remove(change.Key);
				}

				while (reader.Peek() > 0)
				{
					var line = reader.ReadLine();
					if (line == null)
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

				writer.Flush();
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
	}
}