#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

namespace Speedy.Collections;

/// <summary>
/// Represents an observable collection that supports notification on clear.
/// </summary>
/// <typeparam name="T"> The type of the item stored in the collection. </typeparam>
public class BaseObservableCollection<T> : ObservableCollection<T>, IBindable
{
	#region Fields

	private bool _hasChanges;
	private bool _notificationsEnabled;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	public BaseObservableCollection() : this(null, Array.Empty<T>())
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	/// <param name="items"> An optional set of initial items. </param>
	public BaseObservableCollection(params T[] items) : this(null, items)
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set of initial items. </param>
	public BaseObservableCollection(IDispatcher dispatcher, IEnumerable<T> items) : this(dispatcher, items?.ToArray())
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	/// <param name="items"> An optional set of initial items. </param>
	public BaseObservableCollection(IEnumerable<T> items) : this(null, items?.ToArray())
	{
	}

	/// <summary>
	/// Instantiates an instance of the collection.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	/// <param name="items"> An optional set of initial items. </param>
	public BaseObservableCollection(IDispatcher dispatcher, params T[] items) : base(items)
	{
		Dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The distinct check for item values.
	/// </summary>
	[Browsable(false)]
	[JsonIgnore]
	public Func<T, T, bool> DistinctCheck { get; set; }

	/// <summary>
	/// Represents a thread dispatcher to help with cross threaded request.
	/// </summary>
	[Browsable(false)]
	[JsonIgnore]
	protected IDispatcher Dispatcher { get; private set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual void DisablePropertyChangeNotifications()
	{
		_notificationsEnabled = false;
	}

	/// <inheritdoc />
	public void Dispatch(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
		{
			dispatcher.Dispatch(action, priority);
			return;
		}

		action();
	}

	/// <inheritdoc />
	public T2 Dispatch<T2>(Func<T2> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.Dispatch(action, priority)
			: action();
	}

	/// <inheritdoc />
	public Task DispatchAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		action();
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<T2> DispatchAsync<T2>(Func<T2> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		var result = action();
		return Task.FromResult(result);
	}

	/// <inheritdoc />
	public virtual void EnablePropertyChangeNotifications()
	{
		_notificationsEnabled = true;
	}

	/// <inheritdoc />
	public ReadOnlySet<string> GetChangedProperties()
	{
		return ReadOnlySet<string>.Empty;
	}

	/// <inheritdoc />
	public IDispatcher GetDispatcher()
	{
		return Dispatcher;
	}

	/// <inheritdoc />
	public bool HasChanges()
	{
		return HasChanges(Array.Empty<string>());
	}

	/// <inheritdoc />
	public bool HasChanges(params string[] exclusions)
	{
		return _hasChanges;
	}

	/// <inheritdoc />
	public virtual bool IsPropertyChangeNotificationsEnabled()
	{
		return _notificationsEnabled;
	}

	/// <inheritdoc />
	public virtual void OnPropertyChanged(string propertyName = null)
	{
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Reset the collection to the provided values.
	/// </summary>
	/// <param name="values"> The values to be set to. </param>
	public void Reset(params T[] values)
	{
		if (ShouldDispatch())
		{
			Dispatch(() => Reset(values));
			return;
		}

		var itemsToRemove = this.Except(values).ToList();

		foreach (var value in itemsToRemove)
		{
			Remove(value);
		}

		var itemsToAdd = values.Except(this);

		foreach (var value in itemsToAdd)
		{
			Add(value);
		}
	}

	/// <inheritdoc />
	public void ResetHasChanges()
	{
		_hasChanges = false;
	}

	/// <inheritdoc />
	public bool ShouldDispatch()
	{
		var dispatcher = GetDispatcher();
		return dispatcher is { IsDispatcherThread: false };
	}

	/// <inheritdoc />
	public void UpdateDispatcher(IDispatcher dispatcher)
	{
		Dispatcher = dispatcher;
	}

	/// <inheritdoc />
	protected override void ClearItems()
	{
		// Do not throw changed on elements with no changes, this will result in exception with some UI components
		if (Count <= 0)
		{
			// no changes so just return
			return;
		}

		if (ShouldDispatch())
		{
			Dispatch(ClearItems);
			return;
		}

		var removed = new List<T>(this.ToList());

		foreach (var item in removed)
		{
			Remove(item);
		}
	}

	/// <inheritdoc />
	protected override void InsertItem(int index, T item)
	{
		if (ShouldDispatch())
		{
			Dispatch(() => InsertItem(index, item));
			return;
		}

		if (ItemExists(item))
		{
			// Do not allow inserting because this item exists already
			return;
		}

		base.InsertItem(index, item);
	}

	/// <summary>
	/// Checks to see if an item exist in the collection.
	/// </summary>
	/// <param name="item"> The item to check for. </param>
	/// <returns> True if the item exists otherwise false. </returns>
	protected bool ItemExists(T item)
	{
		if (DistinctCheck == null)
		{
			return false;
		}

		var exists = this.FirstOrDefault(x => DistinctCheck(x, item));
		return exists != null;
	}

	/// <inheritdoc />
	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
	{
		if (e.Action != NotifyCollectionChangedAction.Reset)
		{
			base.OnCollectionChanged(e);
		}

		_hasChanges = true;
	}

	/// <inheritdoc />
	protected sealed override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (ShouldDispatch())
		{
			Dispatch(() => OnPropertyChanged(e));
			return;
		}

		PropertyChanged?.Invoke(this, e);

		base.OnPropertyChanged(e);
	}

	#endregion

	#region Events

	/// <summary>
	/// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
	/// </summary>
	public new virtual event PropertyChangedEventHandler PropertyChanged;

	#endregion
}