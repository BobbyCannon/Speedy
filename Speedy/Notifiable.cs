#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Speedy.Collections;
using Speedy.Extensions;
using Speedy.Sync;

#endregion

namespace Speedy;

/// <summary>
/// Represents a notifiable object.
/// </summary>
public abstract class Notifiable : INotifiable, IUpdateable
{
	#region Fields

	/// <summary>
	/// The properties that has changed since last <see cref="ResetHasChanges" /> event.
	/// </summary>
	private readonly ConcurrentDictionary<string, DateTime> _changedProperties;
	private bool _pausePropertyChanged;
	private Type _realType;
	private readonly ReadOnlySet<string> _writableProperties;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates a notifiable object.
	/// </summary>
	protected Notifiable()
	{
		if (this is Entity entity)
		{
			SyncEntity.ExclusionCacheForUpdatableExclusions.GetOrAdd(GetRealType(),
				_ => new HashSet<string>(entity.GetDefaultExclusionsForUpdatableExclusions()));

			var exclusions = SyncEntity.ExclusionCacheForChangeTracking.GetOrAdd(GetRealType(),
				_ => new HashSet<string>(entity.GetDefaultExclusionsForChangeTracking()));

			var writeables = Cache.GetWriteables(this);

			_writableProperties = writeables.Where(x => !exclusions.Contains(x)).AsReadOnly();
		}
		else
		{
			_writableProperties = Cache.GetWriteables(this);
		}

		_changedProperties = new ConcurrentDictionary<string, DateTime>();
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public ReadOnlySet<string> GetChangedProperties()
	{
		return new ReadOnlySet<string>(_changedProperties.Keys);
	}

	/// <summary>
	/// Cached version of the "real" type, meaning not EF proxy but rather root type
	/// </summary>
	public Type GetRealType()
	{
		return _realType ??= this.GetRealTypeUsingReflection();
	}

	/// <inheritdoc />
	public bool HasChanges()
	{
		return HasChanges(Array.Empty<string>());
	}

	/// <inheritdoc />
	public virtual bool HasChanges(params string[] exclusions)
	{
		return _changedProperties.Keys.Any(x => !exclusions.Contains(x));
	}

	/// <inheritdoc />
	public virtual bool IsChangeNotificationsPaused()
	{
		return _pausePropertyChanged;
	}

	/// <summary>
	/// Indicates the property has changed on the notifiable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public virtual void OnPropertyChanged(string propertyName)
	{
		// Ensure we have not paused property notifications
		if ((propertyName == null) || IsChangeNotificationsPaused())
		{
			// Property change notifications have been paused or property null so bounce
			return;
		}

		if (_writableProperties?.Contains(propertyName) == true)
		{
			_changedProperties.AddOrUpdate(propertyName, v => TimeService.UtcNow, (k,v) => TimeService.UtcNow);
		}

		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <inheritdoc />
	public virtual void PausePropertyChangeNotifications(bool pause = true)
	{
		_pausePropertyChanged = pause;
	}

	/// <inheritdoc />
	public virtual void ResetHasChanges()
	{
		_changedProperties.Clear();
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
	/// Return true if the change notifications are paused or otherwise false.
	/// </summary>
	public bool IsChangeNotificationsPaused();

	/// <summary>
	/// Indicates the property has changed on the notifiable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	public void OnPropertyChanged(string propertyName);

	/// <summary>
	/// Pause / Un-pause the property change notifications
	/// </summary>
	public void PausePropertyChangeNotifications(bool pause = true);

	#endregion
}