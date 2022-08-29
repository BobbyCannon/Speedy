#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

#endregion

namespace Speedy.Automation.Internal
{
	internal class ManagementObjectDisposer : IDisposable
	{
		#region Fields

		private readonly List<IDisposable> _disposables;

		#endregion

		#region Constructors

		public ManagementObjectDisposer()
		{
			_disposables = new List<IDisposable>();
		}

		#endregion

		#region Methods

		public T Add<T>(T disposable) where T : IDisposable
		{
			_disposables.Add(disposable);
			return disposable;
		}

		public void Dispose()
		{
			Exception firstException = null;

			foreach (var d in Enumerable.Reverse(_disposables))
			{
				try
				{
					DisposeOne(d);
				}
				catch
				{
					// Ignore exception when disposing
				}
			}

			_disposables.Clear();

			if (firstException != null)
			{
				throw firstException;
			}
		}

		/// <summary>
		/// Workaround to dispose ManagementBaseObject properly.
		/// See http://stackoverflow.com/questions/11896282
		/// </summary>
		/// <param name="disposable"> The IDisposable object to dispose. </param>
		public static void DisposeOne(IDisposable disposable)
		{
			var mbo = disposable as ManagementBaseObject;
			if (mbo == null)
			{
				disposable.Dispose();
				return;
			}

			mbo.Dispose();
		}

		/// <summary>
		/// Helper for adding ManagementObjectCollection and enumerating it.
		/// </summary>
		public IEnumerable<ManagementBaseObject> EnumerateCollection(ManagementObjectCollection collection)
		{
			Add(collection);
			var enumerator = Add(collection.GetEnumerator());
			while (enumerator.MoveNext())
			{
				yield return Add(enumerator.Current);
			}
		}

		/// <summary>
		/// Helper for ManagementObjectSearcher with adding all objects to the disposables.
		/// </summary>
		/// <param name="query"> The query string. </param>
		public IEnumerable<ManagementBaseObject> Search(string query)
		{
			var searcher = Add(new ManagementObjectSearcher(query));
			return EnumerateCollection(searcher.Get());
		}

		#endregion
	}
}