#region References

using CoreLocation;
using Foundation;
using Speedy.Devices.Location;
using Speedy.Plugins.Devices.Location;
#if __IOS__ || __TVOS__
using UIKit;

#elif __MACOS__
using AppKit;
#endif

#if __IOS__
#endif

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
    /// <summary>
    /// Implementation for Geolocator
    /// </summary>
    [Preserve(AllMembers = true)]
	public class GeolocatorImplementation : IGeolocator
	{
		private bool deferringUpdates;
		private readonly CLLocationManager manager;
		private bool includeHeading;
		private ProviderLocation lastPosition;
		private ListenerSettings listenerSettings;

		/// <summary>
		/// Constructor for implementation
		/// </summary>
		public GeolocatorImplementation()
		{
			DesiredAccuracy = 10;
			manager = GetManager();
			manager.AuthorizationChanged += OnAuthorizationChanged;
			manager.Failed += OnFailed;

			#if __IOS__
			if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
			{
				manager.LocationsUpdated += OnLocationsUpdated;
			}
			else
			{
				manager.UpdatedLocation += OnUpdatedLocation;
			}
			#elif __MACOS__ || __TVOS__
			manager.LocationsUpdated += OnLocationsUpdated;
			#endif

			#if __IOS__ || __MACOS__
			manager.DeferredUpdatesFinished += OnDeferredUpdatedFinished;
			#endif

			#if __TVOS__
			RequestAuthorization();
			#endif
		}

		private void OnDeferredUpdatedFinished(object sender, NSErrorEventArgs e)
		{
			deferringUpdates = false;
		}

		#if __IOS__
		//private bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable && UIDevice.CurrentDevice.CheckSystemVersion(6, 0);
		private bool CanDeferLocationUpdate => UIDevice.CurrentDevice.CheckSystemVersion(6, 0);
		#elif __MACOS__
        bool CanDeferLocationUpdate => CLLocationManager.DeferredLocationUpdatesAvailable;
		#elif __TVOS__
		bool CanDeferLocationUpdate => false;
		#endif

		#if __IOS__
		private async Task<bool> CheckWhenInUsePermission()
		{
			var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
			if (status != PermissionStatus.Granted)
			{
				Console.WriteLine("Currently does not have Location permissions, requesting permissions");

				status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

				if (status != PermissionStatus.Granted)
				{
					Console.WriteLine("Location permission denied, can not get positions async.");
					return false;
				}
			}

			return true;
		}

		private async Task<bool> CheckAlwaysPermissions()
		{
			var status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
			if (status != PermissionStatus.Granted)
			{
				Console.WriteLine("Currently does not have Location permissions, requesting permissions");

				status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

				if (status != PermissionStatus.Granted)
				{
					Console.WriteLine("Location permission denied, can not get positions async.");
					return false;
				}
			}

			return true;
		}
		#endif

		/// <summary>
		/// ProviderLocation error event handler
		/// </summary>
		public event EventHandler<PositionErrorEventArgs> PositionError;

		/// <summary>
		/// ProviderLocation changed event handler
		/// </summary>
		public event EventHandler<PositionEventArgs> PositionChanged;

		/// <summary>
		/// Desired accuracy in meters
		/// </summary>
		public double DesiredAccuracy { get; set; }

		/// <summary>
		/// Gets if you are listening for location changes
		/// </summary>
		public bool IsListening { get; private set; }

		#if __IOS__ || __MACOS__
		/// <summary>
		/// Gets if device supports heading (course)
		/// </summary>
		public bool SupportsHeading => true;
		#elif __TVOS__
		/// <summary>
		/// Gets if device supports heading
		/// </summary>
		public bool SupportsHeading => false;
		#endif

		/// <summary>
		/// Gets if geolocation is available on device
		/// </summary>
		public bool IsGeolocationAvailable => true; //all iOS devices support Geolocation

		/// <summary>
		/// Gets if geolocation is enabled on device
		/// </summary>
		public bool IsGeolocationEnabled => CLLocationManager.LocationServicesEnabled;

		/// <summary>
		/// Gets the last known and most accurate location.
		/// This is usually cached and best to display first before querying for full position.
		/// </summary>
		/// <returns> Best and most recent location or null if none found </returns>
		public async Task<IProviderLocation> GetLastKnownLocationAsync()
		{
			#if __IOS__
			var hasPermission = await CheckWhenInUsePermission();
			if (!hasPermission)
			{
				throw new GeolocationException(GeolocationError.Unauthorized);
			}
			#endif

			var m = GetManager();
			var newLocation = m?.Location;

			if (newLocation == null)
			{
				return null;
			}

			var position = new ProviderLocation
			{
				HasAccuracy = true,
				Accuracy = newLocation.HorizontalAccuracy,
				HasAltitude = newLocation.Altitude > -1,
				Altitude = newLocation.EllipsoidalAltitude,
				AltitudeAccuracy = newLocation.VerticalAccuracy,
				AltitudeReference = AltitudeReferenceType.Ellipsoid,
				HasLatitudeLongitude = newLocation.HorizontalAccuracy > -1,
				Latitude = newLocation.Coordinate.Latitude,
				Longitude = newLocation.Coordinate.Longitude,
				SourceName = GetSourceName(newLocation.SourceInformation)
			};

			#if !__TVOS__
			position.HasSpeed = newLocation.Speed > -1;
			position.Speed = newLocation.Speed;
			#endif

			try
			{
				position.StatusTime = newLocation.Timestamp.ToDateTime().ToUniversalTime();
			}
			catch (Exception)
			{
				position.StatusTime = TimeService.UtcNow;
			}

			return position;
		}

		private string GetSourceName(CLLocationSourceInformation information)
		{
			if (information == null)
			{
				return "unknown";
			}

			if (information.IsSimulatedBySoftware)
			{
				return "simulated";
			}

			if (information.IsProducedByAccessory)
			{
				return "hardware";
			}

			return "unknown";
		}

		/// <summary>
		/// Gets position async with specified parameters
		/// </summary>
		/// <param name="timeout"> Timeout to wait, Default Infinite </param>
		/// <param name="cancelToken"> Cancellation token </param>
		/// <param name="includeHeading"> If you would like to include heading </param>
		/// <returns> ProviderLocation </returns>
		public async Task<IProviderLocation> GetPositionAsync(TimeSpan? timeout, CancellationToken? cancelToken = null, bool includeHeading = false)
		{
			#if __IOS__
			var hasPermission = await CheckWhenInUsePermission();
			if (!hasPermission)
			{
				throw new GeolocationException(GeolocationError.Unauthorized);
			}
			#endif

			var timeoutMilliseconds = timeout.HasValue ? (int) timeout.Value.TotalMilliseconds : Timeout.Infinite;

			if ((timeoutMilliseconds <= 0) && (timeoutMilliseconds != Timeout.Infinite))
			{
				throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or Timeout.Infinite");
			}

			if (!cancelToken.HasValue)
			{
				cancelToken = CancellationToken.None;
			}

			TaskCompletionSource<IProviderLocation> tcs;
			if (!IsListening)
			{
				var m = GetManager();
				m.DesiredAccuracy = DesiredAccuracy;

				tcs = new TaskCompletionSource<IProviderLocation>(m);
				var singleListener = new GeolocationSingleUpdateDelegate(m, DesiredAccuracy, includeHeading, timeoutMilliseconds, cancelToken.Value);
				m.Delegate = singleListener;

				#if __IOS__ || __MACOS__
				m.StartUpdatingLocation();
				#elif __TVOS__
				m.RequestLocation();
				#endif

				return await singleListener.Task;
			}

			tcs = new TaskCompletionSource<IProviderLocation>();
			if (lastPosition == null)
			{
				if (cancelToken != CancellationToken.None)
				{
					cancelToken.Value.Register(() => tcs.TrySetCanceled());
				}

				EventHandler<PositionErrorEventArgs> gotError = null;
				gotError = (s, e) =>
				{
					tcs.TrySetException(new GeolocationException(e.Error));
					PositionError -= gotError;
				};

				PositionError += gotError;

				EventHandler<PositionEventArgs> gotPosition = null;
				gotPosition = (s, e) =>
				{
					tcs.TrySetResult(e.ProviderLocation);
					PositionChanged -= gotPosition;
				};

				PositionChanged += gotPosition;
			}
			else
			{
				tcs.SetResult(lastPosition);
			}

			return await tcs.Task;
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

			using (var geocoder = new CLGeocoder())
			{
				var positionList = await geocoder.GeocodeAddressAsync(address);
				return positionList.Select(p => new ProviderLocation
				{
					HasLatitudeLongitude = true,
					Latitude = p.Location.Coordinate.Latitude,
					Longitude = p.Location.Coordinate.Longitude
				});
			}
		}

		/// <summary>
		/// Retrieve addresses for position.
		/// </summary>
		/// <param name="position"> Desired position (latitude and longitude) </param>
		/// <returns> Addresses of the desired position </returns>
		public async Task<IEnumerable<Address>> GetAddressesForPositionAsync(IGeoLocation position, string mapKey = null)
		{
			if (position == null)
			{
				throw new ArgumentNullException(nameof(position));
			}

			using (var geocoder = new CLGeocoder())
			{
				var addressList = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(position.Latitude, position.Longitude));

				return addressList?.ToAddresses() ?? null;
			}
		}

		/// <summary>
		/// Start listening for changes
		/// </summary>
		/// <param name="minimumTime"> Time </param>
		/// <param name="minimumDistance"> Distance </param>
		/// <param name="includeHeading"> Include heading or not </param>
		/// <param name="listenerSettings"> Optional settings (iOS only) </param>
		public async Task<bool> StartListeningAsync(TimeSpan minimumTime, double minimumDistance, bool includeHeading = false, ListenerSettings listenerSettings = null)
		{
			if (minimumDistance < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumDistance));
			}

			if (IsListening)
			{
				throw new InvalidOperationException("Already listening");
			}

			// if no settings were passed in, instantiate the default settings. need to check this and create default settings since
			// previous calls to StartListeningAsync might have already configured the location manager in a non-default way that the
			// caller of this method might not be expecting. the caller should expect the defaults if they pass no settings.
			if (listenerSettings == null)
			{
				listenerSettings = new ListenerSettings();
			}

			#if __IOS__
			var hasPermission = false;
			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				if (listenerSettings.RequireLocationAlwaysPermission)
				{
					hasPermission = await CheckAlwaysPermissions();
				}
				else
				{
					hasPermission = await CheckWhenInUsePermission();
				}
			}

			if (!hasPermission)
			{
				throw new GeolocationException(GeolocationError.Unauthorized);
			}
			#endif

			#if __IOS__ || __MACOS__
			this.includeHeading = includeHeading;
			#endif

			// keep reference to settings so that we can stop the listener appropriately later
			this.listenerSettings = listenerSettings;

			var desiredAccuracy = DesiredAccuracy;

			// set background flag
			#if __IOS__
			if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
			{
				manager.ShowsBackgroundLocationIndicator = listenerSettings.ShowsBackgroundLocationIndicator;
			}

			if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
			{
				manager.AllowsBackgroundLocationUpdates = listenerSettings.AllowBackgroundUpdates;
			}

			// configure location update pausing
			if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
			{
				manager.PausesLocationUpdatesAutomatically = listenerSettings.PauseLocationUpdatesAutomatically;

				switch (listenerSettings.ActivityType)
				{
					case ActivityType.AutomotiveNavigation:
						manager.ActivityType = CLActivityType.AutomotiveNavigation;
						break;
					case ActivityType.Fitness:
						manager.ActivityType = CLActivityType.Fitness;
						break;
					case ActivityType.OtherNavigation:
						manager.ActivityType = CLActivityType.OtherNavigation;
						break;
					default:
						manager.ActivityType = CLActivityType.Other;
						break;
				}
			}
			#endif

			// to use deferral, CLLocationManager.DistanceFilter must be set to CLLocationDistance.None, and CLLocationManager.DesiredAccuracy must be 
			// either CLLocation.AccuracyBest or CLLocation.AccuracyBestForNavigation. deferral only available on iOS 6.0 and above.
			if (CanDeferLocationUpdate && listenerSettings.DeferLocationUpdates)
			{
				minimumDistance = CLLocationDistance.FilterNone;
				desiredAccuracy = CLLocation.AccuracyBest;
			}

			IsListening = true;
			manager.DesiredAccuracy = desiredAccuracy;
			manager.DistanceFilter = minimumDistance;

			#if __IOS__ || __MACOS__
			if (listenerSettings.ListenForSignificantChanges)
			{
				manager.StartMonitoringSignificantLocationChanges();
			}
			else
			{
				manager.StartUpdatingLocation();
			}
			#elif __TVOS__
			//not supported
			#endif

			return true;
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

			IsListening = false;

			#if __IOS__ && !__MACCATALYST__
			// it looks like deferred location updates can apply to the standard service or significant change service. disallow deferral in either case.
			if ((listenerSettings?.DeferLocationUpdates ?? false) && CanDeferLocationUpdate)
			{
				#pragma warning disable CA1416 // Validate platform compatibility
				manager.DisallowDeferredLocationUpdates();
				#pragma warning restore CA1416 // Validate platform compatibility
			}
			#endif

			#if __IOS__ || __MACOS__
			if (listenerSettings?.ListenForSignificantChanges ?? false)
			{
				manager.StopMonitoringSignificantLocationChanges();
			}
			else
			{
				manager.StopUpdatingLocation();
			}
			#endif

			listenerSettings = null;
			lastPosition = null;

			return Task.FromResult(true);
		}

		private CLLocationManager GetManager()
		{
			CLLocationManager m = null;
			new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
			return m;
		}

		private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
		{
			if (e.Locations.Any())
			{
				UpdatePosition(e.Locations.Last());
			}

			// defer future location updates if requested
			if ((listenerSettings?.DeferLocationUpdates ?? false) && !deferringUpdates && CanDeferLocationUpdate)
			{
				#if __IOS__ && !__MACCATALYST__
				#pragma warning disable CA1416 // Validate platform compatibility
				manager.AllowDeferredLocationUpdatesUntil(listenerSettings.DeferralDistanceMeters == null ? CLLocationDistance.MaxDistance : listenerSettings.DeferralDistanceMeters.GetValueOrDefault(),
					listenerSettings.DeferralTime == null ? CLLocationManager.MaxTimeInterval : listenerSettings.DeferralTime.GetValueOrDefault().TotalSeconds);
				#pragma warning restore CA1416 // Validate platform compatibility
				#endif

				deferringUpdates = true;
			}
		}

		#if __IOS__ || __MACOS__
		private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e)
		{
			UpdatePosition(e.NewLocation);
		}
		#endif

		private void UpdatePosition(CLLocation location)
		{
			var p = lastPosition ?? new ProviderLocation();
			p.HasAccuracy = true;

			if (location.HorizontalAccuracy > -1)
			{
				p.Accuracy = location.HorizontalAccuracy;
				p.HasLatitudeLongitude = true;
				p.Latitude = location.Coordinate.Latitude;
				p.Longitude = location.Coordinate.Longitude;
			}

			if (location.VerticalAccuracy > -1)
			{
				p.HasAltitude = true;
				p.Altitude = location.EllipsoidalAltitude;
				p.AltitudeAccuracy = location.VerticalAccuracy;
				p.AltitudeReference = AltitudeReferenceType.Ellipsoid;
			}

			#if __IOS__ || __MACOS__
			if (location.Speed > -1)
			{
				p.HasSpeed = true;
				p.Speed = location.Speed;
			}

			if (includeHeading && (location.Course > -1))
			{
				p.HasHeading = true;
				p.Heading = location.Course;
			}
			#endif

			try
			{
				p.StatusTime = location.Timestamp.ToDateTime().ToUniversalTime();
			}
			catch (Exception)
			{
				p.StatusTime = TimeService.UtcNow;
			}

			lastPosition = p;

			OnPositionChanged(new PositionEventArgs(p));

			location.Dispose();
		}

		private void OnPositionChanged(PositionEventArgs e)
		{
			PositionChanged?.Invoke(this, e);
		}

		private async void OnPositionError(PositionErrorEventArgs e)
		{
			await StopListeningAsync();
			PositionError?.Invoke(this, e);
		}

		private void OnFailed(object sender, NSErrorEventArgs e)
		{
			if ((CLError) (int) e.Error.Code == CLError.Network)
			{
				OnPositionError(new PositionErrorEventArgs(GeolocationError.PositionUnavailable));
			}
		}

		private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
		{
			if ((e.Status == CLAuthorizationStatus.Denied) || (e.Status == CLAuthorizationStatus.Restricted))
			{
				OnPositionError(new PositionErrorEventArgs(GeolocationError.Unauthorized));
			}
		}
	}
}