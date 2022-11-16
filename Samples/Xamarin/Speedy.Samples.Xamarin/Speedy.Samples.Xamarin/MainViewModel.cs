#region References

using System;
using System.Collections.Concurrent;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using Speedy.Application;
using Speedy.Application.Xamarin;
using Speedy.Collections;
using Speedy.Commands;
using Speedy.Devices.Location;
using Speedy.Logging;
using Xamarin.Essentials;
using Location = Speedy.Devices.Location.Location;

#endregion

namespace Speedy.Samples.Xamarin;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, BaseObservableCollection<ILocationDeviceInformation>>();
		LocationManager = new LocationManager(dispatcher);
		LocationManager.Changed += LocationManagerOnChanged;
		LocationManager.Refreshed += LocationManagerOnRefreshed;
		Locations = new BaseObservableCollection<ILocationDeviceInformation>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(25);

		var provider = new XamarinLocationProvider<Location, HorizontalLocation, VerticalLocation, LocationProviderSettingsView>(dispatcher);
		var provider2 = new XamarinBarometerLocationProvider<VerticalLocation>(dispatcher);

		LocationManager.Add(provider);
		LocationManager.Add(provider2);

		DeviceDisplay.KeepScreenOn = true;

		ExportHistoryCommand = new RelayCommand(x => OnExportHistoryRequest());
	}

	#endregion

	#region Properties

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, BaseObservableCollection<ILocationDeviceInformation>> LocationHistory { get; }

	//public LineChart AltitudeChart { get; }

	public LocationManager LocationManager { get; }

	public BaseObservableCollection<ILocationDeviceInformation> Locations { get; }

	public BaseObservableCollection<LogEventArgs> Logs { get; }

	public ISeries[] Series { get; set; }
		=
		{
			new LineSeries<int>
			{
				Values = new[] { 2, 5, 4, -2, 4, -3, 5 }
			}
		};

	public Axis[] XAxes { get; set; }
		=
		{
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
		=
		{
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

	#endregion

	#region Methods

	protected virtual void OnExportHistoryRequest()
	{
		ExportHistoryRequest?.Invoke(this, EventArgs.Empty);
	}

	private void LocationManagerOnChanged(object sender, object e)
	{
		ProcessLocation(e as ILocationDeviceInformation);
	}

	private void LocationManagerOnRefreshed(object sender, object e)
	{
		// todo: nothing?
	}

	private void LocationProviderOnLogEventWritten(object sender, LogEventArgs e)
	{
		Dispatcher.Run(() => Logs.Insert(0, e));
	}

	private void ProcessLocation(ILocationDeviceInformation location)
	{
		if (location == null)
		{
			return;
		}

		var currentLocation = Locations.FirstOrDefault(x => x.SourceName == location.SourceName);
		if (currentLocation == null)
		{
			Locations.Add(location);
			currentLocation = location;
		}
		else
		{
			currentLocation.UpdateWith(location);
		}

		var history = LocationHistory.GetOrAdd(location.SourceName, _ => new BaseObservableCollection<ILocationDeviceInformation>());
		history.Add(currentLocation);
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}