#region References

using System.Collections.Generic;

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

		#endregion
	}
}