#region References

using System;
using System.Linq;
using System.Linq.Expressions;

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
	}

	/// <summary>
	/// Instantiate an instance of the order by value.
	/// </summary>
	/// <param name="keySelector"> The </param>
	/// <param name="descending"> True to order descending and otherwise sort ascending. Default value is false for ascending order. </param>
	public OrderBy(Expression<Func<T, object>> keySelector, bool descending = false)
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
	public Expression<Func<T, object>> KeySelector { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Processes a query through the "order by" that will return the query ordered base on the value.
	/// </summary>
	/// <param name="query"> The query to order. </param>
	/// <param name="thenBys"> An optional set of subsequent orderings. </param>
	/// <returns> The ordered queryable for the provided query. </returns>
	public IOrderedQueryable<T> Process(IQueryable<T> query, params OrderBy<T>[] thenBys)
	{
		var response = Descending ? query.OrderByDescending(KeySelector) : query.OrderBy(KeySelector);

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