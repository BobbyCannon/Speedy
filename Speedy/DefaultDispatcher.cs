#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy;

/// <summary>
/// Represents a default dispatcher
/// </summary>
public class DefaultDispatcher : Dispatcher
{
	#region Constructors

	/// <inheritdoc />
	public DefaultDispatcher()
	{
		IsDispatcherThread = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsDispatcherThread { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void ExecuteOnDispatcher(Action action, DispatcherPriority priority)
	{
		action();
	}

	/// <inheritdoc />
	protected override T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority)
	{
		return action();
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority)
	{
		return Task.Run(action);
	}

	/// <inheritdoc />
	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority)
	{
		return Task.Run(action);
	}

	#endregion
}