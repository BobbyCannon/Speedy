#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Collections
{
	/// <summary>
	/// Exposes a method that compares two objects.
	/// </summary>
	/// <typeparam name="T"> The type of the object to compare. </typeparam>
	public class GenericComparer<T> 
		: System.Collections.IComparer, 
			System.Collections.Generic.IComparer<T>,
			IEqualityComparer<T>
	{
		#region Fields

		private readonly Func<T, T, int> _compare;
		private readonly Func<T, int> _getHashCode;

		#endregion

		#region Constructors

		/// <summary>
		/// Create an instance of the comparer.
		/// </summary>
		/// <param name="compare"> The function to compare two objects. </param>
		/// <param name="getHashCode"> An optional override for GetHashCode. If not provided then use the T.GetHashCode. </param>
		public GenericComparer(Func<T, T, int> compare, Func<T, int> getHashCode = null)
		{
			_compare = compare;
			_getHashCode = getHashCode;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public int Compare(T x, T y)
		{
			return _compare(x, y);
		}

		/// <inheritdoc />
		public int Compare(object x, object y)
		{
			return _compare((T) x, (T) y);
		}

		/// <inheritdoc />
		public bool Equals(T x, T y)
		{
			return _compare(x, y) == 0;
		}

		/// <inheritdoc />
		public int GetHashCode(T obj)
		{
			return _getHashCode?.Invoke(obj) ?? obj.GetHashCode();
		}

		#endregion
	}
}