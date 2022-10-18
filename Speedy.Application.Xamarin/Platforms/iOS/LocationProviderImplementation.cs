#region References

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Speedy.Devices.Location;
using Speedy.Serialization;
using Xamarin.Essentials;

#if __IOS__ || __TVOS__
using UIKit;

#elif __MACOS__
using AppKit;
#endif

#if __IOS__
#endif

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
[Preserve(AllMembers = true)]
public class LocationProviderImplementation<T, T2> : LocationProvider<T, T2>
	where T : class, ILocation, new()
	where T2 : LocationProviderSettings, new()
{
	private bool _deferringUpdates;
	private readonly CLLocationManager _manager;
	private T _lastPosition;
	private LocationProviderSettings _locationProviderSettings;

	/// <summary>
	/// Constructor for implementation
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_manager = GetManager();
		_manager.AuthorizationChanged += OnAuthorizationChanged;
		_manager.Failed += OnFailed;

		#if __IOS__
		if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
		{
			_manager.LocationsUpdated += OnLocationsUpdated;
		}
		else
		{
			_manager.UpdatedLocation += OnUpdatedLocation;
		}
		#elif __MACOS__ || __TVOS__
		manager.LocationsUpdated += OnLocationsUpdated;
		#endif

		#if __IOS__ || __MACOS__
		_manager.DeferredUpdatesFinished += OnDeferredUpdatedFinished;
		#endif

		#if __TVOS__
		RequestAuthorization();
		#endif
	}

	private void OnDeferredUpdatedFinished(object sender, NSErrorEventArgs e)
	{
		_deferringUpdates = false;
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
	/// Gets if geolocation is available on device
	/// </summary>
	public override bool IsLocationAvailable => true; //all iOS devices support Geolocation

	/// <summary>
	/// Gets if geolocation is enabled on device
	/// </summary>
	public override bool IsLocationEnabled => CLLocationManager.LocationServicesEnabled;

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
	/// <returns> ProviderLocation </returns>
	public override async Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		#if __IOS__
		var hasPermission = await CheckWhenInUsePermission();
		if (!hasPermission)
		{
			throw new LocationProviderException(LocationProviderError.Unauthorized);
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

		TaskCompletionSource<T> tcs;
		if (!IsListening)
		{
			var m = GetManager();
			m.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy;

			tcs = new TaskCompletionSource<T>(m);
			var singleListener = new LocationProviderSingleUpdateDelegate<T>(m, m.DesiredAccuracy, true, timeoutMilliseconds, cancelToken.Value);
			m.Delegate = singleListener;

			#if __IOS__ || __MACOS__
			m.StartUpdatingLocation();
			#elif __TVOS__
			m.RequestLocation();
			#endif

			return await singleListener.Task;
		}

		tcs = new TaskCompletionSource<T>();
		if (_lastPosition == null)
		{
			if (cancelToken != CancellationToken.None)
			{
				cancelToken.Value.Register(() => tcs.TrySetCanceled());
			}

			EventHandler<LocationProviderError> gotError = null;
			gotError = (s, e) =>
			{
				tcs.TrySetException(new LocationProviderException(e));
				PositionError -= gotError;
			};

			PositionError += gotError;

			EventHandler<T> gotPosition = null;
			gotPosition = (s, e) =>
			{
				tcs.TrySetResult(e);
				PositionChanged -= gotPosition;
			};

			PositionChanged += gotPosition;
		}
		else
		{
			tcs.SetResult(_lastPosition);
		}

		return await tcs.Task;
	}

	/// <summary>
	/// Start listening for changes
	/// </summary>
	public override async Task StartListeningAsync()
	{
		if (IsListening)
		{
			return;
		}

		#if __IOS__
		var hasPermission = false;
		if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
		{
			if (_locationProviderSettings.RequireLocationAlwaysPermission)
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
			throw new LocationProviderException(LocationProviderError.Unauthorized);
		}
		#endif

		// keep reference to settings so that we can stop the listener appropriately later
		_locationProviderSettings = LocationProviderSettings.DeepClone();

		var desiredAccuracy = _locationProviderSettings.DesiredAccuracy;

		// set background flag
		#if __IOS__
		//if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
		//{
		//	manager.ShowsBackgroundLocationIndicator = listenerSettings.ShowsBackgroundLocationIndicator;
		//}

		//if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
		//{
		//	manager.AllowsBackgroundLocationUpdates = listenerSettings.AllowBackgroundUpdates;
		//}

		//// configure location update pausing
		//if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
		//{
		//	manager.PausesLocationUpdatesAutomatically = listenerSettings.PauseLocationUpdatesAutomatically;

		//	//switch (listenerSettings.ActivityType)
		//	//{
		//	//	case ActivityType.AutomotiveNavigation:
		//	//		manager.ActivityType = CLActivityType.AutomotiveNavigation;
		//	//		break;
		//	//	case ActivityType.Fitness:
		//	//		manager.ActivityType = CLActivityType.Fitness;
		//	//		break;
		//	//	case ActivityType.OtherNavigation:
		//	//		manager.ActivityType = CLActivityType.OtherNavigation;
		//	//		break;
		//	//	default:
		//	//		manager.ActivityType = CLActivityType.Other;
		//	//		break;
		//	//}
		//}
		#endif

		// to use deferral, CLLocationManager.DistanceFilter must be set to CLLocationDistance.None, and CLLocationManager.DesiredAccuracy must be 
		// either CLLocation.AccuracyBest or CLLocation.AccuracyBestForNavigation. deferral only available on iOS 6.0 and above.
		//if (CanDeferLocationUpdate && listenerSettings.DeferLocationUpdates)
		//{
		//	minimumDistance = CLLocationDistance.FilterNone;
		//	desiredAccuracy = CLLocation.AccuracyBest;
		//}

		IsListening = true;
		_manager.DesiredAccuracy = desiredAccuracy;
		_manager.DistanceFilter = LocationProviderSettings.MinimumDistance;

		#if __IOS__ || __MACOS__
		//if (listenerSettings.ListenForSignificantChanges)
		//{
		//	manager.StartMonitoringSignificantLocationChanges();
		//}
		//else
		{
			_manager.StartUpdatingLocation();
		}
		#elif __TVOS__
		//not supported
		#endif
	}

	/// <summary>
	/// Stop listening
	/// </summary>
	public override Task StopListeningAsync()
	{
		if (!IsListening)
		{
			return Task.CompletedTask;
		}

		IsListening = false;

		#if __IOS__ && !__MACCATALYST__
		// it looks like deferred location updates can apply to the standard service or significant change service. disallow deferral in either case.
		//if ((listenerSettings?.DeferLocationUpdates ?? false) && CanDeferLocationUpdate)
		//{
		//	#pragma warning disable CA1416 // Validate platform compatibility
		//	manager.DisallowDeferredLocationUpdates();
		//	#pragma warning restore CA1416 // Validate platform compatibility
		//}
		#endif

		#if __IOS__ || __MACOS__
		//if (listenerSettings?.ListenForSignificantChanges ?? false)
		//{
		//	manager.StopMonitoringSignificantLocationChanges();
		//}
		//else
		{
			_manager.StopUpdatingLocation();
		}
		#endif

		_locationProviderSettings = null;
		_lastPosition = null;

		return Task.CompletedTask;
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
		//if ((listenerSettings?.DeferLocationUpdates ?? false) && !deferringUpdates && CanDeferLocationUpdate)
		//{
		//	#if __IOS__ && !__MACCATALYST__
		//	#pragma warning disable CA1416 // Validate platform compatibility
		//	manager.AllowDeferredLocationUpdatesUntil(listenerSettings.DeferralDistanceMeters == null ? CLLocationDistance.MaxDistance : listenerSettings.DeferralDistanceMeters.GetValueOrDefault(),
		//		listenerSettings.DeferralTime == null ? CLLocationManager.MaxTimeInterval : listenerSettings.DeferralTime.GetValueOrDefault().TotalSeconds);
		//	#pragma warning restore CA1416 // Validate platform compatibility
		//	#endif

		//	deferringUpdates = true;
		//}
	}

	#if __IOS__ || __MACOS__
	private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e)
	{
		UpdatePosition(e.NewLocation);
	}
	#endif

	private void UpdatePosition(CLLocation location)
	{
		var p = _lastPosition ?? new T();

		if (location.HorizontalAccuracy > -1)
		{
			p.Accuracy = location.HorizontalAccuracy;
			p.AccuracyReference = AccuracyReferenceType.Meters;
			p.Latitude = location.Coordinate.Latitude;
			p.Longitude = location.Coordinate.Longitude;
		}
		else
		{
			p.AccuracyReference = AccuracyReferenceType.Unknown;
		}

		if (location.VerticalAccuracy > -1)
		{
			p.Altitude = location.EllipsoidalAltitude;
			p.AltitudeAccuracy = location.VerticalAccuracy;
			p.AltitudeAccuracyReference = AccuracyReferenceType.Meters;
			p.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		}
		else
		{
			p.AltitudeAccuracyReference = AccuracyReferenceType.Unknown;
		}

		#if __IOS__ || __MACOS__
		if (location.Speed > -1)
		{
			p.HasSpeed = true;
			p.Speed = location.Speed;
		}
		else
		{
			p.HasSpeed = false;
		}

		if (location.Course > -1)
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

		_lastPosition = p;

		OnPositionChanged(p);

		location.Dispose();
	}

	protected override async void OnPositionError(LocationProviderError e)
	{
		await StopListeningAsync();
		base.OnPositionError(e);
	}

	private void OnFailed(object sender, NSErrorEventArgs e)
	{
		if ((CLError) (int) e.Error.Code == CLError.Network)
		{
			OnPositionError(LocationProviderError.PositionUnavailable);
		}
	}

	private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
	{
		if ((e.Status == CLAuthorizationStatus.Denied) || (e.Status == CLAuthorizationStatus.Restricted))
		{
			OnPositionError(LocationProviderError.Unauthorized);
		}
	}
}