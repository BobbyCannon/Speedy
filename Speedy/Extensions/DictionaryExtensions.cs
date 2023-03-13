#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Serialization;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for dictionary
/// </summary>
public static class DictionaryExtensions
{
	#region Methods

	/// <summary>
	/// Add a dictionary entry if the key is not found.
	/// </summary>
	/// <typeparam name="T1"> The type of the key. </typeparam>
	/// <typeparam name="T2"> The type of the value. </typeparam>
	/// <param name="dictionary"> The dictionary to update. </param>
	/// <param name="key"> The value of the key. </param>
	/// <param name="value"> The value of the value. </param>
	public static void AddIfMissing<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 value)
	{
		if (dictionary.ContainsKey(key))
		{
			return;
		}

		dictionary.Add(key, value);
	}
	
	/// <summary>
	/// Add or update a dictionary entry if the key is not found in the source.
	/// </summary>
	/// <typeparam name="T1"> The type of the key. </typeparam>
	/// <typeparam name="T2"> The type of the value. </typeparam>
	/// <param name="dictionary"> The dictionary to update. </param>
	/// <param name="key"> The value of the key. </param>
	/// <param name="source"> The dictionary for source values. </param>
	public static void AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, IDictionary<T1, T2> source)
	{
		if (!source.ContainsKey(key))
		{
			return;
		}

		dictionary.AddOrUpdate(key, source[key]);
	}
	
	/// <summary>
	/// Add or update an entry in a dictionary.
	/// </summary>
	/// <typeparam name="T1"> The key value type. </typeparam>
	/// <typeparam name="T2"> The value value type. </typeparam>
	/// <param name="dictionary"> The dictionary to add or update. </param>
	/// <param name="value"> The value to add or update. </param>
	/// <returns> The dictionary that was update. </returns>
	public static Dictionary<T1, T2> AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dictionary, KeyValuePair<T1, T2> value)
	{
		if (dictionary.ContainsKey(value.Key))
		{
			dictionary[value.Key] = value.Value;
			return dictionary;
		}

		dictionary.Add(value.Key, value.Value);
		return dictionary;
	}

	/// <summary>
	/// Add or update a dictionary entry.
	/// </summary>
	/// <typeparam name="T1"> The type of the key. </typeparam>
	/// <typeparam name="T2"> The type of the value. </typeparam>
	/// <param name="dictionary"> The dictionary to update. </param>
	/// <param name="key"> The value of the key. </param>
	/// <param name="value"> The value of the value. </param>
	public static void AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 value)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
			return;
		}

		dictionary.Add(key, value);
	}

	/// <summary>
	/// Add or update a dictionary entry.
	/// </summary>
	/// <typeparam name="T1"> The type of the key. </typeparam>
	/// <typeparam name="T2"> The type of the value. </typeparam>
	/// <param name="dictionary"> The dictionary to update. </param>
	/// <param name="key"> The value of the key. </param>
	/// <param name="get"> The function to get the value. </param>
	/// <param name="update"> The function to update the value. </param>
	public static void AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, Func<T2> get, Func<T2, T2> update)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = update(dictionary[key]);
			return;
		}

		dictionary.Add(key, get());
	}

	/// <summary>
	/// Deep clone a dictionary of items. Will use the ICloneable interface if available.
	/// </summary>
	/// <typeparam name="T"> The key type. </typeparam>
	/// <typeparam name="T2"> The value type. </typeparam>
	/// <param name="dictionary"> The dictionary to clone. </param>
	/// <returns> The clone dictionary. </returns>
	public static IDictionary<T, T2> DeepClone<T, T2>(this IDictionary<T, T2> dictionary) where T2 : new()
	{
		var response = new Dictionary<T, T2>();
		foreach (var item in dictionary)
		{
			if (item.Value is ICloneable cloneable)
			{
				response.Add(item.Key, (T2) cloneable.DeepClone());
			}
			else
			{
				// ReSharper disable once InvokeAsExtensionMethod
				response.Add(item.Key, Serializer.DeepClone(item.Value));
			}
		}
		return response;
	}

	/// <summary>
	/// Deep clone a dictionary of items. Will use the ICloneable interface if available.
	/// </summary>
	/// <typeparam name="T"> The key type. </typeparam>
	/// <typeparam name="T2"> The value type. </typeparam>
	/// <param name="dictionary"> The dictionary to clone. </param>
	/// <returns> The clone dictionary. </returns>
	public static Dictionary<T, T2> DeepClone<T, T2>(this Dictionary<T, T2> dictionary)
	{
		if (dictionary == null)
		{
			return null;
		}

		var response = new Dictionary<T, T2>();
		foreach (var item in dictionary)
		{
			if (item.Value is ICloneable cloneable)
			{
				response.Add(item.Key, (T2) cloneable.DeepClone());
			}
			else
			{
				// ReSharper disable once InvokeAsExtensionMethod
				response.Add(item.Key, Serializer.DeepClone(item.Value));
			}
		}
		return response;
	}

	/// <summary>
	/// Get a value if the key is found otherwise create a new item, add to dictionary, then return.
	/// </summary>
	/// <typeparam name="T"> The key type. </typeparam>
	/// <typeparam name="T2"> The value type. </typeparam>
	/// <param name="dictionary"> The dictionary to process. </param>
	/// <param name="key"> The key value. </param>
	/// <param name="create"> The function to create a new value. </param>
	/// <returns> </returns>
	public static T2 GetOrAdd<T, T2>(this IDictionary<T, T2> dictionary, T key, Func<T2> create)
	{
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}

		var response = create();
		dictionary.Add(key, response);
		return response;
	}

	/// <summary>
	/// Tries to get a value or returns the default value.
	/// </summary>
	/// <typeparam name="T"> The key type. </typeparam>
	/// <typeparam name="T2"> The value type. </typeparam>
	/// <param name="dictionary"> The dictionary to check. </param>
	/// <param name="key"> The key to check for. </param>
	/// <returns> The value that was found or default value </returns>
	public static T2 GetValue<T, T2>(this IDictionary<T, T2> dictionary, T key)
	{
		return dictionary.ContainsKey(key) ? dictionary[key] : default;
	}

	/// <summary>
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="TKey"> The type of the key for the dictionary. </typeparam>
	/// <typeparam name="TValue"> The type of the value for the dictionary. </typeparam>
	/// <param name="dictionary"> The left collection. </param>
	/// <param name="updates"> The right collection. </param>
	public static void Reconcile<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> updates)
	{
		// Reconcile two collections
		var keysToAdd = updates.Keys.Where(x => !dictionary.ContainsKey(x)).ToList();
		var updateToBeApplied = updates
			.Where(x => dictionary.ContainsKey(x.Key) && !Equals(dictionary[x.Key], updates[x.Key]))
			.ToList();
		var keysToRemove = dictionary.Keys.Where(x => !updates.ContainsKey(x)).ToList();

		foreach (var key in keysToAdd)
		{
			dictionary.Add(key, updates[key]);
		}

		foreach (var updateToApply in updateToBeApplied)
		{
			// todo: support IUpdatable or use reflection?
			// todo: references should not be used?
			dictionary[updateToApply.Key] = updateToApply.Value;
		}

		foreach (var key in keysToRemove)
		{
			dictionary.Remove(key);
		}
	}

	/// <summary>
	/// Tries to get a value or returns the default value.
	/// </summary>
	/// <typeparam name="T"> The key type. </typeparam>
	/// <typeparam name="T2"> The value type. </typeparam>
	/// <param name="dictionary"> The dictionary to check. </param>
	/// <param name="key"> The key to check for. </param>
	/// <param name="value"> The value that was found or default value. </param>
	/// <returns> True if the value exist otherwise false. </returns>
	public static bool TryGetValue<T, T2>(this IDictionary<T, T2> dictionary, T key, out T2 value)
	{
		if (dictionary.ContainsKey(key))
		{
			value = dictionary[key];
			return true;
		}

		value = default;
		return false;
	}

	#endregion
}