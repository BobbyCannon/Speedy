#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Storage
{
	internal class EntityComparer<T,T2> : IEqualityComparer<T> where T : Entity<T2>
	{
		#region Methods

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(T x, T y)
		{
			return Equals(x.Id, y.Id);
		}

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <returns>
		/// A hash code for the specified object.
		/// </returns>
		/// <param name="obj"> The <see cref="T:System.Object" /> for which a hash code is to be returned. </param>
		/// <exception cref="T:System.ArgumentNullException">
		/// The type of <paramref name="obj" /> is a reference type and
		/// <paramref name="obj" /> is null.
		/// </exception>
		public int GetHashCode(T obj)
		{
			return obj.Id.GetHashCode();
		}

		#endregion
	}
}