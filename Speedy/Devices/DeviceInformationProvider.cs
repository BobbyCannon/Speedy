#region References

using System;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Represents a provider of device information.
/// </summary>
public abstract class DeviceInformationProvider<T> : Bindable, IDeviceInformationProvider<T>
	where T : new()
{
	#region Fields

	private T _currentValue;

	#endregion

	#region Constructors

	/// <summary>
	/// Represents a provider of device information.
	/// </summary>
	protected DeviceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		_currentValue = new T();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public T CurrentValue => _currentValue;

	#endregion

	#region Methods

	/// <summary>
	/// Apply the update to the <see cref="CurrentValue" />.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool ApplyUpdate(T update)
	{
		return ApplyUpdate(ref _currentValue, update);
	}

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public abstract bool ApplyUpdate(ref T value, T update);

	/// <inheritdoc />
	public bool ApplyUpdate(object update)
	{
		return ApplyUpdate(ref _currentValue, (T) update);
	}

	/// <inheritdoc />
	public bool ApplyUpdate(ref object value, object update)
	{
		return value is T tValue && ApplyUpdate(ref tValue, (T) update);
	}

	/// <inheritdoc />
	public object GetCurrentValue()
	{
		return CurrentValue;
	}

	/// <inheritdoc />
	public bool ShouldApplyUpdate(object update)
	{
		return ShouldApplyUpdate(ref _currentValue, (T) update);
	}

	/// <inheritdoc />
	public bool ShouldApplyUpdate(ref object value, object update)
	{
		return value is T tValue && ShouldApplyUpdate(ref tValue, (T) update);
	}

	/// <summary>
	/// Determine if the update should be applied to <see cref="CurrentValue" />.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public bool ShouldApplyUpdate(T update)
	{
		return ShouldApplyUpdate(ref _currentValue, update);
	}

	/// <summary>
	/// Determine if the update should be applied to the provided value.
	/// </summary>
	/// <param name="value"> The current value state. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public abstract bool ShouldApplyUpdate(ref T value, T update);

	/// <inheritdoc />
	public bool TryApplyUpdate(object update)
	{
		return TryApplyUpdate(ref _currentValue, (T) update);
	}

	/// <summary>
	/// Try to apply an update to the current provider.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool TryApplyUpdate(T update)
	{
		return TryApplyUpdate(ref _currentValue, update);
	}

	/// <inheritdoc />
	public bool TryApplyUpdate(ref object value, object update)
	{
		return value is T tValue && TryApplyUpdate(ref tValue, (T) update);
	}

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public bool TryApplyUpdate(ref T value, T update)
	{
		return ShouldApplyUpdate(ref value, update) && ApplyUpdate(ref value, update);
	}

	/// <summary>
	/// Triggers the <see cref="Refreshed" /> event with the provided value;
	/// </summary>
	/// <param name="e"> </param>
	protected virtual void OnRefreshed(T e)
	{
		Refreshed?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<object> Refreshed;

	#endregion
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IDeviceInformationProvider<out T> : IDeviceInformationProvider
	where T : new()
{
	#region Properties

	/// <summary>
	/// Represents the current value of the provider.
	/// </summary>
	T CurrentValue { get; }

	#endregion
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IDeviceInformationProvider
{
	#region Methods

	/// <summary>
	/// Apply the update to the current value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool ApplyUpdate(object update);

	/// <summary>
	/// Apply the update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public abstract bool ApplyUpdate(ref object value, object update);

	/// <summary>
	/// Get the current value from the device information provider.
	/// </summary>
	/// <returns> The current value of the provider. </returns>
	object GetCurrentValue();

	/// <summary>
	/// Determine if the update should be applied to the current value.
	/// </summary>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public bool ShouldApplyUpdate(object update);

	/// <summary>
	/// Determine if the update should be applied to the provided value.
	/// </summary>
	/// <param name="value"> The current value state. </param>
	/// <param name="update"> The update to be tested. </param>
	/// <returns> True if the update should be applied otherwise false. </returns>
	public bool ShouldApplyUpdate(ref object value, object update);

	/// <summary>
	/// Try to apply an update to the current provider.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool TryApplyUpdate(object update);

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool TryApplyUpdate(ref object value, object update);

	#endregion

	#region Events

	/// <summary>
	/// An event to notify when the current value is refreshed.
	/// </summary>
	event EventHandler<object> Refreshed;

	#endregion
}