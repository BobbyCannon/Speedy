﻿#region References

using System;
using System.Threading.Tasks;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represents a sync model, usually used in a web API model.
/// </summary>
/// <typeparam name="T"> The type for the key. </typeparam>
public abstract class SyncModel<T> : SyncEntity<T>, IBindable
{
	#region Fields

	private IDispatcher _dispatcher;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiate a sync model.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected SyncModel(IDispatcher dispatcher = null)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispatch(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher != null && dispatcher.ShouldDispatch())
		{
			dispatcher.Dispatch(action, priority);
			return;
		}

		action();
	}

	/// <inheritdoc />
	public T2 Dispatch<T2>(Func<T2> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		return dispatcher != null && dispatcher.ShouldDispatch()
			? dispatcher.Dispatch(action, priority)
			: action();
	}

	/// <inheritdoc />
	public Task DispatchAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher != null && dispatcher.ShouldDispatch())
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		action();
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public Task<T2> DispatchAsync<T2>(Func<T2> action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher != null && dispatcher.ShouldDispatch())
		{
			return dispatcher.DispatchAsync(action, priority);
		}

		var result = action();
		return Task.FromResult(result);
	}

	/// <inheritdoc />
	public IDispatcher GetDispatcher()
	{
		return _dispatcher;
	}

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public sealed override void OnPropertyChanged(string propertyName)
	{
		// Ensure we have not paused property notifications
		if ((propertyName == null) || !IsPropertyChangeNotificationsEnabled())
		{
			// Property change notifications have been paused or property null so bounce
			return;
		}

		Dispatch(() =>
		{
			OnPropertyChangedInDispatcher(propertyName);
			base.OnPropertyChanged(propertyName);
		});
	}

	/// <inheritdoc />
	public bool ShouldDispatch()
	{
		var dispatcher = GetDispatcher();
		return dispatcher?.ShouldDispatch() ?? false;
	}

	/// <inheritdoc />
	public virtual void UpdateDispatcher(IDispatcher dispatcher)
	{
		_dispatcher = dispatcher;
	}

	/// <summary>
	/// fires the OnPropertyChanged notice for the bindable object on the dispatcher thread.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	protected virtual void OnPropertyChangedInDispatcher(string propertyName)
	{
	}

	#endregion
}