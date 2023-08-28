#region References

using System;
using System.Threading.Tasks;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class Bindable<T> : Bindable, IUpdateable<T>
{
	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected Bindable(IDispatcher dispatcher = null) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual bool ShouldUpdate(T update)
	{
		return true;
	}

	/// <inheritdoc />
	public virtual bool TryUpdateWith(T update, params string[] exclusions)
	{
		return this.TryUpdateWith<T>(update, exclusions);
	}

	/// <inheritdoc />
	public abstract bool UpdateWith(T update, params string[] exclusions);

	#endregion
}

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class Bindable : Notifiable, IBindable
{
	#region Fields

	private IDispatcher _dispatcher;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected Bindable(IDispatcher dispatcher = null)
	{
		_dispatcher = dispatcher;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public void Dispatch(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
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
		return dispatcher is { IsDispatcherThread: false }
			? dispatcher.Dispatch(action, priority)
			: action();
	}

	/// <inheritdoc />
	public Task DispatchAsync(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
	{
		var dispatcher = GetDispatcher();
		if (dispatcher is { IsDispatcherThread: false })
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
		if (dispatcher is { IsDispatcherThread: false })
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
	public override void OnPropertyChanged(string propertyName)
	{
		// Ensure we have not paused property notifications
		if ((propertyName == null) || IsChangeNotificationsPaused())
		{
			// Property change notifications have been paused or property null so bounce
			return;
		}

		DispatchAsync(() =>
		{
			OnPropertyChangedInDispatcher(propertyName);
			base.OnPropertyChanged(propertyName);
		});
	}

	/// <inheritdoc />
	public bool ShouldDispatch()
	{
		var dispatcher = GetDispatcher();
		return dispatcher is { IsDispatcherThread: false };
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

/// <summary>
/// Represents a bindable object.
/// </summary>
public interface IBindable<in T> : IBindable, IUpdateable<T>
{
}

/// <summary>
/// Represents a bindable object.
/// </summary>
public interface IBindable : INotifiable, IDispatchable
{
	#region Methods

	/// <summary>
	/// Get the current dispatcher in use.
	/// </summary>
	/// <returns>
	/// The dispatcher that is currently being used. Null if no dispatcher is assigned.
	/// </returns>
	public IDispatcher GetDispatcher();

	/// <summary>
	/// Updates the entity for this entity.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public void UpdateDispatcher(IDispatcher dispatcher);

	#endregion
}