#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for bindable.
/// </summary>
public static class BindableExtensions
{
	#region Methods

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="bindable"> The bindable to dispatch for. </param>
	/// <param name="action"> The action to be executed. </param>
	public static void Dispatch(this IBindable bindable, Action action)
	{
		var dispatcher = bindable?.GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
		{
			dispatcher.Run(action);
			return;
		}

		action();
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="bindable"> The bindable to dispatch for. </param>
	/// <param name="action"> The action to be executed. </param>
	public static T Dispatch<T>(this IBindable bindable, Func<T> action)
	{
		var dispatcher = bindable?.GetDispatcher();
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.Run(action)
			: action();
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="bindable"> The bindable to dispatch for. </param>
	/// <param name="action"> The action to be executed. </param>
	public static Task DispatchAsync(this IBindable bindable, Action action)
	{
		var dispatcher = bindable?.GetDispatcher();
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.RunAsync(action)
			: Task.Run(action);
	}

	/// <summary>
	/// Run an action on the dispatching thread if available and required.
	/// </summary>
	/// <param name="bindable"> The bindable to dispatch for. </param>
	/// <param name="action"> The action to be executed. </param>
	public static Task<T2> DispatchAsync<T2>(this IBindable bindable, Func<T2> action)
	{
		var dispatcher = bindable?.GetDispatcher();
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.RunAsync(action)
			: Task.Run(action);
	}

	/// <summary>
	/// Returns true if the current context is on the dispatcher thread.
	/// </summary>
	/// <param name="bindable"> The bindable to dispatch for. </param>
	/// <returns> True if on the dispatcher thread otherwise false. </returns>
	public static bool ShouldDispatch(this IBindable bindable)
	{
		var dispatcher = bindable.GetDispatcher();
		return dispatcher is { IsDispatcherThread: false };
	}

	#endregion
}