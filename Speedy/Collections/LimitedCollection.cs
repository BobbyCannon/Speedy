#region References

using System.Collections.ObjectModel;

#endregion

namespace Speedy.Collections
{
	/// <summary>
	/// Limited collection to a maximum number of items.
	/// </summary>
	/// <typeparam name="T"> The type this collection is for. </typeparam>
	public class LimitedCollection<T> : Collection<T>
	{
		#region Fields

		private readonly object _insertLock;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public LimitedCollection() : this(int.MaxValue)
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="limit"> The maximum number of items for this collection. </param>
		public LimitedCollection(int limit)
		{
			_insertLock = new object();

			Limit = limit;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The maximum limit for this collection.
		/// </summary>
		public int Limit { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		protected override void InsertItem(int index, T item)
		{
			lock (_insertLock)
			{
				if (index > Count)
				{
					index = Count;
				}

				base.InsertItem(index, item);

				while (Count > Limit)
				{
					RemoveAt(0);
				}
			}
		}

		#endregion
	}
}