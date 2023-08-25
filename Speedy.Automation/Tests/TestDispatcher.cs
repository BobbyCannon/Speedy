#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy.Automation.Tests;

/// <summary>
/// Represents a test dispatcher
/// </summary>
public class TestDispatcher : IDispatcher
{
	#region Constructors

	/// <summary>
	/// Instantiate a test dispatcher
	/// </summary>
	public TestDispatcher()
	{
		IsDispatcherThread = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public bool IsDispatcherThread { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Run(Action action)
	{
		action();
	}

	/// <inheritdoc />
	public T Run<T>(Func<T> action)
	{
		return action();
	}

	/// <inheritdoc />
	public Task RunAsync(Action action)
	{
		return Task.Run(action);
	}

	/// <inheritdoc />
	public Task<T> RunAsync<T>(Func<T> action)
	{
		return Task.Run(action);
	}

	#endregion
}