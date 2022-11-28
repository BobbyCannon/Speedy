// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

internal class Timeout
{
	#region Constants

	public const int Infinite = -1;

	#endregion

	#region Fields

	private readonly CancellationTokenSource _cancellationToken;

	#endregion

	#region Constructors

	public Timeout(int timeout, Action timesUp)
	{
		_cancellationToken = new CancellationTokenSource();

		switch (timeout)
		{
			case Infinite:
			{
				// nothing to do
				return;
			}
			case < 0:
			{
				throw new ArgumentOutOfRangeException(nameof(timeout));
			}
		}

		if (timesUp == null)
		{
			throw new ArgumentNullException(nameof(timesUp));
		}

		Task.Delay(TimeSpan.FromMilliseconds(timeout), _cancellationToken.Token)
			.ContinueWith(t =>
			{
				if (!t.IsCanceled)
				{
					timesUp();
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