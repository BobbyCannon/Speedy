﻿#region References

using System;
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
public class FilteredCollection<T> : BaseObservableCollection<T>
{
	#region Fields

	private readonly Func<T, bool> _filter;

	private readonly ObservableCollection<T> _originalCollection;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an instance of a filtered collection.
	/// </summary>
	/// <param name="originalCollection"> The collection to filter. </param>
	/// <param name="filter"> The filter expression. </param>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public FilteredCollection(ObservableCollection<T> originalCollection, Func<T, bool> filter, IDispatcher dispatcher = null) : base(dispatcher)
	{
		_originalCollection = originalCollection;
		_originalCollection.CollectionChanged += new WeakEventHandler<NotifyCollectionChangedEventArgs>(OriginalCollectionOnCollectionChanged).Handler;
		_filter = filter;
	}

	#endregion

	#region Methods

	private void OriginalCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		// Need to determine if our collection has changed.
		var oldItems = e.OldItems?.Cast<T>().Where(x => _filter.Invoke(x)).ToList();
		var newItems = e.NewItems?.Cast<T>().Where(x => _filter.Invoke(x)).ToList();
		var hasChanged = oldItems is { Count: > 0 } || newItems is { Count: > 0 };

		if (!hasChanged)
		{
			return;
		}

		switch (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
			{
				newItems?.ForEach(Add);
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				oldItems?.ForEach(x => Remove(x));
				break;
			}
			case NotifyCollectionChangedAction.Reset:
			{
				break;
			}
			case NotifyCollectionChangedAction.Move:
			case NotifyCollectionChangedAction.Replace:
			default:
			{
				return;
			}
		}
	}

	#endregion
}