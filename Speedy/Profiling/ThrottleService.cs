﻿#region References

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using Speedy.Extensions;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public class ThrottleService : ThrottleService<object>
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to throttle. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public ThrottleService(TimeSpan delay, Action<CancellationToken> action, bool useTimeService = false)
		: base(delay, (x, _) => action(x), useTimeService)
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
public class ThrottleService<T> : IDisposable
{
	#region Fields

	private readonly Action<CancellationToken, T> _action;
	private readonly TimeSpan _delay;
	private bool _force;
	private DateTime _lastRequestProcessedOn;
	private readonly ConcurrentQueue<T> _queue;
	private DateTime _requestedFor;
	private readonly bool _useTimeService;
	private BackgroundWorker _worker;
	private readonly object _workerLock;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to throttle. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public ThrottleService(TimeSpan delay, Action<CancellationToken, T> action, bool useTimeService = false)
	{
		_delay = delay;
		_action = action;
		_useTimeService = useTimeService;
		_lastRequestProcessedOn = DateTime.MinValue;
		_queue = new ConcurrentQueue<T>();
		_workerLock = true;
		_worker = new BackgroundWorker();
		_worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
		_worker.DoWork += WorkerOnDoWork;
		_worker.WorkerSupportsCancellation = true;

		// Defaults for the trigger service.
		QueueTriggers = false;
	}

	#endregion

	#region Properties

	/// <summary>
	/// True if the throttle service has been triggered.
	/// </summary>
	public bool IsTriggered => _requestedFor > _lastRequestProcessedOn;

	/// <summary>
	/// The throttle is triggered and the delay has expired so it is ready to process.
	/// </summary>
	public bool IsTriggeredAndReadyToProcess =>
		(_requestedFor > _lastRequestProcessedOn)
		&& (_requestedFor <= CurrentTime);

	/// <summary>
	/// If true trigger will queue and be processed. Be careful because queueing on a delay could
	/// end with a never processed queue. Defaults to false, meaning only last trigger processes.
	/// </summary>
	public bool QueueTriggers { get; set; }

	/// <summary>
	/// The timespan until next trigger
	/// </summary>
	public TimeSpan TimeToNextTrigger =>
		IsTriggered
			? _requestedFor - CurrentTime
			: TimeSpan.Zero;

	/// <summary>
	/// The current time for the throttle.
	/// </summary>
	protected DateTime CurrentTime =>
		_useTimeService
			? TimeService.UtcNow
			: DateTime.UtcNow;

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Reset the throttle service
	/// </summary>
	public void Reset()
	{
		lock (_workerLock)
		{
			_worker?.CancelAsync();

			#if (NETSTANDARD2_0)
			while (!_queue.IsEmpty)
			{
				_queue.TryDequeue(out _);
			}
			#else
			_queue.Clear();
			#endif

			_force = false;
			_requestedFor = DateTime.MinValue;
			_lastRequestProcessedOn = DateTime.MinValue;
		}
	}

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="value"> The value to trigger with. </param>
	/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(T value, bool force = false)
	{
		if (!QueueTriggers && !_queue.IsEmpty)
		{
			_queue.Empty();
		}

		// Optionally turn on force
		_force |= force;
		_queue.Enqueue(value);

		if (!IsTriggered || force)
		{
			// Queue up the next run
			_requestedFor = _force
				? CurrentTime
				: CurrentTime + _delay;
		}

		StartWorker();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		lock (_workerLock)
		{
			var worker = _worker;

			if (!disposing || (worker == null))
			{
				return;
			}

			if (worker.IsBusy)
			{
				worker.CancelAsync();
			}

			worker.RunWorkerCompleted -= WorkerOnRunWorkerCompleted;
			worker.DoWork -= WorkerOnDoWork;

			_worker = null;
		}
	}

	private void StartWorker()
	{
		lock (_workerLock)
		{
			// Start the worker if it's not running
			if (_worker?.IsBusy != true)
			{
				_worker?.RunWorkerAsync();
			}
		}
	}

	private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
	{
		var worker = (BackgroundWorker) sender;

		while (!worker.CancellationPending)
		{
			var now = CurrentTime;

			if (!((_requestedFor > _lastRequestProcessedOn)
					&& (_requestedFor <= now))
				&& !_force)
			{
				// Nothing to do...
				Thread.Sleep(10);
				continue;
			}

			_force = false;

			if (!_queue.TryDequeue(out var data))
			{
				_lastRequestProcessedOn = _requestedFor;
				continue;
			}

			using var cancellationTokenSource = new CancellationTokenSource();

			_action(cancellationTokenSource.Token, data);
			_lastRequestProcessedOn = _requestedFor;

			// See if we need to re-trigger due to queued data
			if (_queue.Count > 0)
			{
				// Queue up the next run
				_requestedFor = now + _delay;
			}
		}
	}

	private void WorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if ((_requestedFor < CurrentTime) && _queue.IsEmpty)
		{
			return;
		}

		StartWorker();
	}

	#endregion
}