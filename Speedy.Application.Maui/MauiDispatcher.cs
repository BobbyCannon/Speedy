#region References

using Speedy.Extensions;

#endregion

namespace Speedy.Application.Maui;

/// <inheritdoc />
public class MauiDispatcher : Dispatcher
{
	#region Fields

	private readonly Microsoft.Maui.Dispatching.IDispatcher _dispatcher;

	#endregion

	#region Constructors

	/// <inheritdoc />
	protected MauiDispatcher(Microsoft.Maui.Dispatching.IDispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	public static MauiDispatcher Instance { get; set; }

	/// <inheritdoc />
	public override bool IsDispatcherThread => !_dispatcher.IsDispatchRequired;

	#endregion

	#region Methods

	public static MauiDispatcher Initialize(Microsoft.Maui.Dispatching.IDispatcher dispatcher)
	{
		Instance = new MauiDispatcher(dispatcher);
		return Instance;
	}

	/// <inheritdoc />
	protected override void ExecuteOnDispatcher(Action action, DispatcherPriority priority)
	{
		_dispatcher.Dispatch(action);
	}

	/// <inheritdoc />
	protected override T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority)
	{
		return _dispatcher.DispatchAsync(action).AwaitResults();
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority)
	{
		return _dispatcher.DispatchAsync(action);
	}

	/// <inheritdoc />
	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority)
	{
		return _dispatcher.DispatchAsync(action);
	}

	#endregion
}