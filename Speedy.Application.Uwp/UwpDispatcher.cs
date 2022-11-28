#region References

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.Toolkit.Uwp.Helpers;
using Speedy.Application.Uwp.Extensions;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp;

public class UwpDispatcher : Dispatcher
{
	#region Fields

	private readonly CoreDispatcher _dispatcher;

	#endregion

	#region Constructors

	public UwpDispatcher() : this(CoreApplication.Views.First().Dispatcher)
	{
	}

	public UwpDispatcher(CoreDispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	public override bool IsDispatcherThread => _dispatcher.HasThreadAccess;

	#endregion

	#region Methods

	protected override void ExecuteOnDispatcher(Action action)
	{
		_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AwaitResults();
	}

	protected override T ExecuteOnDispatcher<T>(Func<T> action)
	{
		return _dispatcher.AwaitableRunAsync(action).AwaitResults();
	}

	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action)
	{
		return _dispatcher.AwaitableRunAsync(action);
	}

	protected override Task ExecuteOnDispatcherAsync(Action action)
	{
		return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
	}

	#endregion
}