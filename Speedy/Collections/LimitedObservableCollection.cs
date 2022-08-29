namespace Speedy.Collections
{
	/// <summary>
	/// Limited collection to a maximum number of items.
	/// </summary>
	/// <typeparam name="T"> The type this collection is for. </typeparam>
	public class LimitedObservableCollection<T> : BaseObservableCollection<T>
	{
		#region Fields

		private readonly object _insertLock;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		public LimitedObservableCollection() : this(new DefaultDispatcher())
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		public LimitedObservableCollection(IDispatcher dispatcher) : this(int.MaxValue, dispatcher)
		{
		}

		/// <summary>
		/// Instantiates an instance of the collection.
		/// </summary>
		/// <param name="limit"> The maximum number of items for this collection. </param>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		public LimitedObservableCollection(int limit, IDispatcher dispatcher = null) : base(dispatcher)
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
			if (Dispatcher?.HasThreadAccess == false)
			{
				Dispatcher.Run(() => InsertItem(index, item));
				return;
			}

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