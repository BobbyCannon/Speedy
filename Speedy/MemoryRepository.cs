#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy
{
	public class MemoryRepository : IRepository
	{
		#region Fields

		private readonly Dictionary<string, string> _changes;
		private readonly Dictionary<string, string> _directory;

		#endregion

		#region Constructors

		public MemoryRepository(string name)
		{
			Name = name;
			_changes = new Dictionary<string, string>();
			_directory = new Dictionary<string, string>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the repository.
		/// </summary>
		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Clears the repository.
		/// </summary>
		public void Clear()
		{
			_directory.Clear();
		}

		/// <summary>
		/// Check the provided keys and returns any keys that are missing.
		/// </summary>
		/// <param name="keys"> The key that will be tested. </param>
		/// <returns> A list of key value pairs that were not found in the repository. </returns>
		public HashSet<string> FindMissingKeys(HashSet<string> keys)
		{
			return new HashSet<string>(_directory.Keys.Except(keys));
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
				_changes.AddOrUpdate(items);
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
			return _directory;
		}

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <returns> The value for the key. </returns>
		/// <exception cref="KeyNotFoundException"> Could not find the entry with the key. </exception>
		public string Read(string key)
		{
			return _directory[key];
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to read. </param>
		/// <returns> The value for the keys. </returns>
		public IEnumerable<KeyValuePair<string, string>> Read(HashSet<string> keys)
		{
			return _directory.Where(x => keys.Contains(x.Key));
		}

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="condition"> The condition to test each key against. </param>
		/// <returns> The value for the keys that match the condition. </returns>
		public IEnumerable<KeyValuePair<string, string>> Read(Func<string, bool> condition)
		{
			return _directory.Where(x => condition.Invoke(x.Key));
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
		/// Save the changes to the repository (Writes, Removes, etc).
		/// </summary>
		public void Save()
		{
			lock (_changes)
			{
				foreach (var item in _changes.Where(x => x.Value == null).ToList())
				{
					if (_directory.ContainsKey(item.Key))
					{
						_directory.Remove(item.Key);
						_changes.Remove(item.Key);
					}
				}

				foreach (var item in _changes.ToList())
				{
					_directory.AddOrUpdate(item.Key, item.Value);
					_changes.Remove(item.Key);
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
				_changes.AddOrUpdate(key, value);
			}
		}

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items"> The list of items to add to the repository. </param>
		public void Write(Dictionary<string, string> items)
		{
			lock (_changes)
			{
				_changes.AddOrUpdate(items);
			}
		}

		#endregion
	}
}