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
using Speedy.Data.Location;
using Speedy.Extensions;
using Speedy.Logging;

#endregion

namespace Speedy.Samples.Xamarin;

public class MainViewModel : ViewModel
{
	#region Constructors

	public MainViewModel(IDispatcher dispatcher) : base(dispatcher)
	{
		LocationHistory = new ConcurrentDictionary<string, BaseObservableCollection<ILocationInformation>>();
		LocationManager = new LocationManager<LocationProviderSettingsView>(dispatcher);
		LocationManager.ProviderUpdated += LocationManagerOnProviderUpdated;
		Locations = new BaseObservableCollection<ILocationInformation>(dispatcher);
		Logs = new LimitedObservableCollection<LogEventArgs>(25);

		var provider = new XamarinLocationProvider<Location, IHorizontalLocation, IVerticalLocation, LocationProviderSettingsView>(dispatcher);
		var provider2 = new XamarinBarometerLocationProvider<VerticalLocation>(dispatcher);

		LocationManager.Add(provider);
		LocationManager.Add(provider2);

		// Commands
		ClearLogCommand = new RelayCommand(x => OnClearLogRequest());
		ExportHistoryCommand = new RelayCommand(x => OnExportHistoryRequest());
		ResetCommand = new RelayCommand(x => OnResetRequest());
	}

	#endregion

	#region Properties

	public RelayCommand ClearLogCommand { get; }

	public RelayCommand ExportHistoryCommand { get; }

	public ConcurrentDictionary<string, BaseObservableCollection<ILocationInformation>> LocationHistory { get; }

	//public LineChart AltitudeChart { get; }

	public LocationManager<LocationProviderSettingsView> LocationManager { get; }

	public BaseObservableCollection<ILocationInformation> Locations { get; }

	public BaseObservableCollection<LogEventArgs> Logs { get; }

	public RelayCommand ResetCommand { get; }

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

	public void ProcessLocation(ILocationInformation location)
	{
		if (!Dispatcher.IsDispatcherThread)
		{
			Dispatcher.Run(() => ProcessLocation(location));
			return;
		}

		if (location is not { HasValue: true })
		{
			// Location is null or does not have a value so bounce
			WriteLogEntry($"Ignore {location?.GetType().Name ?? "Unknown"} because HasValue is false.");
			return;
		}

		var key = location.CalculateKey();
		if (string.IsNullOrWhiteSpace(key))
		{
			// Invalid source name!!!
			WriteLogEntry($"Ignore {key} because it's invalid...");
			return;
		}

		//WriteLogEntry($"Processing {key} {location.StatusTime}...");
		
		var currentLocation = Locations.FirstOrDefault(x => x.CalculateKey() == key);
		if (currentLocation == null)
		{
			Locations.Add(location);
			currentLocation = location;
		}
		else
		{
			if ((currentLocation.CalculateKey() == key)
				&& (currentLocation.StatusTime == location.StatusTime))
			{
				// this is not an update
				WriteLogEntry($"Ignore {location.GetType().Name} @ {location.StatusTime} because status time is the same.");
				return;
			}

			currentLocation.UpdateWith(location);
		}

		var history = LocationHistory.GetOrAdd(key,
			_ => new BaseObservableCollection<ILocationInformation>()
		);

		history.Add(currentLocation.ShallowClone());
	}

	protected virtual void OnExportHistoryRequest()
	{
		ExportHistoryRequest?.Invoke(this, EventArgs.Empty);
	}

	private void LocationManagerOnProviderUpdated(object sender, object e)
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
		WriteLogEntry(e);
	}

	private void OnClearLogRequest()
	{
		Dispatcher.Run(() => Logs.Clear());
	}

	private void OnResetRequest()
	{
		Dispatcher.Run(() =>
		{
			LocationManager.StopListeningCommand.Execute(null);
			LocationManager.CurrentValue.HorizontalLocation.UpdateWith(new HorizontalLocation());
			LocationManager.CurrentValue.VerticalLocation.UpdateWith(new VerticalLocation());
			LocationHistory.Clear();
			Locations.Clear();
			Logs.Clear();
		});
	}

	private void WriteLogEntry(string message)
	{
		WriteLogEntry(new LogEventArgs(message));
	}

	private void WriteLogEntry(LogEventArgs args)
	{
		Dispatcher.Run(() => Logs.Insert(0, args));
	}

	#endregion

	#region Events

	public event EventHandler ExportHistoryRequest;

	#endregion
}