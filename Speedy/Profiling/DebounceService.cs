#region References

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public class DebounceService : DebounceService<object>
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for debouncing an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to debounce. </param>
	public DebounceService(TimeSpan delay, Action<CancellationToken> action)
		: base(delay, (x, _) => action(x))
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(bool force = false)
	{
		Trigger(null, force);
	}

	#endregion
}

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public class DebounceService<T> : IDisposable
{
	#region Fields

	private readonly Action<CancellationToken, T> _action;
	private T _data;
	private readonly TimeSpan _delay;
	private bool _force;
	private DateTime _requestedFor;
	private BackgroundWorker _worker;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for debouncing an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to debounce. </param>
	public DebounceService(TimeSpan delay, Action<CancellationToken, T> action)
	{
		_delay = delay;
		_action = action;
		_worker = new BackgroundWorker();
		_worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
		_worker.DoWork += WorkerOnDoWork;
		_worker.WorkerReportsProgress = true;
		_worker.WorkerSupportsCancellation = true;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="value"> The value to trigger with. </param>
	/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(T value, bool force = false)
	{
		lock (_worker)
		{
			// Optionally turn on force
			_force |= force;
			_data = value;

			// Queue up the next run
			_requestedFor = _force
				? TimeService.UtcNow
				: TimeService.UtcNow + _delay;

			// Start the worker if it's not running
			if (_worker?.IsBusy != true)
			{
				_worker?.RunWorkerAsync();
			}
		}
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		var worker = _worker;

		if (!disposing || (worker == null))
		{
			return;
		}

		lock (_worker)
		{
			if (worker.IsBusy)
			{
				worker.CancelAsync();
			}

			worker.RunWorkerCompleted -= WorkerOnRunWorkerCompleted;
			worker.DoWork -= WorkerOnDoWork;

			_worker = null;
		}
	}

	private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
	{
		var worker = (BackgroundWorker) sender;
		var lastStart = DateTime.MinValue;
		var cancellationTokenSource = new CancellationTokenSource();

		Task currentRun = null;

		while (!worker.CancellationPending)
		{
			if (((_requestedFor <= lastStart) || (_requestedFor > TimeService.UtcNow)) && !_force)
			{
				if (currentRun is { Status: TaskStatus.RanToCompletion or TaskStatus.Canceled or TaskStatus.Faulted })
				{
					currentRun = null;

					// We don't have an outstanding request so bounce
					if (_requestedFor <= lastStart)
					{
						// Exit the worker
						return;
					}
				}

				// Nothing to do...
				Thread.Sleep(10);
				continue;
			}

			// A new request is ready to be processed
			if (currentRun != null)
			{
				if (!cancellationTokenSource.IsCancellationRequested)
				{
					// Cancel the current run
					cancellationTokenSource.Cancel();
				}

				if (currentRun is { Status: TaskStatus.RanToCompletion or TaskStatus.Canceled or TaskStatus.Faulted })
				{
					currentRun = null;

					// We don't have an outstanding request so bounce
					if (_requestedFor <= lastStart)
					{
						// Exit the worker
						return;
					}
				}
			}
			else
			{
				_force = false;
				var data = _data;
				lastStart = _requestedFor;
				cancellationTokenSource = new CancellationTokenSource();
				currentRun = Task.Run(() => _action(cancellationTokenSource.Token, data), cancellationTokenSource.Token);
			}
		}
	}

	private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if ((_worker == null) || (_requestedFor < TimeService.UtcNow))
		{
			return;
		}

		lock (_worker)
		{
			// Start the worker if it's not running
			if (_worker?.IsBusy != true)
			{
				_worker?.RunWorkerAsync();
			}
		}
	}

	#endregion
}