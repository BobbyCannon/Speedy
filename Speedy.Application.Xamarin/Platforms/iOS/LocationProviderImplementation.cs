#region References

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Speedy.Devices.Location;
using Xamarin.Essentials;
using UIKit;

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
	#region Fields

	private readonly CLLocationManager _manager;

	#endregion

	#region Constructors

	/// <summary>
	/// Constructor for implementation
	/// </summary>
	public LocationProviderImplementation(IDispatcher dispatcher) : base(dispatcher)
	{
		_manager = GetManager();
		_manager.AuthorizationChanged += OnAuthorizationChanged;
		_manager.Failed += OnFailed;

		if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
		{
			_manager.LocationsUpdated += OnLocationsUpdated;
		}
		else
		{
			_manager.UpdatedLocation += OnUpdatedLocation;
		}

		LastReadLocation.ProviderName = "Xamarin iOS";
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsLocationAvailable => true;

	/// <inheritdoc />
	public override bool IsLocationEnabled => CLLocationManager.LocationServicesEnabled;

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override async Task<T> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
	{
		var hasPermission = await CheckWhenInUsePermission();
		if (!hasPermission)
		{
			throw new LocationProviderException(LocationProviderError.Unauthorized);
		}

		var timeoutMilliseconds = timeout.HasValue ? (int) timeout.Value.TotalMilliseconds : Timeout.Infinite;

		if ((timeoutMilliseconds <= 0) && (timeoutMilliseconds != Timeout.Infinite))
		{
			throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive or Timeout.Infinite");
		}

		cancelToken ??= CancellationToken.None;

		if (!IsListening)
		{
			var m = GetManager();
			m.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy;

			var singleListener = new LocationProviderSingleUpdateDelegate<T>(m, m.DesiredAccuracy, true, timeoutMilliseconds, cancelToken.Value);
			m.Delegate = singleListener;
			m.StartUpdatingLocation();
			return await singleListener.Task;
		}

		var tcs = new TaskCompletionSource<T>();
		tcs.SetResult(LastReadLocation);
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

		if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
		{
			if (LocationProviderSettings.RequireLocationAlwaysPermission)
			{
				HasPermission = await CheckAlwaysPermissions();
			}
			else
			{
				HasPermission = await CheckWhenInUsePermission();
			}
		}

		if (!HasPermission)
		{
			throw new LocationProviderException(LocationProviderError.Unauthorized);
		}

		IsListening = true;

		_manager.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy;
		_manager.DistanceFilter = LocationProviderSettings.MinimumDistance;
		_manager.StartUpdatingLocation();
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

		_manager.StopUpdatingLocation();

		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override async void OnPositionError(LocationProviderError e)
	{
		await StopListeningAsync();
		base.OnPositionError(e);
	}

	private CLLocationManager GetManager()
	{
		CLLocationManager m = null;
		new NSObject().InvokeOnMainThread(() => m = new CLLocationManager());
		return m;
	}

	private string GetSourceName(CLLocationSourceInformation information)
	{
		if (information == null)
		{
			return "unknown";
		}

		if (information.IsSimulatedBySoftware)
		{
			return "simulated by software";
		}

		if (information.IsProducedByAccessory)
		{
			return "produced by accessory";
		}

		return "iPhone";
	}

	private void OnAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
	{
		if ((e.Status == CLAuthorizationStatus.Denied)
			|| (e.Status == CLAuthorizationStatus.Restricted))
		{
			OnPositionError(LocationProviderError.Unauthorized);
		}
	}

	private void OnFailed(object sender, NSErrorEventArgs e)
	{
		if ((CLError) (int) e.Error.Code == CLError.Network)
		{
			OnPositionError(LocationProviderError.PositionUnavailable);
		}
	}

	private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
	{
		if (e.Locations.Any())
		{
			UpdatePosition(e.Locations.Last());
		}
	}

	private void OnUpdatedLocation(object sender, CLLocationUpdatedEventArgs e)
	{
		UpdatePosition(e.NewLocation);
	}

	private void UpdatePosition(CLLocation location)
	{
		if (location.HorizontalAccuracy > -1)
		{
			LastReadLocation.Latitude = location.Coordinate.Latitude;
			LastReadLocation.Longitude = location.Coordinate.Longitude;
			LastReadLocation.HorizontalAccuracy = location.HorizontalAccuracy;
			LastReadLocation.HorizontalAccuracyReference = AccuracyReferenceType.Meters;
		}
		else
		{
			LastReadLocation.HorizontalAccuracyReference = AccuracyReferenceType.Unspecified;
		}

		if (location.VerticalAccuracy > -1)
		{
			LastReadLocation.Altitude = location.EllipsoidalAltitude;
			LastReadLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
			LastReadLocation.VerticalAccuracy = location.VerticalAccuracy;
			LastReadLocation.VerticalAccuracyReference = AccuracyReferenceType.Meters;
		}
		else
		{
			LastReadLocation.VerticalAccuracyReference = AccuracyReferenceType.Unspecified;
		}

		if (location.Speed > -1)
		{
			LastReadLocation.HasSpeed = true;
			LastReadLocation.Speed = location.Speed;
		}
		else
		{
			LastReadLocation.HasSpeed = false;
		}

		if (location.Course > -1)
		{
			LastReadLocation.HasHeading = true;
			LastReadLocation.Heading = location.Course;
		}

		var sourceName = GetSourceName(location.SourceInformation);
		LastReadLocation.HorizontalSourceName = sourceName;
		LastReadLocation.VerticalSourceName = sourceName;

		try
		{
			var statusTime = location.Timestamp.ToDateTime().ToUniversalTime();
			LastReadLocation.HorizontalStatusTime = statusTime;
			LastReadLocation.VerticalStatusTime = statusTime;
		}
		catch (Exception)
		{
			var statusTime = TimeService.UtcNow;
			LastReadLocation.HorizontalStatusTime = statusTime;
			LastReadLocation.VerticalStatusTime = statusTime;
		}

		OnPositionChanged(LastReadLocation);

		location.Dispose();
	}

	#endregion

	#if __IOS__
	private async Task<bool> CheckWhenInUsePermission()
	{
		var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		return status == PermissionStatus.Granted;
	}

	private async Task<bool> CheckAlwaysPermissions()
	{
		var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
		return status == PermissionStatus.Granted;
	}
	#endif
}