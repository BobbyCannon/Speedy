#region References

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to debounce or throttle work that supports cancellation.
/// This class is the service base for <see cref="DebounceServiceBase" />
/// and <see cref="ThrottleServiceBase" />.
/// </summary>
public abstract class DebounceOrThrottleServiceBase : INotifyPropertyChanged, IDisposable
{
	#region Fields

	private Task _triggerTask;
	private readonly bool _useTimeService;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	protected DebounceOrThrottleServiceBase(TimeSpan interval, bool useTimeService = false)
	{
		_useTimeService = useTimeService;

		LastRequestProcessedOn = DateTime.MinValue;
		ServiceLock = new object();
		TaskWaitDelay = Interval.TotalMilliseconds < 10
			? TimeSpan.Zero
			: Interval.TotalMilliseconds > 1000
				? TimeSpan.FromMilliseconds(500)
				: TimeSpan.FromMilliseconds(10);

		Interval = interval;
		NoDelayOnNextTrigger = false;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The debounce is ready to process.
	/// </summary>
	public bool IsReadyToProcess => CheckIsReadyToProcess(CurrentTime);

	/// <summary>
	/// True if the throttle service has been triggered. It will remain true until all triggers are process.
	/// If trigger are queued then IsTriggered will remain true until the queue is emptied (fully processed).
	/// </summary>
	public virtual bool IsTriggered => RequestedFor > LastRequestProcessedOn;

	/// <summary>
	/// The throttle is triggered and the delay has expired so it is ready to process.
	/// </summary>
	public bool IsTriggeredAndReadyToProcess => CheckIsTriggeredAndReadyToProcess(CurrentTime);

	/// <summary>
	/// The timespan until next trigger
	/// </summary>
	public TimeSpan TimeToNextTrigger => IsTriggered ? RequestedFor - CurrentTime : TimeSpan.Zero;

	/// <summary>
	/// The cancellation token source.
	/// </summary>
	protected CancellationTokenSource CancellationTokenSource { get; set; }

	/// <summary>
	/// The current time for the service.
	/// </summary>
	protected DateTime CurrentTime => _useTimeService ? TimeService.UtcNow : DateTime.UtcNow;

	/// <summary>
	/// The amount of time before the action will trigger.
	/// </summary>
	protected TimeSpan Interval { get; }

	/// <summary>
	/// If true do not delay on the next trigger.
	/// </summary>
	protected bool NoDelayOnNextTrigger { get; set; }

	/// <summary>
	/// The date and time the trigger was requested for.
	/// </summary>
	protected DateTime RequestedFor { get; set; }

	/// <summary>
	/// The lock for the service.
	/// </summary>
	protected object ServiceLock { get; }

	/// <summary>
	/// The delay while waiting on the service to be ready to process.
	/// </summary>
	protected TimeSpan TaskWaitDelay { get; }

	private protected DateTime LastRequestProcessedOn { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Reset the throttle service
	/// </summary>
	public void Reset()
	{
		lock (ServiceLock)
		{
			CancelTask();
			ClearData();

			NoDelayOnNextTrigger = false;

			RequestedFor = DateTime.MinValue;
			LastRequestProcessedOn = DateTime.MinValue;
		}
	}

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="value"> The value to trigger with. </param>
	/// <param name="noDelay"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(object value, bool noDelay = false)
	{
		lock (ServiceLock)
		{
			// Optionally turn on no delay
			SetData(value);
			NoDelayOnNextTrigger |= noDelay;

			// Queue up the next trigger
			RequestedFor = noDelay
				? CurrentTime
				: CurrentTime + Interval;

			if (_triggerTask != null)
			{
				return;
			}

			StartWork();
		}
	}

	/// <summary>
	/// Check to see if the trigger is ready to process.
	/// </summary>
	/// <param name="currentTime"> The date to validate against. </param>
	/// <returns> True if ready to process otherwise false. </returns>
	protected bool CheckIsReadyToProcess(DateTime currentTime)
	{
		// See if we are ready to process;
		return NoDelayOnNextTrigger || (RequestedFor <= currentTime);
	}

	/// <summary>
	/// Check to see if the service has been triggered is ready to process.
	/// </summary>
	/// <param name="currentTime"> The date to validate against. </param>
	/// <returns> True if triggered and ready to process otherwise false. </returns>
	protected bool CheckIsTriggeredAndReadyToProcess(DateTime currentTime)
	{
		// Ensure there is a request
		return (RequestedFor > LastRequestProcessedOn)
			// Then see if it's time to process it
			&& CheckIsReadyToProcess(currentTime);
	}

	/// <summary>
	/// Clear the data in the service.
	/// </summary>
	protected abstract void ClearData();

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <param name="disposing"> True if disposing and false if otherwise. </param>
	protected virtual void Dispose(bool disposing)
	{
		lock (ServiceLock)
		{
			// need to reset / cancel task
			CancelTask();
		}
	}

	/// <summary>
	/// The work to be performed by the service.
	/// </summary>
	/// <returns> The task for the work. </returns>
	protected abstract Task DoWorkAsync();

	/// <summary>
	/// Check to see if there is more data.
	/// </summary>
	/// <returns> True if there is more data otherwise false. </returns>
	protected abstract bool HasMoreData();

	/// <summary>
	/// Set the data for the service.
	/// </summary>
	/// <param name="value"> The value of the data. </param>
	protected abstract void SetData(object value);

	private void CancelTask()
	{
		var source = CancellationTokenSource;
		if (source is { IsCancellationRequested: false })
		{
			source.Cancel();
		}
	}

	private void StartWork()
	{
		CancellationTokenSource?.Dispose();
		CancellationTokenSource = new CancellationTokenSource();
		_triggerTask = DoWorkAsync();
		_triggerTask.ContinueWith(x =>
		{
			lock (ServiceLock)
			{
				// do not dispose, read the link below
				// https://stackoverflow.com/questions/5985973/do-i-need-to-dispose-of-a-task
				//_triggerTask?.Dispose();
				_triggerTask = null;

				// See if we need to re-trigger due to queued data
				if (HasMoreData())
				{
					// Queue up the next run
					RequestedFor = CurrentTime + Interval;
				}

				if (IsTriggered)
				{
					StartWork();
				}
			}
		});
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event PropertyChangedEventHandler PropertyChanged;

	#endregion
}