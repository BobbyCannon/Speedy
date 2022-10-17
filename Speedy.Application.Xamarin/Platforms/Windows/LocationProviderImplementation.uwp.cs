#region References

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Speedy.Devices.Location;
using PositionChangedEventArgs = Windows.Devices.Geolocation.PositionChangedEventArgs;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
public class LocationProviderImplementation : LocationProvider
{
	#region Fields

	private double _desiredAccuracy;
	private Geolocator _locator;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for Implementation
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_locator = new Geolocator();
		_desiredAccuracy = 10;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Desired accuracy in meters
	/// </summary>
	public override double DesiredAccuracy
	{
		get => _desiredAccuracy;
		set
		{
			_desiredAccuracy = value;
			GetGeolocator().DesiredAccuracy = value < 100 ? PositionAccuracy.High : PositionAccuracy.Default;
		}
	}

	/// <summary>
	/// Gets if geolocation is available on device
	/// </summary>
	public override bool IsGeolocationAvailable
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

	/// <summary>
	/// Gets if geolocation is enabled on device
	/// </summary>
	public override bool IsGeolocationEnabled
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

	/// <summary>
	/// Gets if device supports heading
	/// </summary>
	public override bool SupportsHeading => true;

	#endregion

	#region Methods

	/// <summary>
	/// Gets the last known and most accurate location.
	/// This is usually cached and best to display first before querying for full position.
	/// </summary>
	/// <returns> Best and most recent location or null if none found </returns>
	public override Task<IProviderLocation> GetLastKnownLocationAsync()
	{
		return Task.Factory.StartNew<IProviderLocation>(() => null);
	}

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <param name="includeHeading"> If you would like to include heading </param>
	/// <returns> ProviderLocation </returns>
	public override Task<IProviderLocation> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
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

		var tcs = new TaskCompletionSource<IProviderLocation>();

		pos.Completed = (op, s) =>
		{
			timer.Cancel();

			switch (s)
			{
				case AsyncStatus.Canceled:
					tcs.SetCanceled();
					break;
				case AsyncStatus.Completed:
					tcs.SetResult(UpdateLastReadPosition(op.GetResults()));
					break;
				case AsyncStatus.Error:
					var ex = op.ErrorCode;
					if (ex is UnauthorizedAccessException)
					{
						ex = new GeolocationException(GeolocationError.Unauthorized, ex);
					}

					tcs.SetException(ex);
					break;
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

		var loc = GetGeolocator();

		loc.ReportInterval = (uint) ListenerSettings.MinimumTime.TotalMilliseconds;
		loc.MovementThreshold = ListenerSettings.MinimumDistance;
		loc.PositionChanged += OnLocatorPositionChanged;
		loc.StatusChanged += OnLocatorStatusChanged;

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

	private void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs e)
	{
		UpdateLastReadPosition(e.Position);
		OnPositionChanged(new PositionEventArgs(LastReadPosition));
	}

	private async void OnLocatorStatusChanged(Geolocator sender, StatusChangedEventArgs e)
	{
		GeolocationError error;
		switch (e.Status)
		{
			case PositionStatus.Disabled:
				error = GeolocationError.Unauthorized;
				break;

			case PositionStatus.NoData:
				error = GeolocationError.PositionUnavailable;
				break;

			default:
				return;
		}

		if (IsListening)
		{
			await StopListeningAsync();
			OnPositionError(new PositionErrorEventArgs(error));
		}

		_locator = null;
	}

	private void SetMapKey(string mapKey)
	{
		if (string.IsNullOrWhiteSpace(mapKey) && string.IsNullOrWhiteSpace(MapService.ServiceToken))
		{
			Debug.WriteLine("Map API key is required on UWP to reverse geolocate.");
			throw new ArgumentNullException(nameof(mapKey));
		}

		if (!string.IsNullOrWhiteSpace(mapKey))
		{
			MapService.ServiceToken = mapKey;
		}
	}

	private IProviderLocation UpdateLastReadPosition(Geoposition position)
	{
		LastReadPosition.HasAccuracy = true;
		LastReadPosition.Accuracy = position.Coordinate.Accuracy;
		LastReadPosition.HasLatitudeLongitude = true;
		LastReadPosition.Latitude = position.Coordinate.Point.Position.Latitude;
		LastReadPosition.Longitude = position.Coordinate.Point.Position.Longitude;
		LastReadPosition.StatusTime = position.Coordinate.Timestamp.UtcDateTime;

		if (position.Coordinate.Heading != null)
		{
			LastReadPosition.HasHeading = true;
			LastReadPosition.Heading = position.Coordinate.Heading.Value;
		}

		if (position.Coordinate.Speed != null)
		{
			LastReadPosition.HasSpeed = true;
			LastReadPosition.Speed = position.Coordinate.Speed.Value;
		}

		if (position.Coordinate.AltitudeAccuracy.HasValue)
		{
			LastReadPosition.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;
		}

		LastReadPosition.Altitude = position.Coordinate.Point.Position.Altitude;
		LastReadPosition.AltitudeReference = position.Coordinate.Point.AltitudeReferenceSystem.ToAltitudeReferenceType();
		LastReadPosition.HasAltitude = LastReadPosition.AltitudeReference != AltitudeReferenceType.Unspecified;
		LastReadPosition.SourceName = position.Coordinate.PositionSource.ToString();

		return LastReadPosition;
	}

	#endregion
}