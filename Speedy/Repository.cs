#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a repository of key value pairs.
	/// </summary>
	public class Repository : IRepository
	{
		#region Fields

		private readonly Dictionary<string, string> _changes;
		private FileStream _fileStream;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directoryInfo"> The directory info where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		private Repository(DirectoryInfo directoryInfo, string name)
		{
			DirectoryInfo = directoryInfo;
			Name = name;
			FileInfo = new FileInfo(DirectoryInfo + "\\" + Name + ".speedy");
			_changes = new Dictionary<string, string>();
		}

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory where the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		private Repository(string directory, string name)
			: this(new DirectoryInfo(directory), name)
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
		private string TemporaryFullPath => FileInfo.FullName + ".temp";

		#endregion

		#region Methods

		/// <summary>
		/// Clears the repository.
		/// </summary>
		public void Clear()
		{
			lock (_changes)
			{
				_fileStream.SetLength(0);
				_fileStream.Flush(true);
			}
		}

		public static IRepository Create(DirectoryInfo directiory, string name)
		{
			return Create(directiory.FullName, name);
		}

		public static IRepository Create(string directiory, string name)
		{
			var repository = new Repository(directiory, name);
			repository.Initialize();
			return repository;
		}

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
				var foundKeys = new List<string>();

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
		/// Loads items directly into the repository. This will not check the keys so we can
		/// speed up the loading of items. If you need key protection then use Write instead.
		/// </summary>
		/// <param name="items"> The items to load into the repository. </param>
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
				// todo: be sure this still yields
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
				_changes.AddOrUpdate(key, null);
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
				File.Copy(FileInfo.FullName, TemporaryFullPath);
				SaveRepository();
				new FileInfo(TemporaryFullPath).SafeDelete();
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
				_changes.AddOrUpdate(key, value);
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
				_fileStream.Dispose();
			}
		}

		private int GetCount()
		{
			_fileStream.Position = 0;
			var reader = new StreamReader(_fileStream);
			var count = 0;

			while (reader.Peek() > 0)
			{
				reader.ReadLine();
				count++;
			}

			return count;
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
				if (File.Exists(TemporaryFullPath))
				{
					SaveRepository();
				}
			}
		}

		private void SaveRepository()
		{
			using (var tempStream = File.Open(TemporaryFullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
			{
				_fileStream.SetLength(0);
				_fileStream.Flush(true);

				var reader = new NoCloseStreamReader(tempStream);
				var writer = new NoCloseStreamWriter(_fileStream);

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

					// Check to see if we have an update or delete.
					if (!_changes.ContainsKey(key))
					{
						writer.WriteLine(line);
						continue;
					}

					// Read the change and see if was an update. If it was a delete (null) just ignore it.
					var update = _changes[key];
					if (update != null)
					{
						// The change was an update so write it.
						writer.WriteLine(key + "|" + update);
					}

					// Remove the change.
					_changes.Remove(key);
				}

				// Append the remaining changes.
				foreach (var change in _changes)
				{
					writer.WriteLine(change.Key + "|" + change.Value);
				}

				writer.Flush();
				_changes.Clear();
			}
		}

		#endregion
	}
}