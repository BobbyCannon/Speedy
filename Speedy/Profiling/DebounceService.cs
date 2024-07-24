#region References

using System;
using System.Threading;
using Speedy.Runtime;

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
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to debounce. </param>
	/// <param name="timeService"> An optional TimeService instead of DateTime. Defaults to new instance of TimeService (DateTime). </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public DebounceService(TimeSpan interval, Action<CancellationToken> action, IDateTimeProvider timeService = null, IDispatcher dispatcher = null)
		: base(interval, (x, _) => action(x), timeService, dispatcher)
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
public class DebounceService<T> : DebounceOrThrottleService<T>
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="interval"> The amount of time between each trigger. </param>
	/// <param name="action"> The action to throttle. </param>
	/// <param name="timeService"> An optional TimeService instead of DateTime. Defaults to new instance of TimeService (DateTime). </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public DebounceService(TimeSpan interval, Action<CancellationToken, T> action, IDateTimeProvider timeService = null, IDispatcher dispatcher = null)
		: base(interval, action, timeService, dispatcher)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The timespan until next trigger
	/// </summary>
	public override TimeSpan TimeToNextTrigger
	{
		get
		{
			var elapsed = CurrentTime - TriggeredOn;
			if (elapsed <= TimeSpan.Zero)
			{
				return TimeSpan.Zero;
			}

			return Interval - elapsed;
		}
	}

	/// <inheritdoc />
	protected override DateTime NextTriggerDate => CurrentTime;

	#endregion
}