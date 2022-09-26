#region References

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
	internal class Timeout
	{
		#region Constants

		public const int Infite = -1;

		#endregion

		#region Fields

		private readonly CancellationTokenSource canceller = new CancellationTokenSource();

		#endregion

		#region Constructors

		public Timeout(int timeout, Action timesup)
		{
			if (timeout == Infite)
			{
				return; // nothing to do
			}
			if (timeout < 0)
			{
				throw new ArgumentOutOfRangeException("timeoutMilliseconds");
			}
			if (timesup == null)
			{
				throw new ArgumentNullException("timesup");
			}

			Task.Delay(TimeSpan.FromMilliseconds(timeout), canceller.Token)
				.ContinueWith(t =>
				{
					if (!t.IsCanceled)
					{
						timesup();
					}
				});
		}

		#endregion

		#region Methods

		public void Cancel()
		{
			canceller.Cancel();
		}

		#endregion
	}
}