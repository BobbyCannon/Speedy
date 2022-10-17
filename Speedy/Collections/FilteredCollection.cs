#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

#endregion

namespace Speedy.Collections;

/// <summary>
/// This collection representing a list of entities for a relationship.
/// </summary>
/// <typeparam name="T"> The type for the relationship. </typeparam>
[Serializable]
internal class FilteredCollection<T> : IReadOnlyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
{
	#region Fields

	private readonly Func<T, bool> _filter;
	private readonly ObservableCollection<T> _repository;
	private readonly Action<T> _updateRelationship;

	#endregion

	#region Constructors

	public FilteredCollection(ObservableCollection<T> repository, Func<T, bool> filter, Action<T> updateRelationship)
	{
		_repository = repository;
		_repository.CollectionChanged += RepositoryOnCollectionChanged;
		_filter = filter;
		_updateRelationship = updateRelationship;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public int Count => _repository.Where(_filter).Count();

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispose()
	{
		_repository.CollectionChanged -= RepositoryOnCollectionChanged;
	}

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

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
	/// </returns>
	private IEnumerable<T> GetEnumerable()
	{
		return _repository.Where(_filter);
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

	private void RepositoryOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		CollectionChanged?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	/// <inheritdoc />
	public event PropertyChangedEventHandler PropertyChanged;

	#endregion
}