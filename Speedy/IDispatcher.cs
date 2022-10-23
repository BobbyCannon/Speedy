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
	public void Run(Action action)
	{
		if (IsDispatcherThread)
		{
			action();
			return;
		}

		ExecuteOnDispatcher(action);
	}

	/// <inheritdoc />
	public Task RunAsync(Action action)
	{
		if (IsDispatcherThread)
		{
			action();
			return Task.CompletedTask;
		}

		return ExecuteOnDispatcherAsync(action);
	}

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	protected abstract void ExecuteOnDispatcher(Action action);

	/// <summary>
	/// Execute the action on the dispatcher.
	/// </summary>
	/// <param name="action"> The action to execute. </param>
	protected abstract Task ExecuteOnDispatcherAsync(Action action);

	#endregion
}

/// <summary>
/// Represents a dispatcher to help with handling dispatcher thread access.
/// </summary>
public interface IDispatcher
{
	#region Properties

	/// <summary>
	/// Returns true if currently executing on the dispatcher thread.
	/// </summary>
	bool IsDispatcherThread { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Run an action on the dispatching thread.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	void Run(Action action);

	/// <summary>
	/// Run an asynchronous action on the dispatching thread.
	/// </summary>
	/// <param name="action"> The action to be executed. </param>
	/// <returns> The task. </returns>
	Task RunAsync(Action action);

	#endregion
}