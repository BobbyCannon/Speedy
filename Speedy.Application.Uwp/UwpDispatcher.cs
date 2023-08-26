#region References

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
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
		return AwaitableRunAsync(_dispatcher, action, CoreDispatcherPriority.Normal).AwaitResults();
	}

	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action)
	{
		return AwaitableRunAsync(_dispatcher, action, CoreDispatcherPriority.Normal);
	}

	protected override Task ExecuteOnDispatcherAsync(Action action)
	{
		return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
	}

	private Task<T> AwaitableRunAsync<T>(CoreDispatcher dispatcher, Func<T> function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
	{
		if (function is null)
		{
			throw new ArgumentNullException(nameof(function));
		}

		// Skip the dispatch, if possible
		if (dispatcher.HasThreadAccess)
		{
			try
			{
				return Task.FromResult(function());
			}
			catch (Exception e)
			{
				return Task.FromException<T>(e);
			}
		}

		var taskCompletionSource = new TaskCompletionSource<T>();

		_ = dispatcher.RunAsync(priority, () =>
		{
			try
			{
				taskCompletionSource.SetResult(function());
			}
			catch (Exception e)
			{
				taskCompletionSource.SetException(e);
			}
		});

		return taskCompletionSource.Task;
	}

	#endregion
}