#region References

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public abstract class ThrottleServiceBase<T> : ThrottleServiceBase
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	protected ThrottleServiceBase(TimeSpan interval, bool useTimeService = false) : base(interval, useTimeService)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="value"> The value to trigger with. </param>
	/// <param name="noDelay"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(T value, bool noDelay = false)
	{
		base.Trigger(value, noDelay);
	}

	/// <summary>
	/// The action to be throttled.
	/// </summary>
	/// <param name="token"> The token to track cancellation. </param>
	/// <param name="value"> The data for the throttled action. </param>
	/// <returns> The task for the throttled action. </returns>
	protected abstract Task ThrottledAction(CancellationToken token, T value);

	/// <inheritdoc />
	protected override Task ThrottledAction(CancellationToken token, object value)
	{
		return ThrottledAction(token, value is T typedResult ? typedResult : default);
	}

	#endregion
}

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public abstract class ThrottleServiceBase : DebounceOrThrottleServiceBase
{
	#region Fields

	private readonly ConcurrentQueue<object> _queue;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	protected ThrottleServiceBase(TimeSpan interval, bool useTimeService = false)
		: base(interval, useTimeService)
	{
		_queue = new ConcurrentQueue<object>();

		// Defaults for the trigger service.
		QueueTriggers = false;
	}

	#endregion

	#region Properties

	/// <summary>
	/// True if the throttle service has been triggered. It will remain true until all triggers are process.
	/// If trigger are queued then IsTriggered will remain true until the queue is emptied (fully processed).
	/// </summary>
	public override bool IsTriggered => !_queue.IsEmpty || base.IsTriggered;

	/// <summary>
	/// If true trigger will queue and be processed. Be careful because queueing on a delay could
	/// end with a never processed queue. Defaults to false, meaning only last trigger processes.
	/// </summary>
	public bool QueueTriggers { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public override void OnPropertyChanged(string propertyName)
	{
		switch (propertyName)
		{
			case nameof(QueueTriggers):
			{
				if (!QueueTriggers)
				{
					ClearData();
				}
				break;
			}
		}

		base.OnPropertyChanged(propertyName);
	}

	/// <inheritdoc />
	protected override void ClearData()
	{
		lock (ServiceLock)
		{
			#if (NETSTANDARD2_0)
			while (!_queue.IsEmpty)
			{
				_queue.TryDequeue(out _);
			}
			#else
			_queue.Clear();
			#endif
		}
	}

	/// <inheritdoc />
	protected override Task DoWorkAsync()
	{
		var token = CancellationTokenSource?.Token ?? CancellationToken.None;

		return Task.Run(async () =>
		{
			while (token.IsCancellationRequested == false)
			{
				var lastRequestedFor = RequestedFor;
				var now = CurrentTime;

				if (!CheckIsTriggeredAndReadyToProcess(now))
				{
					// Nothing to do... so wait the task delay
					await Task.Delay(TaskWaitDelay, token);
					continue;
				}

				NoDelayOnNextTrigger = false;

				if (_queue.IsEmpty || !_queue.TryDequeue(out var data))
				{
					LastRequestProcessedOn = lastRequestedFor;
				}
				else
				{
					// If not queueing triggers...
					if (!QueueTriggers)
					{
						// Grab last item in the queued up items
						while (_queue.TryDequeue(out var lastItem))
						{
							data = lastItem;
						}
					}

					await ThrottledAction(token, data);
				}

				lock (ServiceLock)
				{
					LastRequestProcessedOn = lastRequestedFor;

					// See if we need to re-trigger due to queued data
					if (_queue.Count > 0)
					{
						// Queue up the next run
						RequestedFor = CurrentTime + Interval;
					}

					if (IsTriggered)
					{
						// Someone triggered while we were running
						continue;
					}

					break;
				}
			}
		});
	}

	/// <inheritdoc />
	protected override bool HasMoreData()
	{
		return _queue.Count > 0;
	}

	/// <inheritdoc />
	protected override void SetData(object value)
	{
		_queue.Enqueue(value);
	}

	/// <summary>
	/// The action to be throttled.
	/// </summary>
	/// <param name="token"> The token to track cancellation. </param>
	/// <param name="value"> The data for the throttled action. </param>
	/// <returns> The task for the throttled action. </returns>
	protected abstract Task ThrottledAction(CancellationToken token, object value);

	#endregion
}