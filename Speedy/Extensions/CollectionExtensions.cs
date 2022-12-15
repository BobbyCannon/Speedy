#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using Speedy.Storage;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for collections.
/// </summary>
public static class CollectionExtensions
{
	#region Fields

	private static readonly ushort[] _crcTable =
	{
		0x0000, 0x1189, 0x2312, 0x329B, 0x4624, 0x57AD, 0x6536, 0x74BF,
		0x8C48, 0x9DC1, 0xAF5A, 0xBED3, 0xCA6C, 0xDBE5, 0xE97E, 0xF8F7,
		0x1081, 0x0108, 0x3393, 0x221A, 0x56A5, 0x472C, 0x75B7, 0x643E,
		0x9CC9, 0x8D40, 0xBFDB, 0xAE52, 0xDAED, 0xCB64, 0xF9FF, 0xE876,
		0x2102, 0x308B, 0x0210, 0x1399, 0x6726, 0x76AF, 0x4434, 0x55BD,
		0xAD4A, 0xBCC3, 0x8E58, 0x9FD1, 0xEB6E, 0xFAE7, 0xC87C, 0xD9F5,
		0x3183, 0x200A, 0x1291, 0x0318, 0x77A7, 0x662E, 0x54B5, 0x453C,
		0xBDCB, 0xAC42, 0x9ED9, 0x8F50, 0xFBEF, 0xEA66, 0xD8FD, 0xC974,
		0x4204, 0x538D, 0x6116, 0x709F, 0x0420, 0x15A9, 0x2732, 0x36BB,
		0xCE4C, 0xDFC5, 0xED5E, 0xFCD7, 0x8868, 0x99E1, 0xAB7A, 0xBAF3,
		0x5285, 0x430C, 0x7197, 0x601E, 0x14A1, 0x0528, 0x37B3, 0x263A,
		0xDECD, 0xCF44, 0xFDDF, 0xEC56, 0x98E9, 0x8960, 0xBBFB, 0xAA72,
		0x6306, 0x728F, 0x4014, 0x519D, 0x2522, 0x34AB, 0x0630, 0x17B9,
		0xEF4E, 0xFEC7, 0xCC5C, 0xDDD5, 0xA96A, 0xB8E3, 0x8A78, 0x9BF1,
		0x7387, 0x620E, 0x5095, 0x411C, 0x35A3, 0x242A, 0x16B1, 0x0738,
		0xFFCF, 0xEE46, 0xDCDD, 0xCD54, 0xB9EB, 0xA862, 0x9AF9, 0x8B70,
		0x8408, 0x9581, 0xA71A, 0xB693, 0xC22C, 0xD3A5, 0xE13E, 0xF0B7,
		0x0840, 0x19C9, 0x2B52, 0x3ADB, 0x4E64, 0x5FED, 0x6D76, 0x7CFF,
		0x9489, 0x8500, 0xB79B, 0xA612, 0xD2AD, 0xC324, 0xF1BF, 0xE036,
		0x18C1, 0x0948, 0x3BD3, 0x2A5A, 0x5EE5, 0x4F6C, 0x7DF7, 0x6C7E,
		0xA50A, 0xB483, 0x8618, 0x9791, 0xE32E, 0xF2A7, 0xC03C, 0xD1B5,
		0x2942, 0x38CB, 0x0A50, 0x1BD9, 0x6F66, 0x7EEF, 0x4C74, 0x5DFD,
		0xB58B, 0xA402, 0x9699, 0x8710, 0xF3AF, 0xE226, 0xD0BD, 0xC134,
		0x39C3, 0x284A, 0x1AD1, 0x0B58, 0x7FE7, 0x6E6E, 0x5CF5, 0x4D7C,
		0xC60C, 0xD785, 0xE51E, 0xF497, 0x8028, 0x91A1, 0xA33A, 0xB2B3,
		0x4A44, 0x5BCD, 0x6956, 0x78DF, 0x0C60, 0x1DE9, 0x2F72, 0x3EFB,
		0xD68D, 0xC704, 0xF59F, 0xE416, 0x90A9, 0x8120, 0xB3BB, 0xA232,
		0x5AC5, 0x4B4C, 0x79D7, 0x685E, 0x1CE1, 0x0D68, 0x3FF3, 0x2E7A,
		0xE70E, 0xF687, 0xC41C, 0xD595, 0xA12A, 0xB0A3, 0x8238, 0x93B1,
		0x6B46, 0x7ACF, 0x4854, 0x59DD, 0x2D62, 0x3CEB, 0x0E70, 0x1FF9,
		0xF78F, 0xE606, 0xD49D, 0xC514, 0xB1AB, 0xA022, 0x92B9, 0x8330,
		0x7BC7, 0x6A4E, 0x58D5, 0x495C, 0x3DE3, 0x2C6A, 0x1EF1, 0x0F78
	};

