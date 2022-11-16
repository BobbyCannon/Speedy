﻿#region References

using System.Windows.Input;
using Speedy.Commands;

#endregion

namespace Speedy.Devices.Location;

/// <summary>
/// The manager for location.
/// </summary>
public class LocationManager<T> : DeviceInformationManager<Location>
	where T : ILocationProviderSettings, IBindable, new()
{
	#region Constructors

	/// <inheritdoc />
	public LocationManager(IDispatcher dispatcher) : base(dispatcher)
	{
		Settings = new T();
		Settings.UpdateDispatcher(dispatcher);

		// Commands
		StartListeningCommand = new RelayCommand(_ => StartMonitoringAsync());
		StopListeningCommand = new RelayCommand(_ => StopMonitoringAsync());
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override string ProviderName => "Location Manager";

	/// <summary>
	/// The settings for the location manager.
	/// </summary>
	public T Settings { get; }

	/// <summary>
	/// Invokes the start listening method.
	/// </summary>
	public ICommand StartListeningCommand { get; }

	/// <summary>
	/// Invokes the stop listening method.
	/// </summary>
	public ICommand StopListeningCommand { get; }

	#endregion
}