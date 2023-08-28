#region References

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Uwp;

/// <summary>
/// Having lots of struggle with the dispatcher.
/// </summary>
/// <remarks>
/// https://github.com/microsoft/Windows-task-snippets/blob/master/tasks/UI-thread-task-await-from-background-thread.md
/// https://github.com/CommunityToolkit/WindowsCommunityToolkit/blob/main/Microsoft.Toolkit.Uwp/Helpers/DispatcherHelper.cs
/// </remarks>
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

	protected override void ExecuteOnDispatcher(Action action, DispatcherPriority priority)
	{
		AwaitableRunAsync(action, ToDispatcherPriority(priority)).AwaitResults();
	}

	protected override T ExecuteOnDispatcher<T>(Func<T> action, DispatcherPriority priority)
	{
		return AwaitableRunAsync(_dispatcher, action, ToDispatcherPriority(priority)).AwaitResults();
	}

	protected override Task<T> ExecuteOnDispatcherAsync<T>(Func<T> action, DispatcherPriority priority)
	{
		return AwaitableRunAsync(_dispatcher, action, ToDispatcherPriority(priority));
	}

	protected override Task ExecuteOnDispatcherAsync(Action action, DispatcherPriority priority)
	{
		return _dispatcher.RunAsync(ToDispatcherPriority(priority), () => action()).AsTask();
	}

	private Task AwaitableRunAsync(Action function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
	{
		if (function is null)
		{
			throw new ArgumentNullException(nameof(function));
		}

		/* Run the function directly when we have thread access.
		* Also reuse Task.CompletedTask in case of success,
		* to skip an unnecessary heap allocation for every invocation. */
		if (_dispatcher.HasThreadAccess)
		{
			try
			{
				function();

				return Task.CompletedTask;
			}
			catch (Exception e)
			{
				return Task.FromException(e);
			}
		}

		var taskCompletionSource = new TaskCompletionSource<object>();

		_ = _dispatcher.RunAsync(priority, () =>
		{
			try
			{
				function();

				taskCompletionSource.SetResult(null);
			}
			catch (Exception e)
			{
				taskCompletionSource.SetException(e);
			}
		});

		return taskCompletionSource.Task;
	}

	private static Task<T> AwaitableRunAsync<T>(CoreDispatcher dispatcher, Func<T> function, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
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

	private static CoreDispatcherPriority ToDispatcherPriority(DispatcherPriority priority)
	{
		return priority switch
		{
			DispatcherPriority.SystemIdle => CoreDispatcherPriority.Idle,
			DispatcherPriority.ApplicationIdle => CoreDispatcherPriority.Idle,
			DispatcherPriority.ContextIdle => CoreDispatcherPriority.Idle,
			DispatcherPriority.Background => CoreDispatcherPriority.Low,
			DispatcherPriority.Normal => CoreDispatcherPriority.Normal,
			_ => CoreDispatcherPriority.Normal
		};
	}

	#endregion
}