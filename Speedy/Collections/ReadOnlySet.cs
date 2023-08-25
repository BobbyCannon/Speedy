#region References

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Speedy.Collections;

/// <inheritdoc />
public class ReadOnlySet<T> : ISet<T>
{
	#region Fields

	private readonly ISet<T> _set;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates an empty readonly set.
	/// </summary>
	public ReadOnlySet() : this(new HashSet<T>())
	{
	}

	/// <summary>
	/// Instantiates a readonly set with the provided values.
	/// </summary>
	/// <param name="values"> The values to include in a read only set. </param>
	public ReadOnlySet(params T[] values)
	{
		_set = new HashSet<T>(values);
	}

	/// <summary>
	/// Instantiates a readonly version of the provided set.
	/// </summary>
	/// <param name="set"> The set to make readonly. </param>
	public ReadOnlySet(ISet<T> set)
	{
		_set = set;
	}

	static ReadOnlySet()
	{
		Empty = new ReadOnlySet<T>();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public int Count => _set.Count;

	/// <summary>
	/// Represents an empty collection.
	/// </summary>
	public static ReadOnlySet<T> Empty { get; }

	/// <inheritdoc />
	public bool IsReadOnly => true;

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Clear()
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public bool Contains(T item)
	{
		return _set.Contains(item);
	}

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
	{
		_set.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		return _set.GetEnumerator();
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<T> other)
	{
		return _set.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<T> other)
	{
		return _set.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<T> other)
	{
		return _set.IsSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<T> other)
	{
		return _set.IsSupersetOf(other);
	}

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<T> other)
	{
		return _set.Overlaps(other);
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<T> other)
	{
		return _set.SetEquals(other);
	}

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	public void UnionWith(IEnumerable<T> other)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	bool ISet<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#endregion
}