#region References

using System;
using System.Collections.Concurrent;
using System.Linq;
using Speedy.Application;
using Speedy.Application.Xamarin;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Devices.Location;
using Speedy.Logging;

#endregion

namespace Speedy.Samples.Xamarin;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, BaseObservableCollection<Location>>();
		Locations = new BaseObservableCollection<Location>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(100);
		LocationProvider = new XamarinLocationProvider<Location, LocationProviderSettingsView>(dispatcher);
		LocationProvider.LogEventWritten += LocationProviderOnLogEventWritten;
		LocationProvider.PositionChanged += LocationProviderOnPositionChanged;

		//AltitudeChart = new LineChart()
		//	{
		//		Entries = new List<ChartEntry>(),
		//		LineMode = LineMode.Straight,
		//		LineSize = 8,
		//		PointMode = PointMode.Square,
		//		PointSize = 18
		//	};

		ExportHistoryCommand = new RelayCommand(x => OnExportHistoryRequest());
	}

	#endregion

	#region Properties

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, BaseObservableCollection<Location>> LocationHistory { get; }

	//public LineChart AltitudeChart { get; }

	public XamarinLocationProvider<Location, LocationProviderSettingsView> LocationProvider { get; }

	public BaseObservableCollection<Location> Locations { get; }

	public BaseObservableCollection<LogEventArgs> Logs { get; }

	#endregion

	#region Methods

	protected virtual void OnExportHistoryRequest()
	{
		ExportHistoryRequest?.Invoke(this, EventArgs.Empty);
	}

	private void LocationProviderOnLogEventWritten(object sender, LogEventArgs e)
	{
		Dispatcher.Run(() => Logs.Insert(0, e));
	}

	private void LocationProviderOnPositionChanged(object sender, Location e)
	{
		var currentLocation = Locations.FirstOrDefault(x => x.SourceName == e.SourceName);
		if (currentLocation == null)
		{
			Locations.Add(e);
			currentLocation = e;
		}
		else
		{
			currentLocation.UpdateWith(e);
		}
		
		var history = LocationHistory.GetOrAdd(currentLocation.SourceName, _ => new BaseObservableCollection<Location>());
		history.Add((Location) e.ShallowClone());
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}