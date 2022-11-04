// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

internal class Timeout
{
	#region Constants

	public const int Infite = -1;

	#endregion

	#region Fields

	private readonly CancellationTokenSource _cancellationToken;

	#endregion

	#region Constructors

	public Timeout(int timeout, Action timesup)
	{
		_cancellationToken = new CancellationTokenSource();

		switch (timeout)
		{
			case Infite:
			{
				// nothing to do
				return;
			}
			case < 0:
			{
				throw new ArgumentOutOfRangeException(nameof(timeout));
			}
		}

		if (timesup == null)
		{
			throw new ArgumentNullException(nameof(timesup));
		}

		Task.Delay(TimeSpan.FromMilliseconds(timeout), _cancellationToken.Token)
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
		_cancellationToken.Cancel();
	}

	#endregion
}