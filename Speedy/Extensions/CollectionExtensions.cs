#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Speedy.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for collections.
	/// </summary>
	public static class CollectionExtensions
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
		/// Add multiple items to a collection
		/// </summary>
		/// <param name="set"> The set to add items to. </param>
		/// <param name="items"> The items to add. </param>
		/// <typeparam name="T"> The type of the items in the collection. </typeparam>
		public static void AddRange<T>(this ICollection<T> set, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				set.Add(item);
			}
		}

		/// <summary>
		/// Appends new values to an existing HashSet.
		/// </summary>
		/// <typeparam name="T"> The type of value in the set. </typeparam>
		/// <param name="set"> The set to append to. </param>
		/// <param name="values"> The values to add. </param>
		/// <returns> A new HashSet containing the new values. </returns>
		public static HashSet<T> Append<T>(this HashSet<T> set, params T[] values)
		{
			return new(set.Union(values));
		}

		/// <summary>
		/// Appends new values to an existing HashSet.
		/// </summary>
		/// <typeparam name="T"> The type of value in the set. </typeparam>
		/// <param name="set"> The set to append to. </param>
		/// <param name="values"> The values to add. </param>
		/// <returns> A new HashSet containing the new values. </returns>
		public static HashSet<T> Append<T>(this HashSet<T> set, HashSet<T> values)
		{
			return new(set.Union(values));
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
				if (item.Value is Serialization.ICloneable cloneable)
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
			var response = new Dictionary<T, T2>();
			foreach (var item in dictionary)
			{
				if (item.Value is Serialization.ICloneable cloneable)
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
		/// Execute the action on each entity in the collection.
		/// </summary>
		/// <param name="items"> The collection of items to process. </param>
		/// <param name="action"> The action to execute for each item. </param>
		public static void ForEach(this IEnumerable items, Action<object> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		/// Execute the action on each entity in the collection.
		/// </summary>
		/// <typeparam name="T"> The type of item in the collection. </typeparam>
		/// <param name="items"> The collection of items to process. </param>
		/// <param name="action"> The action to execute for each item. </param>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		/// Natural sort a string collection.
		/// </summary>
		/// <param name="collection"> The collection to sort. </param>
		/// <returns> The sorted collection. </returns>
		public static IEnumerable<string> NaturalSort(this IEnumerable<string> collection)
		{
			return NaturalSort(collection, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Natural sort a string collection.
		/// </summary>
		/// <param name="collection"> The collection to sort. </param>
		/// <param name="cultureInfo"> The culture information to use during sort. </param>
		/// <returns> The sorted collection. </returns>
		public static IEnumerable<string> NaturalSort(this IEnumerable<string> collection, CultureInfo cultureInfo)
		{
			return collection.OrderBy(s => s, new SyncKeyComparer(cultureInfo));
		}

		/// <summary>
		/// Safely convert an enumeration to a list without worry about "InvalidOperationException" due to collection being modified.
		/// </summary>
		/// <typeparam name="T"> The type in the collection. </typeparam>
		/// <param name="values"> The enumeration to convert to a list. </param>
		/// <returns> The values in a list format. </returns>
		public static IList<T> ToListSafe<T>(this IEnumerable<T> values)
		{
			while (true)
			{
				try
				{
					// ReSharper disable once PossibleMultipleEnumeration
					return values.ToList();
				}
				catch (InvalidOperationException)
				{
					// Ignore "Collection was modified"
				}
			}
		}

		/// <summary>
		/// Safely convert an enumeration to a list without worry about "InvalidOperationException" due to collection being modified.
		/// </summary>
		/// <typeparam name="T"> The type of the entity of the collection. </typeparam>
		/// <typeparam name="T2"> The type of the entity key. </typeparam>
		/// <param name="values"> The enumeration to convert to a list. </param>
		/// <returns> The values in a list format. </returns>
		public static IList<T> ToListSafe<T, T2>(this IRepository<T, T2> values) where T : Entity<T2>
		{
			while (true)
			{
				try
				{
					// ReSharper disable once PossibleMultipleEnumeration
					return values.ToList();
				}
				catch (InvalidOperationException)
				{
					// Ignore "Collection was modified"
				}
			}
		}

		#endregion
	}
}