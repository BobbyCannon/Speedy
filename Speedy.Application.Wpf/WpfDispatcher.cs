#region References

using System;
using System.Threading.Tasks;
using System.Windows.Threading;

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
	protected override void ExecuteOnDispatcher(Action action)
	{
		_dispatcher.Invoke(action, DispatcherPriority.Normal);
	}

	/// <inheritdoc />
	protected override Task ExecuteOnDispatcherAsync(Action action)
	{
		return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
	}

	#endregion
}