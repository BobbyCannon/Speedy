#region References

using System;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace Speedy.Profiling;

/// <summary>
/// The service to debounce work that supports cancellation.
/// </summary>
public class DebounceService : DebounceService<object>
{
	#region Constructors

	/// <summary>
	/// Create an instance of the service for throttling an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to debounce. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public DebounceService(TimeSpan delay, Action<CancellationToken> action, bool useTimeService = false)
		: base(delay, (x, _) => action(x), useTimeService)
	{
	}

	#endregion

	#region Methods

	/// <summary>
	/// Trigger the service. Will be trigger after the timespan.
	/// </summary>
	public void Trigger()
	{
		base.Trigger(null);
	}

	#endregion
}

/// <summary>
/// The service to debounce work that supports cancellation.
/// </summary>
public class DebounceService<T> : DebounceServiceBase<T>
{
	#region Fields

	private readonly Action<CancellationToken, T> _action;

	#endregion

	#region Constructors

	/// <summary>
	/// Create an instance of the service for debouncing an action.
	/// </summary>
	/// <param name="delay"> The amount of time before the action will trigger. </param>
	/// <param name="action"> The action to debounce. </param>
	/// <param name="useTimeService"> An optional flag to use the TimeService instead of DateTime. Defaults to false to use DateTime. </param>
	public DebounceService(TimeSpan delay, Action<CancellationToken, T> action, bool useTimeService = false) : base(delay, useTimeService)
	{
		_action = action;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override Task DebouncedAction(CancellationToken token, T result)
	{
		_action?.Invoke(token, result);
		return Task.CompletedTask;
	}

	#endregion
}