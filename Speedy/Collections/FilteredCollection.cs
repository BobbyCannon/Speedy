#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Speedy.Commands;

#endregion

namespace Speedy.Collections;

/// <summary>
/// This collection representing a list of entities for a relationship.
/// </summary>
/// <typeparam name="T"> The type for the relationship. </typeparam>
[Serializable]
public class FilteredCollection<T> : Bindable, IReadOnlyCollection<T>, INotifyCollectionChanged
{
	#region Fields

	private readonly ObservableCollection<T> _collection;

	private readonly Func<T, bool> _filter;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of a filtered collection.
	/// </summary>
	/// <param name="collection"> The collection to filter. </param>
	/// <param name="filter"> The filter expression. </param>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public FilteredCollection(ObservableCollection<T> collection, Func<T, bool> filter, IDispatcher dispatcher = null) : base(dispatcher)
	{
		_collection = collection;
		_collection.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(CollectionOnCollectionChanged).Handler;
		_filter = filter;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public int Count => _collection.Where(_filter).Count();

	#endregion

	#region Methods

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
	/// </returns>
	public IEnumerator<T> GetEnumerator()
	{
		return GetEnumerable().GetEnumerator();
	}

	private void CollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		// Need to determine if our collection has changed.
		var hasChanged = e.OldItems?.Cast<T>().Any(x => _filter.Invoke(x))
			?? e.NewItems?.Cast<T>().Any(x => _filter.Invoke(x))
			?? false;

		if (hasChanged)
		{
			CollectionChanged?.Invoke(this, e);
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
	/// </returns>
	private IEnumerable<T> GetEnumerable()
	{
		return _collection.Where(_filter);
	}

	/// <summary>
	/// Returns an enumerator that iterates through a collection.
	/// </summary>
	/// <returns>
	/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
	/// </returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	#endregion
}