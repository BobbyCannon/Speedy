#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using PropertyChanged;
using Speedy.Presentation;
using Speedy.Threading;

#endregion

namespace Speedy.Collections;

/// <summary>
/// A thread-safe, dispatch safe, limitable, orderable, filterable, and observable list.
/// Dispatch safe, limit, orderable, and filterable settings are optional.
/// </summary>
/// <typeparam name="T"> The type of items in the list. </typeparam>
/// <remarks>
/// https://github.com/dotnet/wpf/issues/52
/// https://github.com/dotnet/runtime/issues/18087
/// https://github.com/dotnet/runtime/pull/65101#issue-1128955996
/// https://github.com/dotnet/wpf/pull/6097
/// </remarks>
public class SpeedyList<T> : ReaderWriterLockBindable, ISpeedyList, IList<T>
{
	#region Fields

	private readonly FilteredObservableCollection<T> _filtered;
	private readonly List<T> _list;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	public SpeedyList() : this(Array.Empty<T>())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(params T[] items) : this(null, null, null, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(OrderBy<T>[] orderBy) : this(null, null, orderBy)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public SpeedyList(IDispatcher dispatcher) : this(null, dispatcher, Array.Empty<OrderBy<T>>())
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IDispatcher dispatcher, params T[] items) : this(null, dispatcher, Array.Empty<OrderBy<T>>(), items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	public SpeedyList(IDispatcher dispatcher, params OrderBy<T>[] orderBy)
		: this(null, dispatcher, orderBy)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IDispatcher dispatcher, OrderBy<T>[] orderBy, params T[] items)
		: this(null, dispatcher, orderBy, items)
	{
	}

	/// <summary>
	/// Create an instance of the list.
	/// </summary>
	/// <param name="readerWriterLock"> An optional lock. Defaults to <see cref="ReaderWriterLockTiny" /> if not provided. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="orderBy"> The optional set of order by settings. </param>
	/// <param name="items"> An optional set. </param>
	public SpeedyList(IReaderWriterLock readerWriterLock, IDispatcher dispatcher, OrderBy<T>[] orderBy, params T[] items)
		: base(readerWriterLock, dispatcher)
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

	/// <summary>
	/// True if the list is currently filtering items.
	/// </summary>
	public bool IsFiltering { get; private set; }

	/// <inheritdoc />
	public bool IsFixedSize => false;

	/// <summary>
	/// True if the list is currently loading items.
	/// </summary>
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
	[SuppressPropertyChangedWarnings]
	public T this[int index]
	{
		get
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			try
			{
				EnterReadLock();

				if (index >= _list.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				return _list[index];
			}
			finally
			{
				ExitReadLock();
			}
		}
		set
		{
			T oldItem;

			try
			{
				EnterWriteLock();

				oldItem = _list[index];
				_list[index] = value;
			}
			finally
			{
				ExitWriteLock();
			}

			Dispatch(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldItem, index)));
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
	[SuppressPropertyChangedWarnings]
	object IList.this[int index]
	{
		get => this[index];
		set => this[index] = (T) value;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Add an item to the list.
	/// </summary>
	/// <param name="item"> The item to add. </param>
	/// <returns> The index where the item exist after add. </returns>
	public T Add(T item)
	{
		AddWithLock(item);
		return item;
	}