	#endregion

	#region Methods

	/// <summary>
	/// Add the item or replace the item with a new 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="collection"></param>
	/// <param name="lookup"></param>
	/// <param name="create"></param>
	/// <param name="replace"></param>
	/// <returns></returns>
	public static T AddOrReplace<T>(this ICollection<T> collection, Func<T, bool> lookup, Func<T> create, Func<T, T> replace)
	{
		var foundItem = collection.FirstOrDefault(lookup);
		
		// See if the item was found
		if (Equals(foundItem, default(T)))
		{
			// Item was not found so just add
			var result = create();
			collection.Add(result);
			return result;
		}
		else
		{
			// Item existed so replace it
			var result = replace(foundItem);
			collection.Remove(foundItem);
			collection.Add(result);
			return result;
		}
	}

	/// <summary>
	/// Add or update the value in the HTTP headers collection.
	/// </summary>
	/// <param name="headers"> The headers to be updated. </param>
	/// <param name="key"> The key of the value. </param>
	/// <param name="value"> The value of the entry. </param>
	public static void AddOrUpdate(this HttpHeaders headers, string key, string value)
	{
		if (headers.Contains(key))
		{
			headers.Remove(key);
		}

		headers.Add(key, value);
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
	/// Add multiple items to a collection
	/// </summary>
	/// <param name="set"> The set to add items to. </param>
	/// <param name="items"> The items to add. </param>
	/// <typeparam name="T"> The type of the items in the collection. </typeparam>
	public static void AddRange<T>(this ICollection<T> set, params T[] items)
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
		return new HashSet<T>(set.Union(values));
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
		return new HashSet<T>(set.Union(values));
	}

	/// <summary>
	/// Appends all properties that are on the first type (T1) but not on the second type (T2).
	/// </summary>
	/// <typeparam name="T1"> The type of the first item. </typeparam>
	/// <typeparam name="T2"> The type of the second item. </typeparam>
	/// <param name="collection"> The collection to be updated. </param>
	public static void AppendExtraProperties<T1, T2>(this HashSet<string> collection)
	{
		var firstProperties = typeof(T1).GetCachedProperties().Select(x => x.Name);
		var secondProperties = typeof(T2).GetCachedProperties().Select(x => x.Name);
		var missing = firstProperties.Except(secondProperties);
		collection.AddRange(missing);
	}

	/// <summary>
	/// Calculate a CRC using CRC-16/KERMIT.
	/// </summary>
	/// <param name="data"> The data to calculate the CRC for. </param>
	/// <returns> The CRC for the data. </returns>
	public static ushort CalculateCrc16(this byte[] data)
	{
		return CalculateCrc16(data, data.Length);
	}

	/// <summary>
	/// Calculate a CRC using CRC-16/KERMIT.
	/// </summary>
	/// <param name="data"> The data to calculate the CRC for. </param>
	/// <param name="length"> The length to calculate. </param>
	/// <returns> The CRC for the data. </returns>
	public static ushort CalculateCrc16(this byte[] data, int length)
	{
		// CRC-16/KERMIT
		ushort crc = 0;
		var index = 0;

		while (length-- > 0)
		{
			crc = (ushort) ((crc >> 8) ^ _crcTable[(crc ^ data[index++]) & 0xFF]);
		}

		return crc;
	}

