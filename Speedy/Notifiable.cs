#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Represents a notifiable object.
/// </summary>
public abstract class Notifiable : INotifiable, IUpdateable
{
	#region Fields

	private bool _notificationsEnabled;
	private Type _realType;
	private bool _hasChanges;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a notifiable object.
	/// </summary>
	protected Notifiable()
	{
		_notificationsEnabled = true;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual void DisablePropertyChangeNotifications()
	{
		_notificationsEnabled = false;
	}

	/// <inheritdoc />
	public virtual void EnablePropertyChangeNotifications()
	{
		_notificationsEnabled = true;
	}

	/// <summary>
	/// Cached version of the "real" type, meaning not EF proxy but rather root type
	/// </summary>
	public Type GetRealType()
	{
		return _realType ??= this.GetRealTypeUsingReflection();
	}

	/// <inheritdoc />
	public virtual bool HasChanges()
	{
		return _hasChanges;
	}

	/// <inheritdoc />
	public virtual bool IsPropertyChangeNotificationsEnabled()
	{
		return _notificationsEnabled;
	}

	/// <summary>
	/// Indicates the property has changed on the notifiable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public virtual void OnPropertyChanged(string propertyName)
	{
		// Ensure we have not paused property notifications
		if ((propertyName == null) || !IsPropertyChangeNotificationsEnabled())
		{
			// Property change notifications have been paused or property null so bounce
			return;
		}

		if (propertyName != nameof(HasChanges))
		{
			_hasChanges = true;
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc />
	public virtual void ResetHasChanges()
	{
		_hasChanges = false;
	}

	/// <inheritdoc />
	public virtual bool ShouldUpdate(object update)
	{
		return true;
	}

	/// <inheritdoc />
	public virtual bool TryUpdateWith(object update, params string[] exclusions)
	{
		return UpdateableExtensions.TryUpdateWith(this, update, exclusions);
	}

	/// <summary>
	/// Allows updating of one type to another based on member Name and Type.
	/// </summary>
	/// <param name="update"> The source of the update. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public virtual bool UpdateWith(object update, params string[] exclusions)
	{
		return this.UpdateWithUsingReflection(update, exclusions);
	}

	/// <inheritdoc />
	public virtual bool UpdateWith(object update, bool excludeVirtuals, params string[] exclusions)
	{
		var totalExclusions = new HashSet<string>(exclusions);
		if (excludeVirtuals)
		{
			totalExclusions.AddRange(GetRealType().GetVirtualPropertyNames());
		}

		return UpdateWith(update, totalExclusions.ToArray());
	}

	/// <summary>
	/// Indicates the property has changed on the notifiable object.
	/// </summary>
	/// <param name="propertyChangedEvent"> The changed event value of the property that changed. </param>
	protected void OnPropertyChanged(PropertyChangedEventArgs propertyChangedEvent)
	{
		OnPropertyChanged(propertyChangedEvent.PropertyName);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event PropertyChangedEventHandler PropertyChanged;

	#endregion
}

/// <summary>
/// Represents a notifiable object.
/// </summary>
public interface INotifiable : IChangeable, INotifyPropertyChanged
{
	#region Methods

	/// <summary>
	/// Disable the property change notifications
	/// </summary>
	public void DisablePropertyChangeNotifications();

	/// <summary>
	/// Enable the property change notifications
	/// </summary>
	public void EnablePropertyChangeNotifications();

	/// <summary>
	/// Return true if the change notifications are enabled or otherwise false.
	/// </summary>
	public bool IsPropertyChangeNotificationsEnabled();

	/// <summary>
	/// Indicates the property has changed on the notifiable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public void OnPropertyChanged(string propertyName);

	#endregion
}