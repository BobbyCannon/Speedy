#region References

using System;
using Speedy.Data;

#endregion

namespace Speedy.Devices;

/// <summary>
/// Represents a provider of device information.
/// </summary>
public abstract class DeviceInformationProvider<T> : Comparer<T>, IDeviceInformationProvider<T>
	where T : new()
{
	
	#region Constructors

	/// <summary>
	/// Represents a provider of device information.
	/// </summary>
	protected DeviceInformationProvider(IDispatcher dispatcher) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

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
public interface IDeviceInformationProvider<out T> : IDeviceInformationProvider, IComparer<T>
	where T : new()
{
}

/// <summary>
/// Represents a provider of device information.
/// </summary>
public interface IDeviceInformationProvider : IComparer
{
	#region Events

	/// <summary>
	/// An event to notify when the current value is refreshed.
	/// </summary>
	event EventHandler<object> Refreshed;

	#endregion
}