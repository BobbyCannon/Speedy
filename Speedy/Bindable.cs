#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class Bindable<T> : Bindable, IUpdatable<T>
{
	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to update with. </param>
	protected Bindable(IDispatcher dispatcher = null) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual bool Refresh(T update, params string[] exclusions)
	{
		return this.Refresh<T>(update, exclusions);
	}

	/// <inheritdoc />
	public virtual bool ShouldUpdate(T update)
	{
		return true;
	}

	/// <inheritdoc />
	public abstract bool UpdateWith(T update, params string[] exclusions);

	#endregion
}

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class Bindable : IBindable, IUpdatable
{
	#region Fields

	private bool _pausePropertyChanged;
	private Type _realType;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The dispatcher to update with. </param>
	protected Bindable(IDispatcher dispatcher = null)
	{
		Dispatcher = dispatcher;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Determines if the object has changes.
	/// </summary>
	[Browsable(false)]
	[JsonIgnore]
	[IgnoreDataMember]
	public virtual bool HasChanges { get; private set; }

	/// <summary>
	/// Represents a thread dispatcher to help with cross threaded request.
	/// </summary>
	[Browsable(false)]
	[JsonIgnore]
	[IgnoreDataMember]
	protected IDispatcher Dispatcher { get; private set; }

	/// <summary>
	/// Cached version of the "real" type, meaning not EF proxy but rather root type
	/// </summary>
	internal Type RealType => _realType ??= this.GetRealType();

	#endregion

	#region Methods

	/// <inheritdoc />
	public IDispatcher GetDispatcher()
	{
		return Dispatcher;
	}

	/// <inheritdoc />
	public virtual bool IsChangeNotificationsPaused()
	{
		return _pausePropertyChanged;
	}

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		// Ensure we have not paused property notifications
		if ((propertyName == null) || _pausePropertyChanged)
		{
			// Property change notifications have been paused so bounce
			return;
		}

		// If we have a dispatcher *and* not on the dispatcher thread
		// then switch to to the dispatcher thread to trigger property changed
		if (Dispatcher?.IsDispatcherThread == false)
		{
			Dispatcher.Run(() => OnPropertyChanged(propertyName));
			return;
		}

		OnPropertyChangedInDispatcher(propertyName);

		if ((propertyName != nameof(HasChanges)) && !HasChanges)
		{
			HasChanges = true;
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc />
	public virtual void PausePropertyChangeNotifications(bool pause = true)
	{
		_pausePropertyChanged = pause;
	}

	/// <summary>
	/// Reset the change tracking flag.
	/// </summary>
	/// <param name="hasChanges"> An optional value to indicate if this object has changes. Defaults to false. </param>
	public virtual void ResetChangeTracking(bool hasChanges = false)
	{
		HasChanges = hasChanges;
	}

	/// <inheritdoc />
	public virtual bool Refresh(object update, params string[] exclusions)
	{
		return UpdatableExtensions.Refresh(this, update, exclusions);
	}

	/// <inheritdoc />
	public virtual bool ShouldUpdate(object update)
	{
		return true;
	}

	/// <inheritdoc />
	public virtual void UpdateDispatcher(IDispatcher dispatcher)
	{
		Dispatcher = dispatcher;
	}

	/// <inheritdoc />
	public virtual bool UpdateWith(object update, params string[] exclusions)
	{
		this.UpdateWithUsingReflection(update, exclusions);
		return true;
	}

	/// <inheritdoc />
	public bool UpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
	{
		var totalExclusions = new HashSet<string>(exclusions);
		if (excludeVirtuals)
		{
			totalExclusions.AddRange(RealType.GetVirtualPropertyNames());
		}

		return UpdateWith(update, totalExclusions.ToArray());
	}

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyChangedEvent"> The changed event value of the property that changed. </param>
	protected void OnPropertyChanged(PropertyChangedEventArgs propertyChangedEvent)
	{
		OnPropertyChanged(propertyChangedEvent.PropertyName);
	}

	/// <summary>
	/// fires the OnPropertyChanged notice for the bindable object on the dispatcher thread.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	protected virtual void OnPropertyChangedInDispatcher(string propertyName)
	{
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event PropertyChangedEventHandler PropertyChanged;

	#endregion
}