	/// <summary>
	/// Exclude duplicates that are sequential. Ex. 1,2,2,3,3,4 -> 1,2,3,4
	/// </summary>
	/// <typeparam name="T"> The type of the collection entries. </typeparam>
	/// <typeparam name="T2"> The type of the property to be validated. </typeparam>
	/// <param name="collection"> The collection to be processed. </param>
	/// <param name="propertyExpression"> The expression of the property to be tested. </param>
	/// <param name="additionalCheck"> An optional additional check for testing for duplicates. </param>
	/// <returns> The processed collections with sequential duplicates removed. </returns>
	public static IEnumerable<T> ExcludeSequentialDuplicates<T, T2>(this IEnumerable<T> collection,
		Expression<Func<T, T2>> propertyExpression, Func<T, T, bool> additionalCheck = null)
	{
		var list = collection.ToList();
		if (list.Count == 0)
		{
			return Array.Empty<T>();
		}

		var current = list[0];
		var response = new List<T> { current };
		var test = propertyExpression.Compile();

		for (var index = 1; index < list.Count; index++)
		{
			var next = list[index];
			var currentValue = test.Invoke(current);
			var nextValue = test.Invoke(next);

			if (Equals(currentValue, nextValue)
				&& ((additionalCheck == null)
					|| additionalCheck.Invoke(current, next)))
			{
				continue;
			}

			current = next;
			response.Add(current);
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
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="TLeft"> The type of the left collection. </typeparam>
	/// <typeparam name="TLeftKey"> The type of the left collection key. </typeparam>
	/// <typeparam name="TRight"> The type of the right collection. </typeparam>
	/// <param name="collection"> The left collection. </param>
	/// <param name="filter"> The filter for the collection. </param>
	/// <param name="updates"> The right collection. </param>
	/// <param name="compare"> The logic for comparison. </param>
	/// <param name="locate"> The logic to locate matching entity. </param>
	/// <param name="convert"> The function to convert from TLeft to TRight. </param>
	/// <param name="optionalUpdates"> A set of optional updates. </param>
	/// <param name="optionalExclusions"> A set of optional excluded properties </param>
	public static void Reconcile<TLeft, TLeftKey, TRight>(this IRepository<TLeft, TLeftKey> collection,
		Func<TLeft, bool> filter,
		IEnumerable<TRight> updates,
		Func<TLeft, TRight, bool> compare,
		Func<TLeft, TRight, bool> locate,
		Func<TRight, TLeft> convert,
		Action<TLeft, TRight> optionalUpdates = null,
		string[] optionalExclusions = null
	)
		where TLeft : Entity<TLeftKey>
		where TRight : IUpdatable
	{
		var filteredCollection = collection.Where(filter).ToList();
		var updateList = updates.ToList();

		// Reconcile two collections
		var updatesToBeAdded = updateList
			.Where(update => filteredCollection.All(item => !locate(item, update)))
			.ToList();
		var updateToBeApplied = updateList
			.Select(update => new { item = filteredCollection.FirstOrDefault(item => locate(item, update)), update })
			.Where(x => x.item != null)
			.ToList();
		var itemsToRemove = filteredCollection
			.Where(item => updateList.All(update => !locate(item, update)))
			.ToList();

		foreach (var addedUpdates in updatesToBeAdded)
		{
			var newItem = convert(addedUpdates);
			if (newItem == null)
			{
				continue;
			}

			newItem.UpdateWith(addedUpdates, optionalExclusions);
			optionalUpdates?.Invoke(newItem, addedUpdates);
			collection.Add(newItem);
		}

		foreach (var updateToApply in updateToBeApplied)
		{
			if (compare(updateToApply.item, updateToApply.update))
			{
				continue;
			}

			updateToApply.item.UpdateWith(updateToApply.update, optionalExclusions);
			optionalUpdates?.Invoke(updateToApply.item, updateToApply.update);
		}

		foreach (var deviceToRemove in itemsToRemove)
		{
			collection.Remove(deviceToRemove);
		}
	}

	/// <summary>
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="TLeft"> The type of the left collection. </typeparam>
	/// <typeparam name="TRight"> The type of the right collection. </typeparam>
	/// <param name="collection"> The left collection. </param>
	/// <param name="updates"> The right collection. </param>
	/// <param name="compare"> The logic for comparison. </param>
	/// <param name="convert"> The function to convert from TLeft to TRight. </param>
	/// <param name="optionalUpdates"> A set of optional updates. </param>
	/// <param name="optionalExclusions"> A set of optional excluded properties </param>
	public static void Reconcile<TLeft, TRight>(this ICollection<TLeft> collection,
		IEnumerable<TRight> updates,
		Func<TLeft, TRight, bool> compare,
		Func<TRight, TLeft> convert,
		Action<TLeft, TRight> optionalUpdates = null,
		string[] optionalExclusions = null
	)
		where TLeft : IUpdatable
		where TRight : IUpdatable
	{
		var updateList = updates.ToList();

		// Reconcile two collections
		var updatesToBeAdded = updateList
			.Where(update => collection.All(item => !compare(item, update)))
			.ToList();
		var updateToBeApplied = updateList
			.Select(update => new { item = collection.FirstOrDefault(item => compare(item, update)), update })
			.Where(x => x.item != null)
			.ToList();
		var itemsToRemove = collection
			.Where(item => updateList.All(update => !compare(item, update)))
			.ToList();

		foreach (var addedUpdates in updatesToBeAdded)
		{
			var newItem = convert(addedUpdates);
			if (newItem == null)
			{
				continue;
			}

			newItem.UpdateWith(addedUpdates, optionalExclusions);
			optionalUpdates?.Invoke(newItem, addedUpdates);
			collection.Add(newItem);
		}

		foreach (var updateToApply in updateToBeApplied)
		{
			updateToApply.item.UpdateWith(updateToApply.update, optionalExclusions);
			optionalUpdates?.Invoke(updateToApply.item, updateToApply.update);
		}

		foreach (var deviceToRemove in itemsToRemove)
		{
			collection.Remove(deviceToRemove);
		}
	}

	/// <summary>
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="T"> The type of the collections. </typeparam>
	/// <param name="collection"> The left collection. </param>
	/// <param name="updates"> The right collection. </param>
	public static void Reconcile<T>(this IList<T> collection, IEnumerable<T> updates)
	{
		collection.Clear();
		collection.AddRange(updates);
	}

	/// <summary>
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="T"> The type of the collections. </typeparam>
	/// <typeparam name="T2"> </typeparam>
	/// <param name="collection"> The left collection. </param>
	/// <param name="updates"> The right collection. </param>
	public static void Reconcile<T, T2>(this IRepository<T, T2> collection, IRepository<T, T2> updates) where T : Entity<T2>
	{
		collection.BulkRemove(x => true);
		updates.ForEach(collection.Add);
	}

	/// <summary>
	/// Reconcile one collection with another.
	/// </summary>
	/// <typeparam name="T"> The type of the collections. </typeparam>
	/// <param name="collection"> The left collection. </param>
	/// <param name="updates"> The right collection. </param>
	public static void Reconcile<T>(this HashSet<T> collection, IEnumerable<T> updates)
	{
		collection.Clear();
		collection.AddRange(updates);
	}

	/// <summary>
	/// Gets a sub array from an existing array.
	/// </summary>
	/// <typeparam name="T"> The type of the array items. </typeparam>
	/// <param name="data"> The array to pull from. </param>
	/// <param name="index"> The index to start from. </param>
	/// <param name="length"> The amount of data to pull. </param>
	/// <returns> The sub array of data. </returns>
	public static T[] SubArray<T>(this T[] data, int index, int length)
	{
		var result = new T[length];
		Array.Copy(data, index, result, 0, length);
		return result;
	}

	/// <summary>
	/// Gets the base 64 version of the byte array.
	/// </summary>
	/// <param name="data"> The data to process. </param>
	/// <returns> The base 64 string of the data. </returns>
	public static string ToBase64String(this byte[] data)
	{
		return Convert.ToBase64String(data);
	}

	/// <summary>
	/// Appends new values to an existing HashSet.
	/// </summary>
	/// <typeparam name="T"> The type of value in the set. </typeparam>
	/// <param name="set"> The set to append to. </param>
	/// <param name="values"> The values to add. </param>
	/// <returns> A new HashSet containing the new values. </returns>
	public static HashSet<T> ToHashSet<T>(this IEnumerable<T> set, params T[] values)
	{
		return new HashSet<T>(set.Union(values));
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

	/// <summary>
	/// Try to get the first item out of the provided values.
	/// </summary>
	/// <typeparam name="T"> The type of the items in values. </typeparam>
	/// <param name="values"> The values to enumerate. </param>
	/// <param name="predicate"> The predicate to validate the item. </param>
	/// <param name="value"> The value if found otherwise the "default" value of the type. </param>
	/// <returns> True if the item was found otherwise false. </returns>
	public static bool TryFirst<T>(this IEnumerable<T> values, Func<T, bool> predicate, out T value)
	{
		foreach (var item in values)
		{
			if (!predicate.Invoke(item))
			{
				continue;
			}

			value = item;
			return true;
		}

		value = default;
		return false;
	}

	#endregion
}