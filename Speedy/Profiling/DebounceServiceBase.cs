#region References

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to debounce work that supports cancellation.
/// </summary>
public abstract class DebounceServiceBase<T> : DebounceServiceBase
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	protected DebounceServiceBase(TimeSpan delay, bool useTimeService = false) : base(delay, useTimeService)
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
	/// The action to be debounced.
	/// </summary>
	/// <param name="token"> </param>
	/// <param name="result"> </param>
	/// <returns> </returns>
	protected abstract Task DebouncedAction(CancellationToken token, T result);

	/// <inheritdoc />
	protected override Task DebouncedAction(CancellationToken token, object result)
	{
		return DebouncedAction(token, result is T typedResult ? typedResult : default);
	}

	#endregion
}

/// <summary>
/// The service to debounce work that supports cancellation.
/// </summary>
public abstract class DebounceServiceBase : DebounceOrThrottleServiceBase
{
	#region Fields

	private object _data;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	protected DebounceServiceBase(TimeSpan delay, bool useTimeService = false) : base(delay, useTimeService)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void ClearData()
	{
		_data = null;
	}

	/// <summary>
	/// The action to be debounced.
	/// </summary>
	/// <param name="token"> </param>
	/// <param name="result"> </param>
	/// <returns> </returns>
	protected abstract Task DebouncedAction(CancellationToken token, object result);

	/// <inheritdoc />
	protected override Task DoWorkAsync()
	{
		var token = CancellationTokenSource?.Token ?? CancellationToken.None;

		return Task.Run(async () =>
		{
			while (token.IsCancellationRequested == false)
			{
				if (!IsTriggeredAndReadyToProcess)
				{
					// Nothing to do... so wait the task delay
					await Task.Delay(TaskWaitDelay, token);
					continue;
				}

				NoDelayOnNextTrigger = false;

				var lastRequestedFor = RequestedFor;

				await DebouncedAction(token, _data);

				lock (ServiceLock)
				{
					LastRequestProcessedOn = lastRequestedFor;

					if (IsTriggered)
					{
						// Someone triggered while we were running
						continue;
					}

					break;
				}
			}
		}, token);
	}

	/// <inheritdoc />
	protected override bool HasMoreData()
	{
		return false;
	}

	/// <inheritdoc />
	protected override void SetData(object value)
	{
		_data = value;
	}

	#endregion
}