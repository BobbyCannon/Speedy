#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Collections;

/// <summary>
/// Represents an order by value.
/// </summary>
/// <typeparam name="T"> The type of the item to order. </typeparam>
public class OrderBy<T>
{
	#region Constructors

	/// <summary>
	/// Instantiate an instance of the order by value.
	/// </summary>
	/// <param name="descending"> True to order descending and otherwise sort ascending. Default value is false for ascending order. </param>
	public OrderBy(bool descending = false) : this(x => x, descending)
	{
		if (!typeof(T).IsDescendantOf(typeof(IComparable)))
		{
			throw new InvalidOperationException("The type must implement IComparable to use this constructor.");
		}
	}

	/// <summary>
	/// Instantiate an instance of the order by value.
	/// </summary>
	/// <param name="keySelector"> The </param>
	/// <param name="descending"> True to order descending and otherwise sort ascending. Default value is false for ascending order. </param>
	public OrderBy(Func<T, object> keySelector, bool descending = false)
	{
		KeySelector = keySelector;
		Descending = descending;
	}

	#endregion

	#region Properties

	/// <summary>
	/// True for descending and false for ascending order.
	/// </summary>
	public bool Descending { get; set; }

	/// <summary>
	/// A function to extract a key from an element.
	/// </summary>
	public Func<T, object> KeySelector { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Locate the insert index for an item.
	/// </summary>
	/// <typeparam name="T2"> The type of the item. </typeparam>
	/// <param name="list"> The current item list. </param>
	/// <param name="item"> The item to determine index for. </param>
	/// <param name="orderBy"> The list of order by requirements. </param>
	/// <returns> The index to insert the item at. </returns>
	public static int GetInsertIndex<T2>(IList<T2> list, T2 item, params OrderBy<T2>[] orderBy)
	{
		var array = new T2[2];
		var firstOrder = orderBy[0];
		var thenBy = orderBy.Length == 1
			? Array.Empty<OrderBy<T2>>()
			: orderBy.Skip(1).ToArray();

		array[0] = item;

		for (var i = 0; i < list.Count; i++)
		{
			array[1] = list[i];

			var sorted = firstOrder.Process(array, thenBy).First();
			if (Equals(sorted, item))
			{
				return i;
			}
		}

		return list.Count;
	}

	/// <summary>
	/// Order the collection by the provided OrderBy collection.
	/// </summary>
	/// <typeparam name="T2"> The type of the items. </typeparam>
	/// <param name="items"> The items to be ordered. </param>
	/// <param name="orderBy"> The order by expressions. </param>
	/// <returns> The ordered list. </returns>
	public static IList<T2> OrderCollection<T2>(IList<T2> items, params OrderBy<T2>[] orderBy)
	{
		if ((items.Count <= 1) || orderBy is not { Length: > 0 })
		{
			return items;
		}

		var firstOrder = orderBy.First();
		var thenBy = orderBy.Skip(1).ToArray();
		var sorted = firstOrder.Process(items, thenBy).ToList();
		return sorted;
	}

	/// <summary>
	/// Processes a query through the "order by" that will return the query ordered base on the value.
	/// </summary>
	/// <param name="query"> The query to order. </param>
	/// <param name="thenBys"> An optional set of subsequent orderings. </param>
	/// <returns> The ordered queryable for the provided query. </returns>
	public IOrderedEnumerable<T> Process(IEnumerable<T> query, params OrderBy<T>[] thenBys)
	{
		var response = Descending
			? query.OrderByDescending(KeySelector)
			: query.OrderBy(KeySelector);

		if (thenBys is not { Length: > 0 })
		{
			return response;
		}

		foreach (var thenBy in thenBys)
		{
			response = thenBy.Descending
				? response.ThenByDescending(thenBy.KeySelector)
				: response.ThenBy(thenBy.KeySelector);
		}

		return response;
	}

	#endregion
}