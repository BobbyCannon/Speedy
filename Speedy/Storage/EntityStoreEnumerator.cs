#region References

using System.Collections;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Speedy.Storage
{
	internal class EntityStoreEnumerator<T> : IEnumerator<T> where T : Entity, new()
	{
		#region Fields

		private FileInfo[] _files;
		private int _index;
		private readonly EntityStore<T> _store;

		#endregion

		#region Constructors

		public EntityStoreEnumerator(EntityStore<T> store)
		{
			_store = store;
			_files = null;
			_index = 0;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <returns>
		/// The element in the collection at the current position of the enumerator.
		/// </returns>
		public T Current { get; private set; }

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		/// <returns>
		/// The current element in the collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">
		/// The enumerator is positioned before the first element of the
		/// collection or after the last element.
		/// </exception>
		object IEnumerator.Current => Current;

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the
		/// collection.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException"> The collection was modified after the enumerator was created. </exception>
		public bool MoveNext()
		{
			if (_files == null)
			{
				var info = new DirectoryInfo(_store.Directory);
				if (!info.Exists)
				{
					return false;
				}

				_files = info.GetFiles();
			}

			if (_index >= _files.Length)
			{
				return false;
			}

			var fileInfo = _files[_index++];
			Current = _store.ReadEntity(fileInfo.FullName) as T;
			return Current != null;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException"> The collection was modified after the enumerator was created. </exception>
		public void Reset()
		{
			_files = null;
			_index = 0;
		}

		#endregion
	}
}