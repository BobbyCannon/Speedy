#region References

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

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the Repository class.
		/// </summary>
		/// <param name="directory"> The directory the repository will reside. </param>
		/// <param name="name"> The name of the repository. </param>
		public Repository(string directory, string name)
		{
			Directory = directory;
			Name = name;

			_changes = new Dictionary<string, string>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The directory the repository will be located.
		/// </summary>
		public string Directory { get; private set; }

		/// <summary>
		/// The name of the repository.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the full path to the repository file.
		/// </summary>
		private string DataFullPath => Directory + "\\" + Name + ".data";

		/// <summary>
		/// Gets the full path to the temporary repository file.
		/// </summary>
		private string TemporaryFullPath => DataFullPath.Replace(".data", ".temp");

		#endregion

		#region Methods

		/// <summary>
		/// Clears the repository.
		/// </summary>
		public void Clear()
		{
			Initialize();
			if (File.Exists(DataFullPath))
			{
				File.Delete(DataFullPath);
			}
		}

		/// <summary>
		/// Check the provided keys and returns any keys that are missing.
		/// </summary>
		/// <param name="keys"> The key that will be tested. </param>
		/// <returns> A list of key value pairs that were not found in the repository. </returns>
		public HashSet<string> FindMissingKeys(HashSet<string> keys)
		{
			Initialize();

			using (var stream = File.Open(DataFullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
			{
				var reader = new StreamReader(stream);
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
			using (var stream = File.Open(DataFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				stream.Position = stream.Length;
				var writer = new StreamWriter(stream);

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
		public IEnumerable<KeyValuePair<string, string>> Read()
		{
			Initialize();

			using (var stream = File.Open(DataFullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
			{
				var reader = new StreamReader(stream);

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
			Initialize();

			using (var stream = File.Open(DataFullPath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
			{
				var reader = new StreamReader(stream);

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
						yield return new KeyValuePair<string, string>(readKey, line.Substring(delimiter + 1, line.Length - delimiter - 1));
					}
				}
			}
		}

		/// <summary>
		/// Removes an item from the repository by the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to remove. </param>
		public void Remove(string key)
		{
			if (_changes.ContainsKey(key))
			{
				_changes.Remove(key);
			}

			_changes.Add(key, null);
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
			Initialize();

			using (var stream = File.Open(DataFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
			{
				using (var stream2 = File.Open(TemporaryFullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
				{
					var reader = new StreamReader(stream);
					var writer = new StreamWriter(stream2);

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
							_changes.Remove(key);
						}
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

			File.Delete(DataFullPath);
			File.Move(TemporaryFullPath, DataFullPath);
		}

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key"> The key of the item to write. </param>
		/// <param name="value"> The value of the item to write. </param>
		public void Write(string key, string value)
		{
			if (_changes.ContainsKey(key))
			{
				_changes.Remove(key);
			}

			_changes.Add(key, value);
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
		/// Initializes the path the repository is to be located.
		/// </summary>
		private void Initialize()
		{
			if (!System.IO.Directory.Exists(Directory))
			{
				System.IO.Directory.CreateDirectory(Directory);
			}
		}

		#endregion
	}
}