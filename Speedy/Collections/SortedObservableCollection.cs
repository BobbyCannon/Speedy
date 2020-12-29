#region References

using System.Collections.Specialized;
using System.Linq;

#endregion

namespace Speedy.Collections
{
	public class SortedObservableCollection<T> : BaseObservableCollection<T>
	{
		#region Constructors

		public SortedObservableCollection(OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : this(null, orderBy, thenBy)
		{
		}

		public SortedObservableCollection(IDispatcher dispatcher, OrderBy<T> orderBy, params OrderBy<T>[] thenBy) : base(dispatcher)
		{
			OrderBy = orderBy;
			ThenBy = thenBy;
		}

		#endregion

		#region Properties

		public OrderBy<T> OrderBy { get; }

		public OrderBy<T>[] ThenBy { get; }

		#endregion

		#region Methods

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