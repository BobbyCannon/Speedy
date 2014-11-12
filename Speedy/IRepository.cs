#region References

using System.Collections.Generic;

#endregion

namespace Speedy
{
	public interface IRepository
	{
		#region Properties

		/// <summary>
		/// The directory the repository will be located.
		/// </summary>
		string Directory { get; }

		/// <summary>
		/// The name of the repository.
		/// </summary>
		string Name { get; }
		
		#endregion

		#region Methods

		/// <summary>
		/// Clears the repository.
		/// </summary>
		void Clear();

		/// <summary>
		/// Check the provided keys and returns any keys that are missing.
		/// </summary>
		/// <param name="keys">The key that will be tested.</param>
		/// <returns>A list of key value pairs that were not found in the repository.</returns>
		HashSet<string> FindMissingKeys(HashSet<string> keys);

		/// <summary>
		/// Loads items directly into the repository. This will not check the keys so we can
		/// speed up the loading of items. If you need key protection then use Write instead.
		/// </summary>
		/// <param name="items">The items to load into the repository.</param>
		void Load(Dictionary<string, string> items);

		/// <summary>
		/// Read the repository using an enumerator.
		/// </summary>
		/// <returns>The list of key value pairs to enumerate.</returns>
		IEnumerable<KeyValuePair<string, string>> Read();

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key">The key of the item to read.</param>
		/// <returns>The value for the key.</returns>
		/// <exception cref="KeyNotFoundException">Could not find the entry with the key.</exception>
		string Read(string key);

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="keys">The keys of the items to read.</param>
		/// <returns>The value for the keys.</returns>
		Dictionary<string, string> Read(HashSet<string> keys);

		/// <summary>
		/// Removes an item from the repository by the key provided.
		/// </summary>
		/// <param name="key">The key of the item to remove.</param>
		void Remove(string key);

		/// <summary>
		/// Removes items from the repository by the keys provided.
		/// </summary>
		/// <param name="keys">The keys of the items to remove.</param>
		void Remove(HashSet<string> keys);

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key">The key of the item to write.</param>
		/// <param name="value">The value of the item to write.</param>
		void Write(string key, string value);

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items">The list of items to add to the repository.</param>
		void Write(Dictionary<string, string> items);

		/// <summary>
		/// Save the changes to the repository (Writes, Removes, etc).
		/// </summary>
		void Save();

		#endregion
	}
}