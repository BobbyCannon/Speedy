#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Speedy.Extensions;

#endregion

namespace Speedy.Collections;

/// <summary>
/// A thread-safe, dispatch safe, limitable, sortable, filterable, and observable list.
/// Dispatch safe, limit, sortable, and filterable settings are optional.
/// </summary>
/// <typeparam name="T"> The type of items in the list. </typeparam>
public class SpeedyList<T> : LockableBindable, IList<T>, IList, INotifyCollectionChanged
{
	#region Fields

	private readonly FilteredObservableCollection<T> _filtered;
	private readonly List<T> _list;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	public SpeedyList() : this(null, null, Array.Empty<T>())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IEnumerable<T> items) : this(null, null, items.ToArray())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(params T[] items) : this(null, null, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(params OrderBy<T>[] orderBy) : this(null, orderBy, null)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="items"> An optional set. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(IEnumerable<T> items, params OrderBy<T>[] orderBy) : this(null, orderBy, items.ToArray())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="items"> An optional set. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(T[] items, params OrderBy<T>[] orderBy) : this(null, orderBy, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(OrderBy<T>[] orderBy, params T[] items) : this(null, orderBy, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public SpeedyList(IDispatcher dispatcher) : this(dispatcher, (OrderBy<T>[]) null, null)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IDispatcher dispatcher, IEnumerable<T> items) : this(dispatcher, null, items.ToArray())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IDispatcher dispatcher, params T[] items) : this(dispatcher, null, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(IDispatcher dispatcher, params OrderBy<T>[] orderBy) : this(dispatcher, orderBy, Array.Empty<T>())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(IDispatcher dispatcher, T[] items, params OrderBy<T>[] orderBy) : this(dispatcher, orderBy, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IDispatcher dispatcher, OrderBy<T>[] orderBy, params T[] items) : base(dispatcher)
	{
		_filtered = new FilteredObservableCollection<T>();
		_list = new List<T>();

		ComparerFunction = null;
		Limit = int.MaxValue;
		OrderBy = orderBy;
		SortingDisabled = false;
		SyncRoot = new object();

		Load(items);
	}

	#endregion

	#region Properties

	/// <summary>
	/// An optional comparer to use if you want a distinct list.
	/// </summary>
	public Func<T, T, bool> ComparerFunction { get; set; }

	/// <inheritdoc cref="IList" />
	public int Count => _list.Count;

	/// <summary>
	/// The filter items if this list is being filtered.
	/// </summary>
	public ReadOnlyObservableCollection<T> Filtered => new ReadOnlyObservableCollection<T>(_filtered);

	/// <summary>
	/// An optional filter to restrict the <see cref="Filtered" /> sub collection.
	/// </summary>
	public Func<T, bool> IncludeInFilter { get; set; }

	/// <inheritdoc />
	public bool IsFixedSize => false;

	/// <inheritdoc cref="IList" />
	public bool IsReadOnly => false;

	/// <inheritdoc />
	public bool IsSynchronized => true;

	/// <inheritdoc />
	public T this[int index]
	{
		get
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return ReadLock(() =>
			{
				if ((index < 0) || (index > (_list.Count - 1)))
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				return _list[index];
			});
		}
		set
		{
			DispatchWithWriteLock(() =>
			{
				var oldItem = WriteLock(() =>
				{
					var oldItem = _list[index];
					_list[index] = value;
					return oldItem;
				});

				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
			});
		}
	}

	/// <summary>
	/// The maximum limit for this list.
	/// </summary>
	public int Limit { get; set; }

	/// <summary>
	/// The expression to order this collection by.
	/// </summary>
	public OrderBy<T>[] OrderBy { get; set; }

	/// <summary>
	/// True if sorting has been disabled.
	/// </summary>
	public bool SortingDisabled { get; private set; }

	/// <inheritdoc />
	public object SyncRoot { get; }

	/// <inheritdoc />
	object IList.this[int index]
	{
		get => this[index];
		set => this[index] = (T) value;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Add(T item)
	{
		Add(item as object);
	}

	/// <inheritdoc />
	public int Add(object item)
	{
		if (item is not T value)
		{
			throw new ArgumentException("The item is the incorrect value type.", nameof(item));
		}

		return DispatchWithWriteLock(() =>
		{
			var response = InternalAdd(value);
			InternalSort();
			InternalEnforceLimit(true);
			InternalFilter();
			return response;
		});
	}

	/// <summary>
	/// Add a range of items.
	/// </summary>
	/// <param name="items"> The items to be added. </param>
	public void AddRange(params T[] items)
	{
		AddRange((IEnumerable<T>) items);
	}

	/// <summary>
	/// Add a range of items.
	/// </summary>
	/// <param name="items"> The items to be added. </param>
	public void AddRange(IEnumerable<T> items)
	{
		DispatchWithWriteLock(() =>
		{
			foreach (var item in items)
			{
				InternalAdd(item);
			}
			InternalSort();
			InternalEnforceLimit(true);
			InternalFilter();
		});
	}

	/// <inheritdoc cref="IList" />
	public virtual void Clear()
	{
		if (_list.Count <= 0)
		{
			return;
		}

		RemoveAll();
	}

	/// <inheritdoc />
	public bool Contains(object item)
	{
		return ReadLock(() => InternalContains((T) item));
	}

	/// <inheritdoc />
	public bool Contains(T item)
	{
		return ReadLock(() => InternalContains(item));
	}

	/// <inheritdoc />
	public void CopyTo(Array array, int arrayIndex)
	{
		ReadLock(() => Array.Copy(_list.ToArray(), 0, array, arrayIndex, _list.Count));
	}

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
	{
		ReadLock(() => _list.CopyTo(array, arrayIndex));
	}

	/// <summary>
	/// Get the first item in the list.
	/// </summary>
	/// <returns> The first item. </returns>
	public T First()
	{
		return ReadLock(() => _list.First());
	}

	/// <summary>
	/// Get the first item in the list or default value.
	/// </summary>
	/// <returns> The first item or default. </returns>
	public T FirstOrDefault()
	{
		return ReadLock(() => _list.FirstOrDefault());
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		return ReadLock(() =>
		{
			var list = _list.ToList();
			return list.GetEnumerator();
		});
	}

	/// <inheritdoc />
	public int IndexOf(object item)
	{
		return item is T value
			? ReadLock(() => InternalIndexOf(value))
			: -1;
	}

	/// <inheritdoc />
	public int IndexOf(T item)
	{
		return ReadLock(() => InternalIndexOf(item));
	}

	/// <inheritdoc />
	public void Insert(int index, object value)
	{
		Insert(index, (T) value);
	}

	/// <inheritdoc />
	public void Insert(int index, T item)
	{
		DispatchWithWriteLock(() =>
		{
			if (!InternalInsert(index, item))
			{
				return;
			}

			InternalSort();
			InternalEnforceLimit(false);
			InternalFilter();
		});
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <returns> The last item or default. </returns>
	public T Last()
	{
		return ReadLock(() => _list.Last());
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <returns> The last item or default. </returns>
	public T LastOrDefault()
	{
		return ReadLock(() => _list.LastOrDefault());
	}

	/// <summary>
	/// Loads the items into the list. All existing items will be cleared.
	/// </summary>
	/// <param name="items"> The items to be loaded. </param>
	public void Load(IEnumerable<T> items)
	{
		Load(items.ToArray());
	}

	/// <summary>
	/// Loads the items into the list. All existing items will be cleared.
	/// </summary>
	/// <param name="items"> The items to be loaded. </param>
	public void Load(params T[] items)
	{
		// Ensure items is not null.
		items ??= Array.Empty<T>();

		// Guarantee uniqueness of items if we have a comparer
		IList<T> processedItems = ComparerFunction != null
			? items.Distinct(new EqualityComparer<T>(ComparerFunction)).ToList()
			: items.ToList();

		// Order the collection if we have any OrderBy configuration
		processedItems = OrderCollection(processedItems);

		// See if we should limit the collection
		if (processedItems.Count > Limit)
		{
			// Limit to the set limit
			processedItems = processedItems.Take(Limit).ToList();
		}

		this.Dispatch(() =>
		{
			Clear();

			WriteLock(() => { _list.AddRange(processedItems); });

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList) processedItems, 0));

			InternalFilter();
		});
	}

	/// <inheritdoc />
	public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		switch (propertyName)
		{
			case nameof(IncludeInFilter):
			{
				RefreshFilter();
				break;
			}
		}

		base.OnPropertyChanged(propertyName);
	}

	/// <summary>
	/// Process an action then sort the collection.
	/// </summary>
	/// <param name="process"> The process to execute before sorting. </param>
	public void ProcessThenSort(Action process)
	{
		ProcessThenSort<object>(() =>
		{
			process();
			return null;
		});
	}

	/// <summary>
	/// Process an action then sort, filter, event on the collection changes. ** see remarks **
	/// </summary>
	/// <param name="process"> The process to execute before sorting. </param>
	/// <typeparam name="T2"> The type of the item from the process. </typeparam>
	/// <returns> The items returned from the process. </returns>
	public T2 ProcessThenSort<T2>(Func<T2> process)
	{
		try
		{
			// Disable sorting
			SortingDisabled = true;

			// Do the processing
			return process();
		}
		finally
		{
			// Re-enable sorting then sort
			SortingDisabled = false;

			Sort();
		}
	}

	/// <summary>
	/// Refresh the filter.
	/// </summary>
	public void RefreshFilter()
	{
		DispatchWithReadLock(InternalFilter);
	}

	/// <inheritdoc />
	public void Remove(object value)
	{
		Remove((T) value);
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		return DispatchWithWriteLock(() =>
		{
			var index = InternalIndexOf(item);
			if (index < 0)
			{
				return false;
			}

			var oldItem = _list[index];
			_list.RemoveAt(index);

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));

			return true;
		});
	}

	/// <summary>
	/// Remove all entries that match predicate
	/// </summary>
	/// <param name="predicate"> The predicate to find entries to remove. </param>
	public void Remove(Predicate<T> predicate)
	{
		DispatchWithWriteLock(() =>
		{
			var until = -1;

			for (var i = _list.Count - 1; i >= 0; i--)
			{
				if (predicate.Invoke(_list[i]))
				{
					if (until >= 0)
					{
						if (i == 0)
						{
							InternalRemoveRange(0, (until - i) + 1);
						}
						continue;
					}

					if (i == 0)
					{
						// Remove the first entry because that
						InternalRemoveRange(0, 1);
						continue;
					}

					until = i;
					continue;
				}

				if (until >= 0)
				{
					InternalRemoveRange(i + 1, until - i);
					until = -1;
				}
			}
		});
	}

	/// <summary>
	/// Remove all items from the collection.
	/// </summary>
	public void RemoveAll()
	{
		DispatchWithWriteLock(() => InternalRemoveRange(0, Count));
	}

	/// <inheritdoc cref="IList" />
	public void RemoveAt(int index)
	{
		DispatchWithWriteLock(() => InternalRemoveAt(index));
	}

	/// <summary>
	/// Remove all items from the collection.
	/// </summary>
	/// <param name="index"> The zero-based starting index of the range of items to remove. </param>
	/// <param name="length"> The number of items to remove. </param>
	public void RemoveRange(int index, int length)
	{
		DispatchWithWriteLock(() => InternalRemoveRange(index, length));
	}

	/// <summary>
	/// Sort the collection.
	/// </summary>
	public void Sort()
	{
		DispatchWithWriteLock(InternalSort);
	}

	/// <summary>
	/// Try to get an item then remove it.
	/// </summary>
	/// <param name="index"> The index to get the item from. </param>
	/// <param name="item"> The item retrieved or default. </param>
	/// <returns> True if the item was available and retrieved otherwise false. </returns>
	public bool TryGetAndRemoveAt(int index, out T item)
	{
		var result = DispatchWithWriteLock(() =>
		{
			if (!InternalHasIndex(index))
			{
				return (false, default);
			}

			var item = _list[index];
			InternalRemoveAt(index);
			return (true, item);
		});

		item = result.Item2;
		return result.Item1;
	}

	/// <summary>
	/// Raises the <see cref="CollectionChanged" /> event.
	/// </summary>
	/// <param name="e"> A <see cref="NotifyCollectionChangedEventArgs" /> describing the event arguments. </param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		InternalUpdateFilter(e);
		CollectionChanged?.Invoke(this, e);
		OnPropertyChanged(nameof(Count));
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(OrderBy):
			{
				_filtered.OrderBy = OrderBy?.FirstOrDefault();
				_filtered.ThenBy = OrderBy?.Length > 1 ? OrderBy.Skip(1).ToArray() : null;
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int InternalAdd(T item)
	{
		int index;

		if (ComparerFunction != null)
		{
			index = InternalIndexOf(item);
			if (index >= 0)
			{
				return index;
			}
		}

		index = WriteLock(() =>
		{
			_list.Add(item);
			return _list.Count - 1;
		});

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

		return index;
	}

	private bool InternalContains(T item)
	{
		var index = InternalIndexOf(item);
		return index >= 0;
	}

	private void InternalEnforceLimit(bool start)
	{
		while (_list.Count > Limit)
		{
			InternalRemoveAt(start ? 0 : _list.Count - 1);
		}
	}

	private void InternalFilter()
	{
		if (IncludeInFilter == null)
		{
			return;
		}

		var toRemove = _filtered.Where(x => !InternalContains(x) || !IncludeInFilter(x)).ToList();
		foreach (var item in toRemove)
		{
			_filtered.Remove(item);
		}

		var toAdd = _list.Where(item => IncludeInFilter(item) && !_filtered.Contains(item)).ToList();
		foreach (var item in toAdd)
		{
			_filtered.Add(item);
		}

		_filtered.Sort();
	}

	private bool InternalHasIndex(int index)
	{
		return (index >= 0) && (index < _list.Count);
	}

	private int InternalIndexOf(T item)
	{
		if (ComparerFunction == null)
		{
			return _list.IndexOf(item);
		}

		for (var i = 0; i < _list.Count; i++)
		{
			if (ComparerFunction.Invoke(item, _list[i]))
			{
				return i;
			}
		}

		return -1;
	}

	private bool InternalInsert(int index, T item)
	{
		if (ComparerFunction != null)
		{
			var existingIndex = _list.IndexOf(item);
			if (existingIndex >= 0)
			{
				return false;
			}
		}

		WriteLock(() => _list.Insert(index, item));

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));

		return true;
	}

	private void InternalMove(int oldIndex, int newIndex)
	{
		var removedItem = _list[oldIndex];

		WriteLock(() =>
		{
			_list.RemoveAt(oldIndex);
			_list.Insert(newIndex, removedItem);
		});

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex));
	}

	private void InternalRemoveAt(int index)
	{
		var oldItem = WriteLock(() =>
		{
			var oldItem = _list[index];
			_list.RemoveAt(index);
			return oldItem;
		});

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
	}

	private void InternalRemoveRange(int index, int length)
	{
		List<T> itemsRemoved = null;

		WriteLock(() =>
		{
			var remaining = _list.Count - index;
			var itemsToRemoved = Math.Min(length, remaining);
			var until = index + itemsToRemoved;
			itemsRemoved = new List<T>(itemsToRemoved);

			for (var i = index; i < until; i++)
			{
				itemsRemoved.Add(_list[i]);
			}

			_list.RemoveRange(index, length);
		});

		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsRemoved, index));
	}

	private void InternalSort()
	{
		if ((_list.Count <= 1)
			|| (OrderBy == null)
			|| SortingDisabled)
		{
			return;
		}

		var sorted = OrderCollection(_list);

		for (var i = 0; i < sorted.Count; i++)
		{
			var currentItem = sorted[i];
			var index = InternalIndexOf(currentItem);

			if ((index != -1) && (index != i))
			{
				InternalMove(index, i);
			}
		}
	}

	private void InternalUpdateFilter(NotifyCollectionChangedEventArgs e)
	{
		if (IncludeInFilter == null)
		{
			return;
		}

		// Need to determine if our collection has changed.
		var oldItems = e.OldItems?.Cast<T>().ToList();
		var newItems = e.NewItems?.Cast<T>().Where(IncludeInFilter.Invoke).ToList();
		var hasChanged = oldItems is { Count: > 0 } || newItems is { Count: > 0 };

		if (!hasChanged)
		{
			return;
		}

		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
			{
				newItems?.ForEach(_filtered.Add);
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				oldItems?.ForEach(x => _filtered.Remove(x));
				break;
			}
		}
	}

	private IList<T> OrderCollection(IList<T> items)
	{
		if ((items.Count <= 1) || OrderBy is not { Length: > 0 })
		{
			return items;
		}

		var firstOrder = OrderBy.First();
		var thenBy = OrderBy.Skip(1).ToArray();
		var sorted = firstOrder.Process(items, thenBy).ToList();
		return sorted;
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	#endregion

	#region Classes

	/// <summary>
	/// Represents a sorted observable collection. The collection supports notification on clear and ability to be sorted.
	/// </summary>
	/// <typeparam name="T2"> The type of the item stored in the collection. </typeparam>
	private class FilteredObservableCollection<T2> : ObservableCollection<T2>
	{
		#region Fields

		private readonly object _sortLock;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public FilteredObservableCollection()
		{
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
		public OrderBy<T2> OrderBy { get; set; }

		/// <summary>
		/// An optional set of expressions to further order this collection by.
		/// </summary>
		public OrderBy<T2>[] ThenBy { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Sort the collection.
		/// </summary>
		public void Sort()
		{
			if ((Count <= 1) || (OrderBy == null))
			{
				return;
			}

			lock (_sortLock)
			{
				// Track if we are currently already disable sorting
				var wasDisableSorting = DisableSorting;

				try
				{
					// Disable sorting while we are sorting
					DisableSorting = true;

					var sorted = ThenBy?.Length > 0
						? OrderBy.Process(this.AsQueryable(), ThenBy).ToList()
						: OrderBy.Process(this.AsQueryable()).ToList();

					for (var i = 0; i < sorted.Count; i++)
					{
						var index = IndexOf(sorted[i]);
						if ((index != -1) && (index != i))
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

	#endregion
}