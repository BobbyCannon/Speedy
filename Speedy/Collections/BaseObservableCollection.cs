#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

#endregion

namespace Speedy.Collections
{
	/// <summary>
	/// Represents an observable collection that supports notification on clear.
	/// </summary>
	/// <typeparam name="T"> The type of the item stored in the collection. </typeparam>
	public class BaseObservableCollection<T> : ObservableCollection<T>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public BaseObservableCollection()
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		/// <param name="items"> An optional set of initial items. </param>
		public BaseObservableCollection(IDispatcher dispatcher, params T[] items) : base(items)
		{
			Dispatcher = dispatcher;
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="items"> An optional set of initial items. </param>
		public BaseObservableCollection(params T[] items) : base(items)
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		/// <param name="items"> An optional set of initial items. </param>
		public BaseObservableCollection(IDispatcher dispatcher, IEnumerable<T> items) : base(items)
		{
			Dispatcher = dispatcher;
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="items"> An optional set of initial items. </param>
		public BaseObservableCollection(IEnumerable<T> items) : base(items)
		{
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
		protected IDispatcher Dispatcher { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Indicates the property has changed on the collection object.
		/// </summary>
		/// <param name="propertyName"> The name of the property has changed. </param>
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
			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => Reset(values));
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
		protected override void ClearItems()
		{
			// Do not throw changed on elements with no changes, this will result in exception with some UI components
			if (Count <= 0)
			{
				// no changes so just return
				return;
			}

			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(ClearItems);
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
			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => InsertItem(index, item));
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
		}

		/// <inheritdoc />
		protected sealed override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (Dispatcher != null && !Dispatcher.HasThreadAccess)
			{
				Dispatcher.Run(() => OnPropertyChanged(e));
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
}