#region References

using System;
using System.Threading.Tasks;
using Speedy.Serialization;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Represents a provider of device information.
/// </summary>
public abstract class DeviceInformationProvider<T>
	: Comparer<T>, IDeviceInformationProvider<T>
	where T : IUpdatable<T>, new()
{
	#region Constructors

	/// <summary>
	/// Represents a provider of device information.
	/// </summary>
	protected DeviceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		CurrentValue = new T();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public T CurrentValue { get; }

	/// <inheritdoc />
	public Type CurrentValueType => typeof(T);

	/// <inheritdoc />
	public bool HasPermission { get; protected set; }

	/// <inheritdoc />
	public bool IsListening { get; protected set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public bool Refresh(T update)
	{
		return CurrentValue.ShouldUpdate(update)
			&& CurrentValue.UpdateWith(update);
	}

	/// <inheritdoc />
	public bool Refresh(ref T value, T update)
	{
		return value.ShouldUpdate(update)
			&& value.UpdateWith(update);
	}

	/// <inheritdoc />
	public bool Refresh(object update)
	{
		return update is T tUpdate
			&& Refresh(tUpdate);
	}

	/// <inheritdoc />
	public bool Refresh(ref object value, object update)
	{
		return value is T tValue
			&& update is T tUpdate
			&& Refresh(ref tValue, tUpdate);
	}

	/// <inheritdoc />
	public abstract Task StartListeningAsync();

	/// <inheritdoc />
	public abstract Task StopListeningAsync();

	/// <summary>
	/// Triggers the <see cref="OnChanged" /> event when the device information changes.
	/// </summary>
	/// <param name="e"> The new value. </param>
	protected virtual void OnChanged(T e)
	{
		switch (e)
		{
			case ICloneable<T> cloneableT:
			{
				Changed?.Invoke(this, cloneableT.ShallowClone());
				return;
			}
			case ICloneable cloneable:
			{
				Changed?.Invoke(this, cloneable.ShallowClone());
				return;
			}
			default:
			{
				Changed?.Invoke(this, e);
				break;
			}
		}
	}

	/// <summary>
	/// Triggers the <see cref="Refreshed" /> event with the provided value;
	/// </summary>
	/// <param name="e"> The value that was updated. </param>
	protected virtual void OnRefreshed(T e)
	{
		Refreshed?.Invoke(this, e);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<object> Changed;

	/// <inheritdoc />
	public event EventHandler<object> Refreshed;

	#endregion
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IDeviceInformationProvider<T>
	: IDeviceInformationProvider
	where T : IUpdatable<T>, new()
{
	#region Properties

	/// <summary>
	/// Represents the current value.
	/// </summary>
	public T CurrentValue { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(T update);

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(ref T value, T update);

	#endregion
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IDeviceInformationProvider : IUpdatable
{
	#region Properties

	Type CurrentValueType { get; }

	bool HasPermission { get; }

	bool IsListening { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(object update);

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(ref object value, object update);

	/// <summary>
	/// Start monitoring for device information changes.
	/// </summary>
	Task StartListeningAsync();

	/// <summary>
	/// Stop monitoring for device information changes.
	/// </summary>
	Task StopListeningAsync();

	#endregion

	#region Events

	/// <summary>
	/// An event to notify when the device information has changed.
	/// </summary>
	event EventHandler<object> Changed;

	/// <summary>
	/// An event to notify when the device information current value was refreshed.
	/// </summary>
	event EventHandler<object> Refreshed;

	#endregion
}