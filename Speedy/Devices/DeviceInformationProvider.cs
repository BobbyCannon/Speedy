#region References

using System;
using System.ComponentModel;
using System.Threading.Tasks;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Represents a provider of device information.
/// </summary>
public abstract class DeviceInformationProvider<T>
	: Comparer<T>, IDeviceInformationProvider<T>
	where T : IUpdatable<T>, IUpdatable, new()
{
	#region Constructors

	/// <summary>
	/// Represents a provider of device information.
	/// </summary>
	protected DeviceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
		CurrentValue = new T();
		IsEnabled = true;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public T CurrentValue { get; }

	/// <inheritdoc />
	public Type CurrentValueType => typeof(T);

	/// <inheritdoc />
	public bool IsEnabled { get; set; }

	/// <inheritdoc />
	public bool IsMonitoring { get; protected set; }

	/// <inheritdoc />
	public abstract string ProviderName { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public bool Refresh(T update)
	{
		var response = CurrentValue.ShouldUpdate(update)
			&& CurrentValue.UpdateWith(update);

		if (response)
		{
			OnUpdated(CurrentValue);
		}

		return response;
	}

	/// <inheritdoc />
	public bool Refresh(ref T value, T update)
	{
		return value.ShouldUpdate(update)
			&& value.UpdateWith(update);
	}

	/// <inheritdoc />
	public bool Refresh(IUpdatable update)
	{
		return update is T tUpdate
			&& Refresh(tUpdate);
	}

	/// <inheritdoc />
	public bool Refresh(ref IUpdatable value, IUpdatable update)
	{
		return value is T tValue
			&& update is T tUpdate
			&& Refresh(ref tValue, tUpdate);
	}

	/// <inheritdoc />
	public abstract Task StartMonitoringAsync();

	/// <inheritdoc />
	public abstract Task StopMonitoringAsync();

	/// <summary>
	/// Triggers the <see cref="OnUpdated" /> event when the device information changes.
	/// </summary>
	/// <param name="e"> The new value. </param>
	protected virtual void OnUpdated(T e)
	{
		switch (e)
		{
			case ICloneable<T> cloneableT:
			{
				Updated?.Invoke(this, cloneableT.ShallowClone());
				return;
			}
			case ICloneable cloneable:
			{
				Updated?.Invoke(this, (IUpdatable) cloneable.ShallowClone());
				return;
			}
			default:
			{
				Updated?.Invoke(this, e);
				break;
			}
		}
	}

	/// <summary>
	/// Update the providers CurrentValue then triggers OnUpdated.
	/// </summary>
	/// <param name="update"> The update. </param>
	protected void UpdateCurrentValue(T update)
	{
		CurrentValue.UpdateWith(update);
		OnUpdated(CurrentValue);
	}

	#endregion

	#region Events

	/// <inheritdoc />
	public event EventHandler<IUpdatable> Updated;

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
public interface IDeviceInformationProvider : IUpdatable, INotifyPropertyChanged
{
	#region Properties

	/// <summary>
	/// Get the type of the current value.
	/// </summary>
	Type CurrentValueType { get; }

	/// <summary>
	/// Determines if the provider is enabled.
	/// </summary>
	bool IsEnabled { get; set; }

	/// <summary>
	/// Determines if the provider is listening.
	/// </summary>
	bool IsMonitoring { get; }

	/// <summary>
	/// Gets the name of the provider.
	/// </summary>
	string ProviderName { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(IUpdatable update);

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	bool Refresh(ref IUpdatable value, IUpdatable update);

	/// <summary>
	/// Start monitoring for device information changes.
	/// </summary>
	Task StartMonitoringAsync();

	/// <summary>
	/// Stop monitoring for device information changes.
	/// </summary>
	Task StopMonitoringAsync();

	#endregion

	#region Events

	/// <summary>
	/// An event to notify when the device information was updated.
	/// </summary>
	event EventHandler<IUpdatable> Updated;

	#endregion
}