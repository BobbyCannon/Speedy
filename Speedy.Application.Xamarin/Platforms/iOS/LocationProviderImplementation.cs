#region References

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Speedy.Data.Location;
using UIKit;
using Xamarin.Essentials;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

/// <summary>
/// Implementation for LocationProvider
/// </summary>
[Preserve(AllMembers = true)]
public class LocationProviderImplementation<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	: LocationProvider<TLocation, THorizontal, TVertical, TLocationProviderSettings>
	where TLocation : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
	where TLocationProviderSettings : ILocationProviderSettings, IBindable, new()
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
		_manager.AllowsBackgroundLocationUpdates = LocationProviderSettings.AllowBackgroundUpdates;
		_manager.PausesLocationUpdatesAutomatically = LocationProviderSettings.AllowPausingOfUpdates;

		if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
		{
			_manager.LocationsUpdated += OnLocationsUpdated;
		}
		else
		{
			_manager.UpdatedLocation += OnUpdatedLocation;
		}

		CurrentValue.HorizontalLocation.ProviderName = ProviderName;
		CurrentValue.VerticalLocation.ProviderName = ProviderName;
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsLocationAvailable => true;

	/// <inheritdoc />
	public override bool IsLocationEnabled => CLLocationManager.LocationServicesEnabled;

	/// <inheritdoc />
	public sealed override string ProviderName => "Xamarin iOS";

	/// <summary>
	/// True if the location provider has permission to be accessed.
	/// </summary>
	protected bool HasPermission { get; private set; }

	#endregion

	#region Methods

	/// <summary>
	/// Gets position async with specified parameters
	/// </summary>
	/// <param name="timeout"> Timeout to wait, Default Infinite </param>
	/// <param name="cancelToken"> Cancellation token </param>
	/// <returns> ProviderLocation </returns>
	public override async Task<TLocation> GetCurrentLocationAsync(TimeSpan? timeout = null, CancellationToken? cancelToken = null)
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

		if (!IsMonitoring)
		{
			var m = GetManager();
			m.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy;

			var singleListener = new LocationProviderSingleUpdateDelegate<TLocation, THorizontal, TVertical>(m, m.DesiredAccuracy, timeoutMilliseconds, cancelToken.Value);
			m.Delegate = singleListener;
			m.StartUpdatingLocation();
			return await singleListener.Task;
		}

		var tcs = new TaskCompletionSource<TLocation>();
		tcs.SetResult(CurrentValue);
		return await tcs.Task;
	}

	/// <summary>
	/// Start listening for changes
	/// </summary>
	public override async Task StartMonitoringAsync()
	{
		if (IsMonitoring)
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

		_manager.DesiredAccuracy = LocationProviderSettings.DesiredAccuracy;
		_manager.DistanceFilter = LocationProviderSettings.MinimumDistance;
		_manager.StartUpdatingLocation();

		IsMonitoring = true;
	}

	/// <summary>
	/// Stop listening
	/// </summary>
	public override Task StopMonitoringAsync()
	{
		if (!IsMonitoring)
		{
			return Task.CompletedTask;
		}

		_manager.StopUpdatingLocation();

		IsMonitoring = false;
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	protected override async void OnLocationProviderError(LocationProviderError e)
	{
		await StopMonitoringAsync();
		base.OnLocationProviderError(e);
	}

	private async Task<bool> CheckAlwaysPermissions()
	{
		var status = await Permissions.RequestAsync<Permissions.LocationAlways>();
		return status == PermissionStatus.Granted;
	}

	private async Task<bool> CheckWhenInUsePermission()
	{
		var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
		return status == PermissionStatus.Granted;
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
			OnLocationProviderError(LocationProviderError.Unauthorized);
		}
	}

	private void OnFailed(object sender, NSErrorEventArgs e)
	{
		if ((CLError) (int) e.Error.Code == CLError.Network)
		{
			OnLocationProviderError(LocationProviderError.LocationUnavailable);
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
			CurrentValue.HorizontalLocation.Latitude = location.Coordinate.Latitude;
			CurrentValue.HorizontalLocation.Longitude = location.Coordinate.Longitude;
			CurrentValue.HorizontalLocation.Accuracy = location.HorizontalAccuracy;
			CurrentValue.HorizontalLocation.AccuracyReference = AccuracyReferenceType.Meters;
			CurrentValue.HorizontalLocation.HasValue = true;
		}
		else
		{
			CurrentValue.HorizontalLocation.Latitude = 0;
			CurrentValue.HorizontalLocation.Longitude = 0;
			CurrentValue.HorizontalLocation.Accuracy = 0;
			CurrentValue.HorizontalLocation.AccuracyReference = AccuracyReferenceType.Unspecified;
			CurrentValue.HorizontalLocation.HasValue = false;
		}

		if (location.VerticalAccuracy > -1)
		{
            if (UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
            {
                CurrentValue.VerticalLocation.Altitude = location.EllipsoidalAltitude;
                CurrentValue.VerticalLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
            }
            else
            {
                CurrentValue.VerticalLocation.Altitude = location.Altitude;
                CurrentValue.VerticalLocation.AltitudeReference = AltitudeReferenceType.Geoid;
            }
            CurrentValue.VerticalLocation.Accuracy = location.VerticalAccuracy;
			CurrentValue.VerticalLocation.AccuracyReference = AccuracyReferenceType.Meters;
			CurrentValue.VerticalLocation.HasValue = true;
		}
		else
		{
			CurrentValue.VerticalLocation.Altitude = 0;
			CurrentValue.VerticalLocation.AltitudeReference = AltitudeReferenceType.Unspecified;
			CurrentValue.VerticalLocation.Accuracy = 0;
			CurrentValue.VerticalLocation.AccuracyReference = AccuracyReferenceType.Unspecified;
			CurrentValue.VerticalLocation.HasValue = false;
		}

		if (location.Speed > -1)
		{
			CurrentValue.HorizontalLocation.HasSpeed = true;
			CurrentValue.HorizontalLocation.Speed = location.Speed;
		}
		else
		{
			CurrentValue.HorizontalLocation.HasSpeed = false;
			CurrentValue.HorizontalLocation.Speed = 0;
		}

		if (location.Course > -1)
		{
			CurrentValue.HorizontalLocation.HasHeading = true;
			CurrentValue.HorizontalLocation.Heading = location.Course;
		}
		else
		{
			CurrentValue.HorizontalLocation.HasHeading = false;
			CurrentValue.HorizontalLocation.Heading = 0;
		}

		if(UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
		{
			var sourceName = GetSourceName(location.SourceInformation);
			CurrentValue.HorizontalLocation.SourceName = sourceName;
			CurrentValue.VerticalLocation.SourceName = sourceName;
		}
        else
        {
            CurrentValue.HorizontalLocation.SourceName = "iOS < 15";
            CurrentValue.VerticalLocation.SourceName = "iOS < 15";
        }

        try
		{
			var statusTime = location.Timestamp.ToDateTime().ToUniversalTime();
			CurrentValue.HorizontalLocation.StatusTime = statusTime;
			CurrentValue.VerticalLocation.StatusTime = statusTime;
		}
		catch (Exception)
		{
			var statusTime = TimeService.UtcNow;
			CurrentValue.HorizontalLocation.StatusTime = statusTime;
			CurrentValue.VerticalLocation.StatusTime = statusTime;
		}

		OnUpdated(CurrentValue);

		location.Dispose();
	}

	#endregion
}