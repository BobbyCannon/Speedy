#region References

using CoreLocation;
using Foundation;
using Speedy.Devices.Location;
using Location = Speedy.Devices.Location.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Maui;

[Preserve(AllMembers = true)]
internal class LocationProviderSingleUpdateDelegate<T> : CLLocationManagerDelegate
	where T : class, ILocation, new()
{
	private bool haveLocation;
	private readonly T position = new T();
	private readonly double desiredAccuracy;
	private readonly bool includeHeading;
	private readonly TaskCompletionSource<T> tcs;
	private readonly CLLocationManager manager;

	public LocationProviderSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, bool includeHeading, int timeout, CancellationToken cancelToken)
	{
		this.manager = manager;
		tcs = new TaskCompletionSource<T>(manager);
		this.desiredAccuracy = desiredAccuracy;
		this.includeHeading = includeHeading;

		if (timeout != Timeout.Infinite)
		{
			Timer t = null;
			t = new Timer(s =>
			{
				if (haveLocation)
				{
					var response = new T();
					response.UpdateWith(position);
					tcs.TrySetResult(response);
				}
				else
				{
					tcs.TrySetCanceled();
				}

				StopListening();
				t.Dispose();
			}, null, timeout, 0);
		}

		#if __IOS__
		manager.ShouldDisplayHeadingCalibration += locationManager =>
		{
			locationManager.DismissHeadingCalibrationDisplay();
			return false;
		};
		#endif

		cancelToken.Register(() =>
		{
			StopListening();
			tcs.TrySetCanceled();
		});
	}

	public Task<T> Task => tcs?.Task;

	public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
	{
		// If user has services disabled, we're just going to throw an exception for consistency.
		if ((status == CLAuthorizationStatus.Denied) || (status == CLAuthorizationStatus.Restricted))
		{
			StopListening();
			tcs.TrySetException(new LocationProviderException(LocationProviderError.Unauthorized));
		}
	}

	public override void Failed(CLLocationManager manager, NSError error)
	{
		switch ((CLError) (int) error.Code)
		{
			case CLError.Network:
				StopListening();
				tcs.SetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
				break;
			case CLError.LocationUnknown:
				StopListening();
				tcs.TrySetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
				break;
		}
	}

	#if __IOS__
	public override bool ShouldDisplayHeadingCalibration(CLLocationManager locationManager)
	{
		locationManager.DismissHeadingCalibrationDisplay();
		return false;
	}
	#endif

	#if __TVOS__
		public override void LocationsUpdated(CLLocationManager manager, CLLocation[] locations)
		{
			var newLocation = locations.FirstOrDefault();
			if (newLocation == null)
				return;

	#else
	public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
	{
		#endif
		if (newLocation.HorizontalAccuracy < 0)
		{
			return;
		}

		if (haveLocation && (newLocation.HorizontalAccuracy > position.Accuracy))
		{
			return;
		}

		position.Accuracy = newLocation.HorizontalAccuracy;
		position.AccuracyReference = newLocation.HorizontalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unknown;

		position.Altitude = newLocation.EllipsoidalAltitude;
		position.AltitudeAccuracy = newLocation.VerticalAccuracy;
		position.AltitudeAccuracyReference = newLocation.VerticalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unknown;
		position.AltitudeReference = AltitudeReferenceType.Ellipsoid;

		position.Latitude = newLocation.Coordinate.Latitude;
		position.Longitude = newLocation.Coordinate.Longitude;

		#if __IOS__ || __MACOS__
		position.HasSpeed = newLocation.Speed > -1;
		position.Speed = newLocation.Speed;

		if (includeHeading)
		{
			position.HasHeading = newLocation.Course > -1;
			position.Heading = newLocation.Course;
		}
		#endif

		try
		{
			position.StatusTime = newLocation.Timestamp.ToDateTime().ToUniversalTime();
		}
		catch (Exception)
		{
			position.StatusTime = TimeService.UtcNow;
		}
		haveLocation = true;
	}

	private void StopListening()
	{
		manager.StopUpdatingLocation();
	}
}