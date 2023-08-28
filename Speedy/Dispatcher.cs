#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy;

/// <inheritdoc />
public abstract class Dispatcher : IDispatcher
{
	#region Properties

	/// <inheritdoc />
	public abstract bool IsDispatcherThread { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispatch(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (IsDispatcherThread)
		{
			action();
			return;
		}

		ExecuteOnDispatcher(action, priority);
	}

	/// <inheritdoc />
	public T Dispatch<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		return IsDispatcherThread
			? action()
			: ExecuteOnDispatcher(action, priority);
	}

	/// <inheritdoc />
	public Task DispatchAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		if (!IsDispatcherThread)
		{
			return ExecuteOnDispatcherAsync(action, priority);
		}

		action();
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<T> DispatchAsync<T>(Func<T> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		return IsDispatcherThread
			? Task.FromResult(action())
			: ExecuteOnDispatcherAsync(action, priority);
	}

	/// <inheritdoc />
	public bool ShouldDispatch()
	{
		return !IsDispatcherThread;
	}

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	protected abstract void ExecuteOnDispatcher(Action action, DispatcherPriority priority);

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	protected abstract T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority);

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	protected abstract Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority);

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	/// <param name="priority"> An optional priority for the action. </param>
	protected abstract Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority);

	#endregion
}

/// <summary>
/// Represents a dispatcher to help with handling dispatcher thread access.
/// </summary>
public interface IDispatcher : IDispatchable
{
	#region Properties

	/// <summary>
	/// Returns true if currently executing on the dispatcher thread.
	/// </summary>
	bool IsDispatcherThread { get; }

	#endregion
}