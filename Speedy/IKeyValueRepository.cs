#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a repository of key value pairs.
	/// </summary>
	public interface IKeyValueRepository<T> : IEnumerable<T>, IDisposable
	{
		#region Properties

		/// <summary>
		/// The number of items in the repository.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// The name of the repository.
		/// </summary>
		string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Archives the repository.
		/// </summary>
		void Archive();

		/// <summary>
		/// Clears the repository.
		/// </summary>
		void Clear();

		/// <summary>
		/// Delete the repository.
		/// </summary>
		void Delete();

		/// <summary>
		/// Flushes all cached items to storage.
		/// </summary>
		void Flush();

		/// <summary>
		/// Loads items directly into the repository. This will not check the keys so we can
		/// speed up the loading of items. If you need key protection then use Write instead.
		/// </summary>
		/// <param name="items"> The items to load into the repository. </param>
		void Load(Dictionary<string, T> items);

		/// <summary>
		/// Read the repository using an enumerator.
		/// </summary>
		/// <returns> The list of key value pairs to enumerate. </returns>
		/// <remarks>
		/// Must be IEnumerable so we can yield the return.
		/// </remarks>
		IEnumerable<KeyValuePair<string, T>> Read();

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <returns> The value for the key. </returns>
		/// <exception cref="KeyNotFoundException"> Could not find the entry with the key. </exception>
		T Read(string key);

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to read. </param>
		/// <returns> The value for the keys. </returns>
		IEnumerable<KeyValuePair<string, T>> Read(HashSet<string> keys);

		/// <summary>
		/// Reads a set of items from the repository based on the keys provided.
		/// </summary>
		/// <param name="condition"> The condition to test each key against. </param>
		/// <returns> The value for the keys that match the condition. </returns>
		IEnumerable<KeyValuePair<string, T>> Read(Func<string, bool> condition);

		/// <summary>
		/// Read all the keys from the repository.
		/// </summary>
		/// <returns> The keys for the repository. </returns>
		IEnumerable<string> ReadKeys();

		/// <summary>
		/// Removes an item from the repository by the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to remove. </param>
		void Remove(string key);

		/// <summary>
		/// Removes items from the repository by the keys provided.
		/// </summary>
		/// <param name="keys"> The keys of the items to remove. </param>
		void Remove(HashSet<string> keys);

		/// <summary>
		/// Save the changes to the repository (Writes, Removes, etc).
		/// </summary>
		void Save();

		/// <summary>
		/// Read an item from the repository based on the key provided.
		/// </summary>
		/// <param name="key"> The key of the item to read. </param>
		/// <param name="value"> The value read. </param>
		/// <returns> True if the key was found or false if otherwise.. </returns>
		bool TryRead(string key, out T value);

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key"> The key of the item to write. </param>
		/// <param name="value"> The value of the item to write. </param>
		void Write(string key, T value);

		/// <summary>
		/// Writes an item to the repository.
		/// </summary>
		/// <param name="key"> The key of the item to write. </param>
		/// <param name="value"> The value of the item to write. </param>
		void Write(string key, string value);

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items"> The list of items to add to the repository. </param>
		void Write(Dictionary<string, T> items);

		/// <summary>
		/// Writes a collection of items to the repository.
		/// </summary>
		/// <param name="items"> The list of items to add to the repository. </param>
		void Write(Dictionary<string, string> items);

		#endregion

		#region Events

		/// <summary>
		/// Event for when an item is enumerated.
		/// </summary>
		event Action<T> OnEnumerated;

		#endregion
	}
}