#region References

using System;
using System.Threading;
using System.Threading.Tasks;

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
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to throttle. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public ThrottleService(TimeSpan interval, Action<CancellationToken> action, bool useTimeService = false)
		: base(interval, (x, _) => action(x), useTimeService)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	/// <param name="noDelay"> An optional flag to immediately trigger if true. Defaults to false. </param>
	public void Trigger(bool noDelay = false)
	{
		Trigger(null, noDelay);
	}

	#endregion
}

/// <summary>
/// The service to throttle work that supports cancellation.
/// </summary>
public class ThrottleService<T> : ThrottleServiceBase<T>
{
	#region Fields

	private readonly Action<CancellationToken, T> _action;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="interval"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to throttle. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public ThrottleService(TimeSpan interval, Action<CancellationToken, T> action, bool useTimeService = false) : base(interval, useTimeService)
	{
		_action = action;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override Task ThrottledAction(CancellationToken token, T value)
	{
		_action(token, value);
		return Task.CompletedTask;
	}

	#endregion
}