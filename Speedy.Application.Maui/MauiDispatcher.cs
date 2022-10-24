namespace Speedy.Application.Maui;

/// <inheritdoc />
public class MauiDispatcher : Dispatcher
{
	#region Fields

	private readonly Microsoft.Maui.Dispatching.IDispatcher _dispatcher;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public MauiDispatcher(Microsoft.Maui.Dispatching.IDispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsDispatcherThread => !_dispatcher.IsDispatchRequired;

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void ExecuteOnDispatcher(Action action)
	{
		_dispatcher.Dispatch(action);
	}

	protected override T ExecuteOnDispatcher<T>(Func<T> action)
	{
		T result = default;
		_dispatcher.Dispatch(() => result = action());
		return result;
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action)
	{
		return _dispatcher.DispatchAsync(action);
	}

	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action)
	{
		return _dispatcher.DispatchAsync(action);
	}

	#endregion
}