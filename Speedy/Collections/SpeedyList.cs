#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

#endregion

namespace Speedy.Collections;

/// <summary>
/// A thread-safe, dispatch safe, limitable, orderable, filterable, and observable list.
/// Dispatch safe, limit, orderable, and filterable settings are optional.
/// </summary>
/// <typeparam name="T"> The type of items in the list. </typeparam>
public class SpeedyList<T> : LockableBindable, ISpeedyList<T>
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
	public SpeedyList(IDispatcher dispatcher) : this(dispatcher, null, Array.Empty<T>())
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

		DistinctCheck = null;
		Limit = int.MaxValue;
		OrderBy = orderBy;
		IsOrdering = false;
		SyncRoot = new object();

		Load(items);
	}

	#endregion

	#region Properties

	/// <inheritdoc cref="IList" />
	public int Count => _list.Count;

	/// <summary>
	/// An optional comparer to use if you want a distinct list.
	/// </summary>
	public Func<T, T, bool> DistinctCheck { get; set; }

	/// <summary>
	/// An optional filter to restrict the <see cref="Filtered" /> sub collection.
	/// </summary>
	public Func<T, bool> FilterCheck { get; set; }

	/// <summary>
	/// The filter items if this list is being filtered.
	/// </summary>
	public ReadOnlyObservableCollection<T> Filtered => new ReadOnlyObservableCollection<T>(_filtered);

	/// <inheritdoc />
	public bool IsFiltering { get; private set; }

	/// <inheritdoc />
	public bool IsFixedSize => false;

	/// <inheritdoc />
	public bool IsLoading { get; private set; }

	/// <summary>
	/// True if the list is in the process of ordering.
	/// </summary>
	public bool IsOrdering { get; protected set; }

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
			UpgradeableReadLock(() =>
			{
				var oldItem = WriteLock(() =>
				{
					var oldItem = _list[index];
					_list[index] = value;
					return oldItem;
				});

				Dispatch(() =>
				{
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index));
					OnPropertyChanged(nameof(Count));
				});
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
	/// Flag to track pausing of ordering.
	/// </summary>
	public bool PauseOrdering { get; private set; }

	/// <summary>
	/// The profiler for the list. <see cref="InitializeProfiler" /> must be called before accessing the profiler.
	/// </summary>
	public SpeedyListProfiler Profiler { get; private set; }

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

		return UpgradeableReadLock(() =>
		{
			var limitFromStart = !ShouldOrder();
			var response = InternalAdd(value);
			InternalEnforceLimit(limitFromStart);
			return response;
		});
	}

	/// <inheritdoc cref="IList" />
	public virtual void Clear()
	{
		if (_list.Count <= 0)
		{
			return;
		}

		InternalRemoveRange(0, Count);
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
	/// Get the first item in the list.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The first item. </returns>
	public T First(Func<T, bool> predicate)
	{
		return ReadLock(() => _list.First(predicate));
	}

	/// <summary>
	/// Get the first item in the list or default value.
	/// </summary>
	/// <returns> The first item or default. </returns>
	public T FirstOrDefault()
	{
		return ReadLock(() => _list.FirstOrDefault());
	}

	/// <summary>
	/// Get the first item in the list or default value.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The first item or default. </returns>
	public T FirstOrDefault(Func<T, bool> predicate)
	{
		return ReadLock(() => _list.FirstOrDefault(predicate));
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

	/// <summary>
	/// Initialize the profiler to allow tracking of list events.
	/// </summary>
	public void InitializeProfiler()
	{
		Profiler ??= new SpeedyListProfiler(GetDispatcher());
	}

	/// <inheritdoc />
	public void Insert(int index, object value)
	{
		Insert(index, (T) value);
	}

	/// <inheritdoc />
	public void Insert(int index, T item)
	{
		if (ShouldOrder())
		{
			// Just add because the list is an ordered list.
			Add(item);
			return;
		}

		UpgradeableReadLock(() =>
		{
			if (!InternalInsert(index, item))
			{
				return;
			}

			InternalOrder();
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
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The last item or default. </returns>
	public T Last(Func<T, bool> predicate)
	{
		return ReadLock(() => _list.Last(predicate));
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
	/// Get the last item in the list or default value.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The last item or default. </returns>
	public T LastOrDefault(Func<T, bool> predicate)
	{
		return ReadLock(() => _list.LastOrDefault(predicate));
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
		InternalLoad(items);
	}

	/// <summary>
	/// Order the collection.
	/// </summary>
	public void Order()
	{
		WriteLock(InternalOrder);
	}

	/// <summary>
	/// Process an action then order the collection.
	/// </summary>
	/// <param name="process"> The process to execute before ordering. </param>
	public void ProcessThenOrder(Action process)
	{
		ProcessThenOrder<object>(() =>
		{
			process();
			return null;
		});
	}

	/// <summary>
	/// Process an action then order, filter, event on the collection changes. ** see remarks **
	/// </summary>
	/// <param name="process"> The process to execute before ordering. </param>
	/// <typeparam name="T2"> The type of the item from the process. </typeparam>
	/// <returns> The items returned from the process. </returns>
	public T2 ProcessThenOrder<T2>(Func<T2> process)
	{
		// Check to see if we are already ordering
		if (IsOrdering || PauseOrdering)
		{
			// Do the processing
			return process();
		}

		try
		{
			// Disable ordering
			PauseOrdering = true;

			// Do the processing
			return process();
		}
		finally
		{
			// Re-enable ordering then order
			PauseOrdering = false;

			Order();
		}
	}

	/// <summary>
	/// Refresh the filter.
	/// </summary>
	public void RefreshFilter()
	{
		ReadLock(InternalFilter);
	}

	/// <inheritdoc />
	public void Remove(object value)
	{
		Remove((T) value);
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		return UpgradeableReadLock(() =>
		{
			var index = InternalIndexOf(item);
			if (index < 0)
			{
				return false;
			}

			var removedItem = WriteLock(() =>
			{
				var removedItem = _list[index];
				_list.RemoveAt(index);
				return removedItem;
			});

			Dispatch(() =>
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
				OnPropertyChanged(nameof(Count));
			});

			return true;
		});
	}

	/// <summary>
	/// Remove all entries that match predicate
	/// </summary>
	/// <param name="predicate"> The predicate to find entries to remove. </param>
	public void Remove(Predicate<T> predicate)
	{
		UpgradeableReadLock(() =>
		{
			for (var i = _list.Count - 1; i >= 0; i--)
			{
				if (predicate.Invoke(_list[i]))
				{
					InternalRemoveAt(i);
				}
			}
		});
	}

	/// <inheritdoc cref="IList" />
	public void RemoveAt(int index)
	{
		UpgradeableReadLock(() => InternalRemoveAt(index));
	}

	/// <summary>
	/// Remove all items from the collection.
	/// </summary>
	/// <param name="index"> The zero-based starting index of the range of items to remove. </param>
	/// <param name="length"> The number of items to remove. </param>
	public void RemoveRange(int index, int length)
	{
		InternalRemoveRange(index, length);
	}

	/// <summary>
	/// Try to get an item then remove it.
	/// </summary>
	/// <param name="index"> The index to get the item from. </param>
	/// <param name="item"> The item retrieved or default. </param>
	/// <returns> True if the item was available and retrieved otherwise false. </returns>
	public bool TryGetAndRemoveAt(int index, out T item)
	{
		var result = UpgradeableReadLock(() =>
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
	/// Loads the items into the list. All existing items will be cleared.
	/// </summary>
	/// <param name="items"> The items to be loaded. </param>
	protected virtual void InternalLoad(params T[] items)
	{
		IsLoading = true;

		if (items is not { Length: > 0 })
		{
			Clear();
			IsLoading = false;
			return;
		}

		// Guarantee uniqueness of items if we have a comparer
		IList<T> processedItems = DistinctCheck != null
			? items.Distinct(new EqualityComparer<T>(DistinctCheck)).ToList()
			: items.ToList();

		// Order the collection if we have any OrderBy configuration
		processedItems = OrderCollection(processedItems);

		// See if we should limit the collection
		if (processedItems.Count > Limit)
		{
			// Limit to the set limit
			processedItems = processedItems.Take(Limit).ToList();
		}

		UpgradeableReadLock(() =>
		{
			Clear();
			WriteLock(() => _list.AddRange(processedItems));
			Dispatch(() =>
			{
				for (var index = 0; index < processedItems.Count; index++)
				{
					var item = processedItems[index];
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
				}

				OnPropertyChanged(nameof(Count));
			});
		});

		IsLoading = false;
	}

	/// <summary>
	/// Raises the <see cref="CollectionChanged" /> event.
	/// </summary>
	/// <param name="e"> A <see cref="NotifyCollectionChangedEventArgs" /> describing the event arguments. </param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		InternalUpdateFilter(e);
		CollectionChanged?.Invoke(this, e);
	}

	/// <inheritdoc />
	protected override void OnPropertyChangedInDispatcher(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(FilterCheck):
			{
				RefreshFilter();
				break;
			}
			case nameof(OrderBy):
			{
				_filtered.OrderBy = OrderBy?.FirstOrDefault();
				_filtered.ThenBy = OrderBy?.Length > 1 ? OrderBy.Skip(1).ToArray() : null;
				break;
			}
		}

		base.OnPropertyChangedInDispatcher(propertyName);
	}

	/// <summary>
	/// Determine if the list should order.
	/// </summary>
	/// <returns> True if the list should order or false otherwise. </returns>
	protected virtual bool ShouldOrder()
	{
		return !IsLoading
			&& !PauseOrdering
			&& !IsOrdering
			&& (_list.Count > 0)
			&& OrderBy is { Length: > 0 };
	}

	internal int InternalIndexOf(T item)
	{
		if (DistinctCheck == null)
		{
			return _list.IndexOf(item);
		}

		for (var i = 0; i < _list.Count; i++)
		{
			if (DistinctCheck.Invoke(item, _list[i]))
			{
				return i;
			}
		}

		return -1;
	}

	internal void InternalMove(int oldIndex, int newIndex)
	{
		var removedItem = _list[oldIndex];

		WriteLock(() =>
		{
			// Be sure that if the last item was select we insert at count instead of the
			// requested new index because it will be (Count + 1) instead of Count (end).
			_list.RemoveAt(oldIndex);
			_list.Insert(newIndex > _list.Count ? _list.Count : newIndex, removedItem);
		});

		Dispatch(() =>
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex));
			OnPropertyChanged(nameof(Count));
		});
	}

	internal virtual void InternalOrder()
	{
		if (!ShouldOrder())
		{
			return;
		}

		IsOrdering = true;

		try
		{
			Profiler?.OrderCount.Increment();

			var firstOrder = OrderBy.First();
			var thenBy = OrderBy.Skip(1).ToArray();
			var ordered = firstOrder.Process(_list.AsQueryable(), thenBy).ToList();

			for (var i = 0; i < ordered.Count; i++)
			{
				var currentItem = ordered[i];
				var index = InternalIndexOf(currentItem);

				if ((index != -1) && (index != i))
				{
					InternalMove(index, i);
				}
			}
		}
		finally
		{
			IsOrdering = false;
		}
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int InternalAdd(T item)
	{
		int index;
		var function = DistinctCheck;

		if (function != null)
		{
			index = InternalIndexOf(item);
			if (index >= 0)
			{
				return index;
			}
		}

		if (_list.Count > 0)
		{
			var orderBy = OrderBy?.ToArray();

			if (orderBy is { Length: > 0 })
			{
				// Check to see if there is a place to insert
				var insertIndex = OrderBy<T>.GetInsertIndex(this, item, orderBy);

				if (insertIndex >= 0)
				{
					// Found a place to insert so do the insert
					InternalInsert(insertIndex, item);
					return insertIndex;
				}
			}
		}

		index = Count;
		InternalInsert(index, item);

		if (ShouldOrder())
		{
			// This is a custom order
			InternalOrder();
		}

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
		if (FilterCheck == null)
		{
			return;
		}

		IsFiltering = true;

		try
		{
			var toRemove = _filtered.Where(x =>
				!InternalContains(x)
				|| !FilterCheck(x)
			).ToList();

			foreach (var item in toRemove)
			{
				_filtered.Remove(item);
			}

			var toAdd = _list.Where(x =>
				FilterCheck(x)
				&& !_filtered.Contains(x)
			).ToList();

			foreach (var item in toAdd)
			{
				_filtered.Add(item);
			}

			_filtered.Order();
		}
		finally
		{
			IsFiltering = false;
		}
	}

	private bool InternalHasIndex(int index)
	{
		return (index >= 0) && (index < _list.Count);
	}

	private bool InternalInsert(int index, T item)
	{
		if (DistinctCheck != null)
		{
			var existingIndex = _list.IndexOf(item);
			if (existingIndex >= 0)
			{
				return false;
			}
		}

		WriteLock(() => _list.Insert(index, item));

		Dispatch(() =>
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			OnPropertyChanged(nameof(Count));
		});

		return true;
	}

	private void InternalRemoveAt(int index)
	{
		var oldItem = WriteLock(() =>
		{
			var oldItem = _list[index];
			_list.RemoveAt(index);
			return oldItem;
		});

		Dispatch(() =>
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem, index));
			OnPropertyChanged(nameof(Count));
		});
	}

	private void InternalRemoveRange(int index, int length)
	{
		List<T> itemsRemoved = null;

		UpgradeableReadLock(() =>
		{
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

			Dispatch(() =>
			{
				foreach (var item in itemsRemoved)
				{
					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
				}

				OnPropertyChanged(nameof(Count));
			});
		});
	}

	private void InternalUpdateFilter(NotifyCollectionChangedEventArgs e)
	{
		if (FilterCheck == null)
		{
			return;
		}

		// Need to determine if our collection has changed.
		var oldItems = e.OldItems?.Cast<T>().ToList();
		var newItems = e.NewItems?.Cast<T>().Where(FilterCheck.Invoke).ToList();
		var hasChanged = oldItems is { Count: > 0 } || newItems is { Count: > 0 };

		if (!hasChanged)
		{
			return;
		}

		IsFiltering = true;

		try
		{
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
		finally
		{
			IsFiltering = false;
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
		var ordered = firstOrder.Process(items, thenBy).ToList();
		return ordered;
	}

	/// <summary>
	/// Determine if the list should order.
	/// </summary>
	/// <returns> True if the list should order or false otherwise. </returns>
	bool ISpeedyList.ShouldOrder()
	{
		return ShouldOrder();
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	#endregion

	#region Classes

	/// <summary>
	/// Represents a ordered observable collection. The collection supports notification on clear and ability to be ordered.
	/// </summary>
	/// <typeparam name="T2"> The type of the item stored in the collection. </typeparam>
	private class FilteredObservableCollection<T2> : ObservableCollection<T2>
	{
		#region Fields

		private readonly object _orderLock;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public FilteredObservableCollection()
		{
			_orderLock = new object();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Allows disable ordering for faster loading.
		/// </summary>
		public bool DisableOrdering { get; set; }

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
		/// Order the collection.
		/// </summary>
		public void Order()
		{
			if ((Count <= 1) || (OrderBy == null))
			{
				return;
			}

			lock (_orderLock)
			{
				// Track if we are currently already disable ordering
				var wasDisableOrdering = DisableOrdering;

				try
				{
					// Disable ordering while we are ordering
					DisableOrdering = true;

					var ordered = ThenBy?.Length > 0
						? OrderBy.Process(this.AsQueryable(), ThenBy).ToList()
						: OrderBy.Process(this.AsQueryable()).ToList();

					for (var i = 0; i < ordered.Count; i++)
					{
						var index = IndexOf(ordered[i]);
						if ((index != -1) && (index != i))
						{
							Move(index, i);
						}
					}
				}
				finally
				{
					// Reset ordering back to what it was
					DisableOrdering = wasDisableOrdering;
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
				// No need to order on these actions
				return;
			}

			// Some mass inserts may disable ordering to speed up the process
			if (!DisableOrdering)
			{
				Order();
			}
		}

		#endregion
	}

	#endregion
}

/// <summary>
/// Represents a speedy list.
/// </summary>
public interface ISpeedyList<T> : ISpeedyList, IList<T>
{
}

/// <summary>
/// Represents a speedy list.
/// </summary>
public interface ISpeedyList : IList, INotifyCollectionChanged, IDisposable
{
	#region Properties

	/// <summary>
	/// True if the list has been disposed.
	/// </summary>
	bool IsDisposed { get; }

	/// <summary>
	/// True if the list is currently filtering items.
	/// </summary>
	bool IsFiltering { get; }

	/// <summary>
	/// True if the list is currently loading items.
	/// </summary>
	bool IsLoading { get; }

	/// <summary>
	/// True if the list is currently ordering items.
	/// </summary>
	bool IsOrdering { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Determine if the list should order.
	/// </summary>
	/// <returns> True if the list should order or false otherwise. </returns>
	internal bool ShouldOrder();

	#endregion
}