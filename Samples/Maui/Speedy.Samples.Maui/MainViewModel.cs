#region References

using System;
using System.Collections.Concurrent;
using Microsoft.Maui.Devices;
using Speedy.Application;
using Speedy.Application.Maui;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Data.Location;
using Speedy.Logging;
using Location = Speedy.Data.Location.Location;
using RuntimeInformation = Speedy.Data.RuntimeInformation;

#endregion

namespace Speedy.Samples.Maui;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, SpeedyList<Location>>();
		LocationManager = new LocationManager<LocationProviderSettingsView>(dispatcher);
		//LocationManager.LocationChanged += LocationManagerOnLocationChanged;
		Locations = new SpeedyList<Location>(dispatcher);
		Logs = new SpeedyList<LogEventArgs> { Limit = 25 };
		RuntimeInformation = new MauiRuntimeInformation(dispatcher);

		DeviceDisplay.KeepScreenOn = true;

		// Commands
		ExportHistoryCommand = new RelayCommand(_ => OnExportHistoryRequest());
	}

	#endregion

	#region Properties

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, SpeedyList<Location>> LocationHistory { get; }

	public LocationManager<LocationProviderSettingsView> LocationManager { get; }

	public SpeedyList<Location> Locations { get; }

	public SpeedyList<LogEventArgs> Logs { get; }
	
	public RuntimeInformation RuntimeInformation { get; }

	#endregion

	#region Methods

	protected virtual void OnExportHistoryRequest()
	{
		ExportHistoryRequest?.Invoke(this, EventArgs.Empty);
	}

	private void LocationManagerOnLocationChanged(object sender, Location e)
	{
		ProcessLocation(e.ShallowClone());
	}

	private void LocationProviderOnLogEventWritten(object sender, LogEventArgs e)
	{
		this.Dispatch(() => Logs.Insert(0, e));
	}

	private void ProcessLocation(Location location)
	{
		//var currentLocation = Locations.FirstOrDefault(x => x.HorizontalSourceName == location.HorizontalSourceName);
		//if (currentLocation == null)
		//{
		//	Locations.Add(location);
		//	currentLocation = location;
		//}
		//else
		//{
		//	currentLocation.UpdateWith(location);
		//}

		//var history = LocationHistory.GetOrAdd(location.HorizontalSourceName, _ => new SpeedyList<Location>());
		//history.Add(currentLocation.ShallowClone());
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}