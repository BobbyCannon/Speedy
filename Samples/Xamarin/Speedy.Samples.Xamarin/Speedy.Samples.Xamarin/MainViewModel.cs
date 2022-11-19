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
using Speedy.Extensions;
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
		LocationManager = new LocationManager<LocationProviderSettingsView>(dispatcher);
		LocationManager.Updated += LocationManagerOnChanged;
		Locations = new BaseObservableCollection<ILocationDeviceInformation>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(25);

		var provider = new XamarinLocationProvider<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettingsView>(dispatcher);
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

	public LocationManager<LocationProviderSettingsView> LocationManager { get; }

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
		switch (e)
		{
			case Location location:
			{
				ProcessLocation(location.HorizontalLocation);
				ProcessLocation(location.VerticalLocation);
				break;
			}
			case ILocation<IHorizontalLocation, IVerticalLocation> location:
			{
				ProcessLocation(location.HorizontalLocation);
				ProcessLocation(location.VerticalLocation);
				break;
			}
			case IHorizontalLocation location:
			{
				ProcessLocation(location);
				break;
			}
			case IVerticalLocation location:
			{
				ProcessLocation(location);
				break;
			}
		}
	}

	private void LocationProviderOnLogEventWritten(object sender, LogEventArgs e)
	{
		Dispatcher.Run(() => Logs.Insert(0, e));
	}

	private void ProcessLocation(ILocationDeviceInformation location)
	{
		if (!Dispatcher.IsDispatcherThread)
		{
			Dispatcher.Run(() => ProcessLocation(location));
			return;
		}

		if (location is not { HasValue: true })
		{
			// Location is null or does not have a value so bounce
			return;
		}

		if (string.IsNullOrWhiteSpace(location.SourceName))
		{
			// Invalid source name!!!
			return;
		}

		var currentLocation = Locations.FirstOrDefault(x => x.CalculateKey() == location.CalculateKey());
		if (currentLocation == null)
		{
			Locations.Add(location);
			currentLocation = location;
		}
		else
		{
			if (currentLocation.StatusTime == location.StatusTime)
			{
				// this is not an update
				return;
			}

			currentLocation.UpdateWith(location);
		}

		var key = location.CalculateKey();
		var history = LocationHistory.GetOrAdd(key,
			_ => new BaseObservableCollection<ILocationDeviceInformation>()
		);

		history.Add(currentLocation);
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}