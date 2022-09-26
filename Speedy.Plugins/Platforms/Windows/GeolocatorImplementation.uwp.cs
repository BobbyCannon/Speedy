#region References

using System.Diagnostics;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Speedy.Devices.Location;
using PositionChangedEventArgs = Windows.Devices.Geolocation.PositionChangedEventArgs;
using Speedy.Plugins.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
    /// <summary>
    /// Implementation for Geolocator
    /// </summary>
    public class GeolocatorImplementation : IGeolocator
	{
		#region Fields

		private double _desiredAccuracy;
		private Geolocator _locator;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for Implementation
		/// </summary>
		public GeolocatorImplementation()
		{
			_locator = new Geolocator();

			DesiredAccuracy = 10;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Desired accuracy in meters
		/// </summary>
		public double DesiredAccuracy
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
		public bool IsGeolocationAvailable
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
		public bool IsGeolocationEnabled
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
		/// Gets if you are listening for location changes
		/// </summary>
		public bool IsListening { get; private set; }

		/// <summary>
		/// Gets if device supports heading
		/// </summary>
		public bool SupportsHeading => false;

		#endregion

		#region Methods

		/// <summary>
		/// Retrieve addresses for position.
		/// </summary>
		/// <param name="position"> Desired position (latitude and longitude) </param>
		/// <param name="mapKey"> The key for the map access. </param>
		/// <returns> Addresses of the desired position </returns>
		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(IGeoLocation position, string mapKey = null)
		{
			if (position == null)
			{
				throw new ArgumentNullException(nameof(position));
			}

			SetMapKey(mapKey);

			var queryResults = await MapLocationFinder.FindLocationsAtAsync(
				new Geopoint(new BasicGeoposition { Latitude = position.Latitude, Longitude = position.Longitude })
			).AsTask();

			return queryResults?.Locations?.ToAddresses() ?? null;
		}

		/// <summary>
		/// Gets the last known and most accurate location.
		/// This is usually cached and best to display first before querying for full position.
		/// </summary>
		/// <returns> Best and most recent location or null if none found </returns>
		public Task<IProviderLocation> GetLastKnownLocationAsync()
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
		public Task<IProviderLocation> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
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
						tcs.SetResult(GetPosition(op.GetResults()));
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

		/// <summary>
		/// Retrieve positions for address.
		/// </summary>
		/// <param name="address"> Desired address </param>
		/// <param name="mapKey"> Map Key required only on UWP </param>
		/// <returns> Positions of the desired address </returns>
		public async Task<IEnumerable<IProviderLocation>> GetPositionsForAddressAsync(string address, string mapKey = null)
		{
			if (address == null)
			{
				throw new ArgumentNullException(nameof(address));
			}

			SetMapKey(mapKey);

			var queryResults = await MapLocationFinder.FindLocationsAsync(address, null, 10);
			var positions = new List<ProviderLocation>();
			if (queryResults?.Locations == null)
			{
				return positions;
			}

			foreach (var p in queryResults.Locations)
			{
				positions.Add(new ProviderLocation
				{
					Latitude = p.Point.Position.Latitude,
					Longitude = p.Point.Position.Longitude
				});
			}

			return positions;
		}

		/// <summary>
		/// Start listening for changes
		/// </summary>
		/// <param name="minimumTime"> Time </param>
		/// <param name="minimumDistance"> Distance </param>
		/// <param name="includeHeading"> Include heading or not </param>
		/// <param name="listenerSettings"> Optional settings (iOS only) </param>
		public Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{
			if ((minimumTime.TotalMilliseconds <= 0) && (minimumDistance <= 0))
			{
				throw new ArgumentException("You must specify either a minimumTime or minimumDistance, setting a minimumDistance will always take precedence over minTime");
			}

			if (minimumTime.TotalMilliseconds < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumTime));
			}

			if (minimumDistance < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));
			}

			if (IsListening)
			{
				throw new InvalidOperationException();
			}

			IsListening = true;

			var loc = GetGeolocator();

			loc.ReportInterval = (uint) minimumTime.TotalMilliseconds;
			loc.MovementThreshold = minimumDistance;
			loc.PositionChanged += OnLocatorPositionChanged;
			loc.StatusChanged += OnLocatorStatusChanged;

			return Task.FromResult(true);
		}

		/// <summary>
		/// Stop listening
		/// </summary>
		public Task<bool> StopListeningAsync()
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

		private static IProviderLocation GetPosition(Geoposition position)
		{
			var response = new ProviderLocation
			{
				HasAccuracy = true,
				Accuracy = position.Coordinate.Accuracy,
				HasLatitudeLongitude = true,
				Latitude = position.Coordinate.Point.Position.Latitude,
				Longitude = position.Coordinate.Point.Position.Longitude,
				StatusTime = position.Coordinate.Timestamp.UtcDateTime
			};

			if (position.Coordinate.Heading != null)
			{
				response.HasHeading = true;
				response.Heading = position.Coordinate.Heading.Value;
			}

			if (position.Coordinate.Speed != null)
			{
				response.HasSpeed = true;
				response.Speed = position.Coordinate.Speed.Value;
			}

			if (position.Coordinate.AltitudeAccuracy.HasValue)
			{
				response.AltitudeAccuracy = position.Coordinate.AltitudeAccuracy.Value;
			}

			response.Altitude = position.Coordinate.Point.Position.Altitude;
			response.AltitudeReference = position.Coordinate.Point.AltitudeReferenceSystem.ToAltitudeReferenceType();
			response.HasAltitude = response.AltitudeReference != AltitudeReferenceType.Unspecified;
			response.SourceName = position.Coordinate.PositionSource.ToString();

			return response;
		}

		private void OnLocatorPositionChanged(Geolocator sender, PositionChangedEventArgs e)
		{
			OnPositionChanged(new PositionEventArgs(GetPosition(e.Position)));
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

		private void OnPositionChanged(PositionEventArgs e)
		{
			PositionChanged?.Invoke(this, e);
		}

		private void OnPositionError(PositionErrorEventArgs e)
		{
			PositionError?.Invoke(this, e);
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

		#endregion

		#region Events

		/// <summary>
		/// ProviderLocation error event handler
		/// </summary>
		public event EventHandler<PositionEventArgs> PositionChanged;

		/// <summary>
		/// ProviderLocation changed event handler
		/// </summary>
		public event EventHandler<PositionErrorEventArgs> PositionError;

		#endregion
	}
}