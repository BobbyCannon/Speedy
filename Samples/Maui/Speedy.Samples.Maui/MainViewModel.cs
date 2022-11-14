#region References

using System.Collections.Concurrent;
using Speedy.Application;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Devices.Location;
using Speedy.Logging;
using Location = Speedy.Devices.Location.Location;

#endregion

namespace Speedy.Samples.Maui;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, BaseObservableCollection<Location>>();
		LocationManager = new LocationManager<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettingsView>(dispatcher);
		LocationManager.LocationChanged += LocationManagerOnLocationChanged;
		Locations = new BaseObservableCollection<Location>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(25);

		//var provider = new XamarinLocationProvider<Location, LocationProviderSettingsView>(dispatcher);
		//var provider2 = new XamarinBarometerLocationProvider<Location>(dispatcher);

		//LocationManager.LocationProviders.Add(provider);
		//LocationManager.LocationProviders.Add(provider2);

		DeviceDisplay.KeepScreenOn = true;

		ExportHistoryCommand = new RelayCommand(x => OnExportHistoryRequest());
	}

	#endregion

	#region Properties

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, BaseObservableCollection<Location>> LocationHistory { get; }

	public LocationManager<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettingsView> LocationManager { get; }

	public BaseObservableCollection<Location> Locations { get; }

	public BaseObservableCollection<LogEventArgs> Logs { get; }

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
		Dispatcher.Run(() => Logs.Insert(0, e));
	}

	private void ProcessLocation(Location location)
	{
		var currentLocation = Locations.FirstOrDefault(x => x.HorizontalSourceName == location.HorizontalSourceName);
		if (currentLocation == null)
		{
			Locations.Add(location);
			currentLocation = location;
		}
		else
		{
			currentLocation.UpdateWith(location);
		}

		var history = LocationHistory.GetOrAdd(location.HorizontalSourceName, _ => new BaseObservableCollection<Location>());
		history.Add(currentLocation.ShallowClone());
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}