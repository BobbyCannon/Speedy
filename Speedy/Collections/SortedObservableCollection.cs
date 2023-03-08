#region References

using System;
using System.Collections.Specialized;
using System.Linq;

#endregion

namespace Speedy.Collections;

/// <summary>
/// Represents a sorted observable collection. The collection supports notification on clear and ability to be sorted.
/// </summary>
/// <typeparam name="T"> The type of the item stored in the collection. </typeparam>
public class SortedObservableCollection<T> : LimitedObservableCollection<T>
{
	#region Fields

	private readonly object _sortLock;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	public SortedObservableCollection() : this(new OrderBy<T>(x => x))
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	public SortedObservableCollection(OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : this(null, orderBy, thenBy)
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	public SortedObservableCollection(IDispatcher dispatcher, OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : this(Array.Empty<T>(), dispatcher, orderBy, thenBy)
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	public SortedObservableCollection(T[] items, IDispatcher dispatcher, OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : base(dispatcher, items)
	{
		OrderBy = orderBy;
		ThenBy = thenBy;

		_sortLock = new object();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Allows disable sorting for faster loading.
	/// </summary>
	public bool DisableSorting { get; set; }

	/// <summary>
	/// The expression to order this collection by.
	/// </summary>
	public OrderBy<T> OrderBy { get; }

	/// <summary>
	/// An optional set of expressions to further order this collection by.
	/// </summary>
	public OrderBy<T>[] ThenBy { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Sort the collection.
	/// </summary>
	public void Sort()
	{
		lock (_sortLock)
		{
			// Track if we are currently already disable sorting
			var wasDisableSorting = DisableSorting;

			try
			{
				// Disable sorting while we are sorting
				DisableSorting = true;

				var sorted = OrderBy.Process(this.AsQueryable(), ThenBy).ToList();

				for (var i = 0; i < sorted.Count; i++)
				{
					var index = IndexOf(sorted[i]);
					if (index != i)
					{
						Move(index, i);
					}
				}
			}
			finally
			{
				// Reset sorting back to what it was
				DisableSorting = wasDisableSorting;
			}
		}
	}

	/// <inheritdoc />
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		base.OnCollectionChanged(e);

		if ((OrderBy == null)
			|| (e.Action == NotifyCollectionChangedAction.Move)
			|| (e.Action == NotifyCollectionChangedAction.Remove)
			|| (e.Action == NotifyCollectionChangedAction.Reset))
		{
			// No need to sort on these actions
			return;
		}

		// Some mass inserts may disable sorting to speed up the process
		if (!DisableSorting)
		{
			Sort();
		}
	}

	#endregion
}