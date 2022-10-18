#region References

using System.Diagnostics;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Speedy.Devices.Location;
using PositionChangedEventArgs = Windows.Devices.Geolocation.PositionChangedEventArgs;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
public class LocationProviderImplementation<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	#region Fields

	private Geolocator _locator;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for Implementation
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_locator = new Geolocator();
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsLocationAvailable
	{
		get
		{
			var status = GetGeolocatorStatus();

			while (status == PositionStatus.Initializing)
			{
				Task.Delay(10).Wait();
				status = GetGeolocatorStatus();
			}

			return status != PositionStatus.NotAvailable;
		}
	}

	/// <inheritdoc />
	public override bool IsLocationEnabled
	{
		get
		{
			var status = GetGeolocatorStatus();

			while (status == PositionStatus.Initializing)
			{
				Task.Delay(10).Wait();
				status = GetGeolocatorStatus();
			}

			return (status != PositionStatus.Disabled) && (status != PositionStatus.NotAvailable);
		}
	}

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var timeoutMilliseconds = timeout.HasValue ? (int) timeout.Value.TotalMilliseconds : Timeout.Infite;

		if ((timeoutMilliseconds < 0) && (timeoutMilliseconds != Timeout.Infite))
		{
			throw new ArgumentOutOfRangeException(nameof(timeout));
		}

		cancelToken ??= CancellationToken.None;

		var pos = GetGeolocator().GetGeopositionAsync(TimeSpan.FromTicks(0), TimeSpan.FromDays(365));
		cancelToken.Value.Register(o => ((IAsyncOperation<Geoposition>) o).Cancel(), pos);

		var timer = new Timeout(timeoutMilliseconds, pos.Cancel);

		var tcs = new TaskCompletionSource<T>();

		pos.Completed = (op, s) =>
		{
			timer.Cancel();

			switch (s)
			{
				case AsyncStatus.Canceled:
				{
					tcs.SetCanceled();
					break;
				}
				case AsyncStatus.Completed:
				{
					tcs.SetResult(UpdateLastReadPosition(op.GetResults()));
					break;
				}
				case AsyncStatus.Error:
				{
					var ex = op.ErrorCode;
					if (ex is UnauthorizedAccessException)
					{
						ex = new LocationProviderException(LocationProviderError.Unauthorized, ex);
					}

					tcs.SetException(ex);
					break;
				}
			}
		};

		return tcs.Task;
	}

	/// <inheritdoc />
	public override Task<bool> StartListeningAsync()
	{
		if (IsListening)
		{
			return Task.FromResult(true);
		}

		IsListening = true;

		var geolocator = GetGeolocator();

		geolocator.ReportInterval = (uint) LocationProviderSettings.MinimumTime.TotalMilliseconds;
		geolocator.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy < 100 ? PositionAccuracy.High : PositionAccuracy.Default;
		geolocator.MovementThreshold = LocationProviderSettings.MinimumDistance;
		geolocator.PositionChanged += OnLocatorPositionChanged;
		geolocator.StatusChanged += OnLocatorStatusChanged;

		return Task.FromResult(true);
	}

	/// <summary>
	/// Stop listening
	/// </summary>
	public override Task<bool> StopListeningAsync()
	{
		if (!IsListening)
		{
			return Task.FromResult(true);
		}

		_locator.PositionChanged -= OnLocatorPositionChanged;
		_locator.StatusChanged -= OnLocatorStatusChanged;

		IsListening = false;

		return Task.FromResult(true);
	}

	private Geolocator GetGeolocator()
	{
		var locator = _locator;

		if (locator != null)
		{
			return locator;
		}

		_locator = new Geolocator();
		_locator.StatusChanged += OnLocatorStatusChanged;

		return _locator;
	}

	private PositionStatus GetGeolocatorStatus()
	{
		var loc = GetGeolocator();
		return loc.LocationStatus;
	}

	private void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs args)
	{
		UpdateLastReadPosition(args.Position);
		OnPositionChanged(LastReadLocation);
	}

	private async void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs args)
	{
		LocationProviderError error;

		switch (args.Status)
		{
			case PositionStatus.Disabled:
			{
				error = LocationProviderError.Unauthorized;
				break;
			}
			case PositionStatus.NoData:
			{
				error = LocationProviderError.PositionUnavailable;
				break;
			}
			case PositionStatus.Ready:
			case PositionStatus.Initializing:
			case PositionStatus.NotInitialized:
			case PositionStatus.NotAvailable:
			default:
			{
				return;
			}
		}

		if (IsListening)
		{
			await StopListeningAsync();
			OnPositionError(error);
		}

		_locator = null;
	}

	private void SetMapKey(string mapKey)
	{
		if (string.IsNullOrWhiteSpace(mapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
		{
			Debug.WriteLine("Map API key is required on UWP to reverse geo locate.");
			throw new ArgumentNullException(nameof(mapKey));
		}

		if (!string.IsNullOrWhiteSpace(mapKey))
		{
			MapService.ServiceToken = mapKey;
		}
	}

	private T UpdateLastReadPosition(Geoposition position)
	{
		LastReadLocation.Accuracy = position.Coordinate.Accuracy;
		LastReadLocation.AccuracyReference = AccuracyReferenceType.Meters;
		LastReadLocation.Latitude = position.Coordinate.Point.Position.Latitude;
		LastReadLocation.Longitude = position.Coordinate.Point.Position.Longitude;
		LastReadLocation.StatusTime = position.Coordinate.Timestamp.UtcDateTime;

		if (position.Coordinate.Heading != null)
		{
			LastReadLocation.HasHeading = true;
			LastReadLocation.Heading = position.Coordinate.Heading.Value;
		}

		if (position.Coordinate.Speed != null)
		{
			LastReadLocation.HasSpeed = true;
			LastReadLocation.Speed = position.Coordinate.Speed.Value;
		}

		if (position.Coordinate.AltitudeAccuracy.HasValue)
		{
			LastReadLocation.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;
		}

		LastReadLocation.Altitude = position.Coordinate.Point.Position.Altitude;
		LastReadLocation.AltitudeReference = position.Coordinate.Point.AltitudeReferenceSystem.ToAltitudeReferenceType();
		LastReadLocation.SourceName = position.Coordinate.PositionSource.ToString();

		return LastReadLocation;
	}

	#endregion
}