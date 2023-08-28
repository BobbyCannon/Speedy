#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for dispatcher.
/// </summary>
public static class DispatcherExtensions
{
	#region Methods

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to use. </param>
	/// <param name="action"> The action to be executed. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	public static void Dispatch(this IDispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (dispatcher is { IsDispatcherThread: false })
		{
			dispatcher.Dispatch(action, priority);
			return;
		}

		action();
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to use. </param>
	/// <param name="action"> The action to be executed. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	public static T Dispatch<T>(this IDispatcher dispatcher, Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.Dispatch(action, priority)
			: action();
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to use. </param>
	/// <param name="action"> The action to be executed. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	public static Task DispatchAsync(this IDispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (dispatcher is { IsDispatcherThread: false })
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		action();
		return Task.CompletedTask;
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to use. </param>
	/// <param name="action"> The action to be executed. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	public static Task<T2> DispatchAsync<T2>(this IDispatcher dispatcher, Func<T2> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (dispatcher is { IsDispatcherThread: false })
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		var result = action();
		return Task.FromResult(result);
	}

	/// <summary>
	/// Returns true if the current context is on the dispatcher thread.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to use. </param>
	/// <returns> True if on the dispatcher thread otherwise false. </returns>
	public static bool ShouldDispatch(this IDispatcher dispatcher)
	{
		return dispatcher is { IsDispatcherThread: false };
	}

	#endregion
}