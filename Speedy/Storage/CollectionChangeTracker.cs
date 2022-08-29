#region References

using System.Collections;
using System.Collections.Generic;

#endregion

namespace Speedy.Storage
{
	/// <summary>
	/// Simple collection tracker to help with collection changed notification.
	/// </summary>
	public class CollectionChangeTracker
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection change tracker.
		/// </summary>
		public CollectionChangeTracker()
		{
			Added = new List<object>();
			Modified = new List<object>();
			Removed = new List<object>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The items added during this collection update.
		/// </summary>
		public IList Added { get; }

		/// <summary>
		/// The items modified during this collection update.
		/// </summary>
		public IList Modified { get; }

		/// <summary>
		/// The items remove during this collection update.
		/// </summary>
		public IList Removed { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Add a list of items that was added during the collection update.
		/// </summary>
		/// <param name="list"> The list of items that was added. </param>
		public void AddAddedEntities(IList list)
		{
			if (list == null)
			{
				return;
			}

			foreach (var item in list)
			{
				Added.Add(item);
			}
		}

		/// <summary>
		/// Add an item that was added during the collection update.
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <param name="item"> The item that was added. </param>
		public void AddAddedEntity<T>(T item)
		{
			if (item == null)
			{
				return;
			}

			Added.Add(item);
		}

		/// <summary>
		/// Add a list of items that was modified during the collection update.
		/// </summary>
		/// <param name="list"> The list of items that was modified. </param>
		public void AddModifiedEntities(IList list)
		{
			if (list == null)
			{
				return;
			}

			foreach (var item in list)
			{
				Modified.Add(item);
			}
		}

		/// <summary>
		/// Add an item that was modified during the collection update.
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <param name="item"> The item that was modified. </param>
		public void AddModifiedEntity<T>(T item)
		{
			if (item == null)
			{
				return;
			}

			Modified.Add(item);
		}

		/// <summary>
		/// Add a list of items that was removed during the collection update.
		/// </summary>
		/// <param name="list"> The list of items that was removed. </param>
		public void AddRemovedEntities(IList list)
		{
			if (list == null)
			{
				return;
			}

			foreach (var item in list)
			{
				Removed.Add(item);
			}
		}

		/// <summary>
		/// Add an item that was removed during the collection update.
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <param name="item"> The item that was removed. </param>
		public void AddRemovedEntity<T>(T item)
		{
			if (item == null)
			{
				return;
			}

			Removed.Add(item);
		}

		/// <summary>
		/// Reset the tracking session.
		/// </summary>
		public void Reset()
		{
			Added.Clear();
			Modified.Clear();
			Removed.Clear();
		}

		/// <summary>
		/// Update the tracker with a collection of changes.
		/// </summary>
		/// <param name="tracker"> The changes to the database. </param>
		public void Update(CollectionChangeTracker tracker)
		{
			AddAddedEntities(tracker.Added);
			AddModifiedEntities(tracker.Modified);
			AddRemovedEntities(tracker.Removed);
		}

		#endregion
	}
}