#region References

using System.Collections;
using System.Collections.Generic;
using Speedy.Extensions;

#endregion

namespace Speedy.Collections
{
	public class ThreadSafeList<T> : IList<T>
	{
		#region Fields

		private readonly IList<T> _list;
		private readonly object _lock;

		#endregion

		#region Constructors

		public ThreadSafeList() : this(0)
		{
		}

		public ThreadSafeList(int size)
		{
			_list = new List<T>(size);
			_lock = new object();
		}

		#endregion

		#region Properties

		public int Count
		{
			get
			{
				lock (_lock)
				{
					return _list.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock (_lock)
				{
					return _list.IsReadOnly;
				}
			}
		}

		public T this[int index]
		{
			get
			{
				lock (_lock)
				{
					return _list[index];
				}
			}
			set
			{
				lock (_lock)
				{
					_list[index] = value;
				}
			}
		}

		#endregion

		#region Methods

		public void Add(T item)
		{
			lock (_lock)
			{
				_list.Add(item);
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				_list.Clear();
			}
		}

		public bool Contains(T item)
		{
			lock (_lock)
			{
				return _list.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_lock)
			{
				_list.CopyTo(array, arrayIndex);
			}
		}
		
		public IEnumerator<T> GetEnumerator()
		{
			return Clone().GetEnumerator();
		}

		public int IndexOf(T item)
		{
			lock (_lock)
			{
				return _list.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			lock (_lock)
			{
				_list.Insert(index, item);
			}
		}

		public bool Remove(T item)
		{
			lock (_lock)
			{
				return _list.Remove(item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_lock)
			{
				_list.RemoveAt(index);
			}
		}

		private List<T> Clone()
		{
			var response = new List<T>();

			lock (_lock)
			{
				_list.ForEach(x => response.Add(x));
			}

			return response;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}