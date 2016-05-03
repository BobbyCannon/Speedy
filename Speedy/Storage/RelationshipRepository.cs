#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// This collection representing a list of entities for a relationship.
	/// </summary>
	internal interface IRelationshipRepository
	{
		#region Methods

		/// <summary>
		/// Updates the relationship for all entities in this list.
		/// </summary>
		void UpdateRelationships();

		#endregion
	}

	/// <summary>
	/// This collection representing a list of entities for a relationship.
	/// </summary>
	/// <typeparam name="T"> The type for the relationship. </typeparam>
	[Serializable]
	internal class RelationshipRepository<T> : IRelationshipRepository, ICollection<T> where T : Entity, new()
	{
		#region Fields

		private readonly Func<T, bool> _filter;
		private readonly Action<T> _newItem;
		private readonly Repository<T> _repository;
		private readonly Action<T> _updateRelationship;

		#endregion

		#region Constructors

		public RelationshipRepository(IRepository<T> repository, Func<T, bool> filter, Action<T> newItem, Action<T> updateRelationship)
		{
			_repository = (Repository<T>) repository;
			_filter = filter;
			_newItem = newItem;
			_updateRelationship = updateRelationship;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		public int Count
		{
			get
			{
				var newEntities = _repository.GetRawQueryable(_filter).Count(x => x.Id == 0);
				var existingEntities = _repository.GetRawQueryable(_filter)
					.Where(x => x.Id > 0)
					.Select(x => x.Id)
					.Concat(_repository.Where(_filter).Select(x => x.Id))
					.Distinct()
					.Count();

				return newEntities + existingEntities;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly => false;

		#endregion

		#region Methods

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item"> The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />. </param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1" /> is
		/// read-only.
		/// </exception>
		public void Add(T item)
		{
			_newItem(item);

			if (!Contains(item))
			{
				_repository.Add(item);
			}
		}

		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item"> The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />. </param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1" /> is
		/// read-only.
		/// </exception>
		public void AddOrUpdate(T item)
		{
			_newItem(item);
			_repository.AddOrUpdate(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1" /> is
		/// read-only.
		/// </exception>
		public void Clear()
		{
			_repository.GetRawQueryable(_filter).ForEach(_repository.Remove);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise,
		/// false.
		/// </returns>
		/// <param name="item"> The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />. </param>
		public bool Contains(T item)
		{
			return GetEnumerable().FirstOrDefault(x => (x.Id != 0 && x.Id == item.Id) || x == item) != null;
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an
		/// <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied
		/// from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based
		/// indexing.
		/// </param>
		/// <param name="arrayIndex"> The zero-based index in <paramref name="array" /> at which copying begins. </param>
		/// <exception cref="T:System.ArgumentNullException"> <paramref name="array" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"> <paramref name="arrayIndex" /> is less than 0. </exception>
		/// <exception cref="T:System.ArgumentException">
		/// The number of elements in the source
		/// <see cref="T:System.Collections.Generic.ICollection`1" /> is greater than the available space from
		/// <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
		/// </exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			var myArray = GetEnumerable().ToArray();
			Array.Copy(myArray, 0, array, arrayIndex, myArray.Length);
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
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the
		/// <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if
		/// <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <param name="item"> The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />. </param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1" /> is
		/// read-only.
		/// </exception>
		public bool Remove(T item)
		{
			_repository.Remove(item);
			return true;
		}

		/// <summary>
		/// Updates the relationship for all entities in this list.
		/// </summary>
		public void UpdateRelationships()
		{
			_repository.Where(_filter).ForEach(_updateRelationship);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		private IEnumerable<T> GetEnumerable()
		{
			return _repository.GetRawQueryable(_filter)
				.Union(_repository.Where(_filter), new EntityComparer<T>());
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
	}
}