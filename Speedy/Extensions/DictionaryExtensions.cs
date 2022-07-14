#region References

using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for dictionary
	/// </summary>
	public static class DictionaryExtensions
	{
		#region Methods

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

		#endregion
	}
}