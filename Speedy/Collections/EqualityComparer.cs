#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Collections;

/// <summary>
/// Exposes a method that compares two objects.
/// </summary>
/// <typeparam name="T"> The type of the object to compare. </typeparam>
public class EqualityComparer<T> : IEqualityComparer<T>
{
	#region Fields

	private readonly Func<T, T, bool> _compare;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the comparer.
	/// </summary>
	public EqualityComparer() : this((x, y) => object.Equals(x, y))
	{
	}

	/// <summary>
	/// Create an instance of the comparer.
	/// </summary>
	/// <param name="compare"> The function to compare two objects. </param>
	public EqualityComparer(Func<T, T, bool> compare)
	{
		_compare = compare;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual bool Equals(T x, T y)
	{
		return _compare(x, y);
	}

	/// <inheritdoc />
	public virtual int GetHashCode(T obj)
	{
		return obj.GetHashCode();
	}

	#endregion
}