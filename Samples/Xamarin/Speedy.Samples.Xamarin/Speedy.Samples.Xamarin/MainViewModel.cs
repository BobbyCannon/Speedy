#region References

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using Speedy.Application;
using Speedy.Application.Xamarin;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Speedy.Devices.Location;
using Xamarin.Essentials;
using Location = Speedy.Devices.Location.Location;

#endregion

namespace Speedy.Samples.Xamarin;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, BaseObservableCollection<Location>>();
		LocationManager = new LocationManager(dispatcher);
		LocationManager.Refreshed += LocationManagerOnRefreshed;
		Locations = new BaseObservableCollection<Location>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(25);
		
		var provider = new XamarinLocationProvider<Location, LocationProviderSettingsView>(dispatcher);
		var provider2 = new XamarinBarometerLocationProvider<Location>(dispatcher);
		
		LocationManager.Add(provider);
		LocationManager.Add(provider2);

		DeviceDisplay.KeepScreenOn = true;

		ExportHistoryCommand = new RelayCommand(x => OnExportHistoryRequest());
	}

	#endregion

	#region Properties

	public ISeries[] Series { get; set; }
		= {
			new LineSeries<int>
			{
				Values = new[] { 2, 5, 4, -2, 4, -3, 5 }
			}
		};


	public Axis[] XAxes { get; set; }
		= {
			new Axis
			{
				Name = "X Axis",
				NamePaint = new SolidColorPaint(SKColors.Black), 

				LabelsPaint = new SolidColorPaint(SKColors.Blue), 
				TextSize = 10,

				SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
				{
					StrokeThickness = 2
				}
			}
		};

	public Axis[] YAxes { get; set; }
		= {
			new Axis
			{
				Name = "Y Axis",
				NamePaint = new SolidColorPaint(SKColors.Red), 

				LabelsPaint = new SolidColorPaint(SKColors.Green), 
				TextSize = 20,

				SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
				{ 
					StrokeThickness = 2, 
					PathEffect = new DashEffect(new float[] { 3, 3 }) 
				} 
			}
		};

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, BaseObservableCollection<Location>> LocationHistory { get; }

	//public LineChart AltitudeChart { get; }

	public LocationManager LocationManager { get; }

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

	private void LocationManagerOnRefreshed(object sender, Location e)
	{
		ProcessLocation(e);
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
		history.Add((Location) currentLocation.ShallowClone());
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}