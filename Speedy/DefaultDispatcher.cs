#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy;

/// <summary>
/// Represents a default dispatcher
/// </summary>
public class DefaultDispatcher : IDispatcher
{
	#region Properties

	/// <inheritdoc />
	public bool IsDispatcherThread { get; private set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Run(Action action)
	{
		if (IsDispatcherThread == false)
		{
			// Go ahead and mark thread access as true
			IsDispatcherThread = true;
		}

		action();
	}

	/// <inheritdoc />
	public T Run<T>(Func<T> action)
	{
		if (IsDispatcherThread == false)
		{
			// Go ahead and mark thread access as true
			IsDispatcherThread = true;
		}

		return action();
	}

	/// <inheritdoc />
	public Task RunAsync(Action action)
	{
		if (IsDispatcherThread == false)
		{
			// Go ahead and mark thread access as true
			IsDispatcherThread = true;
		}

		action();
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<T> RunAsync<T>(Func<T> action)
	{
		if (IsDispatcherThread == false)
		{
			// Go ahead and mark thread access as true
			IsDispatcherThread = true;
		}

		return Task.FromResult(action());
	}

	#endregion
}