	/// <inheritdoc cref="IList" />
	public virtual void Clear()
	{
		if (_list.Count <= 0)
		{
			return;
		}

		try
		{
			EnterUpgradeableReadLock();

			var removedItems = _list.ToArray();

			try
			{
				EnterWriteLock();

				_list.Clear();

				Profiler?.RemovedCount.Increment(removedItems.Length);
			}
			finally
			{
				ExitWriteLock();
			}

			Dispatch(() =>
			{
				OnListUpdated(null, removedItems);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				OnPropertyChanged(nameof(Count));
			});
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <inheritdoc />
	public bool Contains(T item)
	{
		try
		{
			EnterReadLock();
			return InternalContains(item);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
	{
		try
		{
			EnterReadLock();
			_list.CopyTo(array, arrayIndex);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the first item in the list.
	/// </summary>
	/// <returns> The first item. </returns>
	public T First()
	{
		try
		{
			EnterReadLock();
			return _list.First();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the first item in the list.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The first item. </returns>
	public T First(Func<T, bool> predicate)
	{
		try
		{
			EnterReadLock();
			return _list.First(predicate);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the first item in the list or default value.
	/// </summary>
	/// <returns> The first item or default. </returns>
	public T FirstOrDefault()
	{
		try
		{
			EnterReadLock();
			return _list.FirstOrDefault();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the first item in the list or default value.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The first item or default. </returns>
	public T FirstOrDefault(Func<T, bool> predicate)
	{
		try
		{
			EnterReadLock();
			return _list.FirstOrDefault(predicate);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		try
		{
			EnterReadLock();
			var list = _list.ToList();
			return list.GetEnumerator();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	public int IndexOf(T item)
	{
		try
		{
			EnterReadLock();
			return InternalIndexOf(item);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Initialize the profiler to allow tracking of list events.
	/// </summary>
	public void InitializeProfiler()
	{
		Profiler ??= new SpeedyListProfiler(GetDispatcher());
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

		try
		{
			EnterUpgradeableReadLock();

			if (!InternalInsert(index, item))
			{
				return;
			}

			InternalOrderWithoutLocking();
			InternalEnforceLimit(false);
			InternalFilter();
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <returns> The last item or default. </returns>
	public T Last()
	{
		try
		{
			EnterReadLock();
			return _list.Last();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The last item or default. </returns>
	public T Last(Func<T, bool> predicate)
	{
		try
		{
			EnterReadLock();
			return _list.Last(predicate);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <returns> The last item or default. </returns>
	public T LastOrDefault()
	{
		try
		{
			EnterReadLock();
			return _list.LastOrDefault();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <summary>
	/// Get the last item in the list or default value.
	/// </summary>
	/// <param name="predicate"> The predicate filter. </param>
	/// <returns> The last item or default. </returns>
	public T LastOrDefault(Func<T, bool> predicate)
	{
		try
		{
			EnterReadLock();
			return _list.LastOrDefault(predicate);
		}
		finally
		{
			ExitReadLock();
		}
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
		InternalOrder();
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
		try
		{
			EnterReadLock();
			InternalFilter();
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		T removedItem;
		int index;

		try
		{
			EnterWriteLock();

			index = InternalIndexOf(item);
			if (index < 0)
			{
				return false;
			}

			removedItem = _list[index];

			_list.RemoveAt(index);

			Profiler?.RemovedCount.Increment();
		}
		finally
		{
			ExitWriteLock();
		}

		Dispatch(() =>
		{
			OnListUpdated(null, [removedItem]);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
			OnPropertyChanged(nameof(Count));
		});

		return true;
	}

	/// <summary>
	/// Remove all entries that match predicate
	/// </summary>
	/// <param name="predicate"> The predicate to find entries to remove. </param>
	public void Remove(Predicate<T> predicate)
	{
		try
		{
			EnterUpgradeableReadLock();
			for (var i = _list.Count - 1; i >= 0; i--)
			{
				if (predicate.Invoke(_list[i]))
				{
					InternalRemoveAt(i);
				}
			}
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <inheritdoc cref="IList" />
	public void RemoveAt(int index)
	{
		InternalRemoveAt(index);
	}

	/// <summary>
	/// Remove all items from the collection.
	/// </summary>
	/// <param name="index"> The zero-based starting index of the range of items to remove. </param>
	/// <param name="length"> The number of items to remove. </param>
	public void RemoveRange(int index, int length)
	{
		try
		{
			EnterUpgradeableReadLock();

			var start = (index + length) - 1;
			if (start >= Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			var removed = new T[length];

			try
			{
				EnterWriteLock();

				_list.CopyTo(index, removed, 0, length);
				_list.RemoveRange(index, length);

				Profiler?.RemovedCount.Increment(length);
			}
			finally
			{
				ExitWriteLock();
			}

			Dispatch(() =>
			{
				OnListUpdated(null, removed);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, index));
				OnPropertyChanged(nameof(Count));
			});
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <summary>
	/// Try to get an item then remove it.
	/// </summary>
	/// <param name="index"> The index to get the item from. </param>
	/// <param name="item"> The item retrieved or default. </param>
	/// <returns> True if the item was available and retrieved otherwise false. </returns>
	public bool TryGetAndRemoveAt(int index, out T item)
	{
		try
		{
			EnterUpgradeableReadLock();

			if (!InternalHasIndex(index))
			{
				item = default;
				return false;
			}

			item = _list[index];
			InternalRemoveAt(index);
			return true;
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <summary>
	/// Raises the <see cref="CollectionChanged" /> event.
	/// </summary>
	/// <param name="e"> A <see cref="NotifyCollectionChangedEventArgs" /> describing the event arguments. </param>
	protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		CollectionChanged?.Invoke(this, e);
	}

	/// <summary>
	/// Used to invoke the <see cref="ListUpdated" /> event.
	/// </summary>
	/// <param name="added"> The items added. </param>
	/// <param name="removed"> The items removed. </param>
	protected void OnListUpdated(T[] added, T[] removed)
	{
		OnListUpdated(new SpeedyListUpdatedEventArg(added, removed));
	}

	/// <summary>
	/// Used to invoke the <see cref="ListUpdated" /> event.
	/// </summary>
	/// <param name="e"> The changed event args with the details. </param>
	protected virtual void OnListUpdated(SpeedyListUpdatedEventArg e)
	{
		InternalUpdateFilter(e);
		ListUpdated?.Invoke(this, e);
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

		try
		{
			EnterWriteLock();

			// Be sure that if the last item was select we insert at count instead of the
			// requested new index because it will be (Count + 1) instead of Count (end).
			_list.RemoveAt(oldIndex);
			_list.Insert(newIndex > _list.Count ? _list.Count : newIndex, removedItem);
		}
		finally
		{
			ExitWriteLock();
		}

		Dispatch(() => OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem, newIndex, oldIndex)));
	}

	internal virtual void InternalOrder()
	{
		if (!ShouldOrder())
		{
			return;
		}

		try
		{
			EnterUpgradeableReadLock();
			InternalOrderWithoutLocking();
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <inheritdoc />
	void ICollection<T>.Add(T item)
	{
		AddWithLock(item);
	}

	/// <inheritdoc />
	int IList.Add(object item)
	{
		if (item is not T value)
		{
			throw new ArgumentException("The item is the incorrect value type.", nameof(item));
		}

		return AddWithLock(value);
	}

	private int AddWithLock(T item)
	{
		try
		{
			EnterUpgradeableReadLock();

			var limitFromStart = !ShouldOrder();
			var response = InternalAdd(item);
			InternalEnforceLimit(limitFromStart);
			return response;
		}
		finally
		{
			ExitUpgradeableReadLock();
		}
	}

	/// <inheritdoc />
	bool IList.Contains(object item)
	{
		try
		{
			EnterReadLock();
			return InternalContains((T) item);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		try
		{
			EnterReadLock();
			Array.Copy(_list.ToArray(), 0, array, arrayIndex, _list.Count);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <inheritdoc />
	int IList.IndexOf(object item)
	{
		if (item is not T value)
		{
			return -1;
		}

		try
		{
			EnterReadLock();
			return InternalIndexOf(value);
		}
		finally
		{
			ExitReadLock();
		}
	}

	/// <inheritdoc />
	void IList.Insert(int index, object value)
	{
		Insert(index, (T) value);
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
			InternalOrderWithoutLocking();
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
		var filterCheck = FilterCheck;
		if ((filterCheck == null) || IsFiltering)
		{
			return;
		}

		IsFiltering = true;

		var previousDisableOrdering = _filtered.DisableOrdering;
		_filtered.DisableOrdering = true;

		try
		{
			var toRemove = _filtered.Where(x =>
				!InternalContains(x)
				|| !filterCheck(x)
			).ToList();

			Dispatch(() =>
			{
				foreach (var item in toRemove)
				{
					_filtered.Remove(item);
				}
			});

			var toAdd = _list
				.Where(x =>
					filterCheck(x) && !_filtered.Contains(x)).ToList();

			Dispatch(() =>
			{
				foreach (var item in toAdd)
				{
					_filtered.Add(item);
				}

				_filtered.DisableOrdering = false;
				_filtered.Order();
			});
		}
		finally
		{
			_filtered.DisableOrdering = previousDisableOrdering;
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

		try
		{
			EnterWriteLock();

			_list.Insert(index, item);

			Profiler?.AddedCount.Increment();
		}
		finally
		{
			ExitWriteLock();
		}

		Dispatch(() =>
		{
			OnListUpdated([item], null);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			OnPropertyChanged(nameof(Count));
		});

		return true;
	}

	/// <summary>
	/// Loads the items into the list. All existing items will be cleared.
	/// </summary>
	/// <param name="items"> The items to be loaded. </param>
	private void InternalLoad(params T[] items)
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

		Clear();

		try
		{
			EnterWriteLock();

			_list.AddRange(processedItems);

			Profiler?.AddedCount.Increment(processedItems.Count);
		}
		finally
		{
			ExitWriteLock();
		}

		Dispatch(() =>
		{
			OnListUpdated(processedItems.ToArray(), null);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			OnPropertyChanged(nameof(Count));
		});

		IsLoading = false;
	}

	private void InternalOrderWithoutLocking()
	{
		if (!ShouldOrder())
		{
			return;
		}

		try
		{
			IsOrdering = true;
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

	private void InternalRemoveAt(int index)
	{
		T removedItem;

		try
		{
			EnterWriteLock();

			removedItem = _list[index];
			_list.RemoveAt(index);

			Profiler?.RemovedCount.Increment();
		}
		finally
		{
			ExitWriteLock();
		}

		Dispatch(() =>
		{
			OnListUpdated(null, [removedItem]);
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
			OnPropertyChanged(nameof(Count));
		});
	}

	private void InternalUpdateFilter(SpeedyListUpdatedEventArg e)
	{
		if (FilterCheck == null)
		{
			return;
		}

		// Need to determine if our collection has changed.
		var removed = e.Removed?.Cast<T>().ToList();
		var added = e.Added?.Cast<T>().Where(FilterCheck.Invoke).ToList();
		var hasChanged = removed is { Count: > 0 } || added is { Count: > 0 };

		if (!hasChanged)
		{
			return;
		}

		IsFiltering = true;

		try
		{
			removed?.ForEach(x => _filtered.Remove(x));

			if (added != null)
			{
				var previousDisableOrdering = _filtered.DisableOrdering;
				try
				{
					_filtered.DisableOrdering = true;
					added.ForEach(_filtered.Add);
				}
				finally
				{
					_filtered.DisableOrdering = previousDisableOrdering;
					_filtered.Order();
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

	/// <inheritdoc />
	void IList.Remove(object value)
	{
		Remove((T) value);
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

	/// <summary>
	/// Used for notifying presentation layers the collection changed.
	/// Note: There is a few gotchas with CollectionChanged. Not all change
	/// notifications provide the changes with the notification. Ex. When
	/// the list is cleared the items are not provided but rather it's just
	/// a Reset event. This is due to limitations with the
	/// <see cref="INotifyCollectionChanged" /> interface. See links in the
	/// class description.
	/// </summary>
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <summary>
	/// Used to notify when items are added or removed.
	/// </summary>
	public event EventHandler<SpeedyListUpdatedEventArg> ListUpdated;

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
		/// Initializes an instance of the collection.
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
public interface ISpeedyList : IList, INotifyCollectionChanged
{
	#region Properties

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