#region References

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

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
			Removed = new List<object>();
			Updated = new List<object>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The items added during this collection update.
		/// </summary>
		public IList Added { get; }

		/// <summary>
		/// The items remove during this collection update.
		/// </summary>
		public IList Removed { get; }

		/// <summary>
		/// The items updated during this collection update.
		/// </summary>
		public IList Updated { get; }

		#endregion

		#region Methods

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
		/// Add a list of items that was added during the collection update.
		/// </summary>
		/// <param name="list"> The list of items that was added. </param>
		public void AddAddedEntity(IList list)
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
		/// Add an item that was update during the collection update.
		/// </summary>
		/// <typeparam name="T"> The type of the item. </typeparam>
		/// <param name="item"> The item that was update. </param>
		public void AddUpdatedEntity<T>(T item)
		{
			if (item == null)
			{
				return;
			}

			Updated.Add(item);
		}

		/// <summary>
		/// Add a list of items that was removed during the collection update.
		/// </summary>
		/// <param name="list"> The list of items that was removed. </param>
		public void Remove(IList list)
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
		/// Reset the tracking session.
		/// </summary>
		public void Reset()
		{
			Added.Clear();
			Removed.Clear();
			Updated.Clear();
		}

		/// <summary>
		/// Update the tracker with a change event.
		/// </summary>
		/// <param name="args"> The arguments to update with. </param>
		public void Update(NotifyCollectionChangedEventArgs args)
		{
			AddAddedEntity(args.NewItems);
			Remove(args.OldItems);
		}

		#endregion
	}
}