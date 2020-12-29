#region References

using System.Collections.Specialized;
using System.Linq;

#endregion

namespace Speedy.Collections
{
	/// <summary>
	/// Represents a sorted observable collection. The collection supports notification on clear and ability to be sorted.
	/// </summary>
	/// <typeparam name="T"> The type of the item stored in the collection. </typeparam>
	public class SortedObservableCollection<T> : BaseObservableCollection<T>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public SortedObservableCollection(OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : this(null, orderBy, thenBy)
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public SortedObservableCollection(IDispatcher dispatcher, OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : base(dispatcher)
		{
			OrderBy = orderBy;
			ThenBy = thenBy;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The expression to order this collection by.
		/// </summary>
		public OrderBy<T> OrderBy { get; }

		/// <summary>
		/// An optional set of expressions to further order this collection by.
		/// </summary>
		public OrderBy<T>[] ThenBy { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sort the collection.
		/// </summary>
		public void Sort()
		{
			var sorted = OrderBy.Process(this.AsQueryable(), ThenBy).ToList();

			for (var i = 0; i < sorted.Count; i++)
			{
				var index = IndexOf(sorted[i]);
				if (index != i)
				{
					var item = this[index];
					RemoveAt(index);
					Insert(i, item);
					//Move(index, i);
				}
			}
		}

		/// <inheritdoc />
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnCollectionChanged(e);

			if (OrderBy == null || e.Action == NotifyCollectionChangedAction.Move || e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
			{
				// No need to sort on these actions
				return;
			}

			Sort();
		}

		#endregion
	}
}