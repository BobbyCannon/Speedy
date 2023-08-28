#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy.Application.Wpf;

/// <inheritdoc />
public class WpfDispatcher : Dispatcher
{
	#region Fields

	private readonly System.Windows.Threading.Dispatcher _dispatcher;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public WpfDispatcher(System.Windows.Threading.Dispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsDispatcherThread => _dispatcher.CheckAccess();

	#endregion

	#region Methods

	/// <inheritdoc />
	protected override void ExecuteOnDispatcher(Action action, DispatcherPriority priority)
	{
		_dispatcher.Invoke(action, ToDispatcherPriority(priority));
	}

	/// <inheritdoc />
	protected override T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority)
	{
		return _dispatcher.Invoke(action, ToDispatcherPriority(priority));
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority)
	{
		return _dispatcher.InvokeAsync(action, ToDispatcherPriority(DispatcherPriority.Normal)).Task;
	}

	/// <inheritdoc />
	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority)
	{
		return _dispatcher.InvokeAsync(action, ToDispatcherPriority(DispatcherPriority.Normal)).Task;
	}

	private static System.Windows.Threading.DispatcherPriority ToDispatcherPriority(DispatcherPriority priority)
	{
		return priority switch
		{
			DispatcherPriority.SystemIdle => System.Windows.Threading.DispatcherPriority.SystemIdle,
			DispatcherPriority.ApplicationIdle => System.Windows.Threading.DispatcherPriority.ApplicationIdle,
			DispatcherPriority.ContextIdle => System.Windows.Threading.DispatcherPriority.ContextIdle,
			DispatcherPriority.Background => System.Windows.Threading.DispatcherPriority.Background,
			DispatcherPriority.Normal => System.Windows.Threading.DispatcherPriority.Normal,
			_ => System.Windows.Threading.DispatcherPriority.Normal
		};
	}

	#endregion
}