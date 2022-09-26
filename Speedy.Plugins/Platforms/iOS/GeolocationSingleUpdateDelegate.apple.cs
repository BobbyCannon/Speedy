#region References

using CoreLocation;
using Foundation;
using Speedy.Plugins.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Maui
{
	[Preserve(AllMembers = true)]
	internal class GeolocationSingleUpdateDelegate : CLLocationManagerDelegate
	{
		private bool haveLocation;
		private readonly ProviderLocation position = new ProviderLocation();

		private readonly double desiredAccuracy;
		private readonly bool includeHeading;
		private readonly TaskCompletionSource<IProviderLocation> tcs;
		private readonly CLLocationManager manager;

		public GeolocationSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, bool includeHeading, int timeout, CancellationToken cancelToken)
		{
			this.manager = manager;
			tcs = new TaskCompletionSource<IProviderLocation>(manager);
			this.desiredAccuracy = desiredAccuracy;
			this.includeHeading = includeHeading;

			if (timeout != Timeout.Infinite)
			{
				Timer t = null;
				t = new Timer(s =>
				{
					if (haveLocation)
					{
						tcs.TrySetResult((IProviderLocation) position.ShallowClone());
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

		public Task<IProviderLocation> Task => tcs?.Task;

		public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
		{
			// If user has services disabled, we're just going to throw an exception for consistency.
			if ((status == CLAuthorizationStatus.Denied) || (status == CLAuthorizationStatus.Restricted))
			{
				StopListening();
				tcs.TrySetException(new GeolocationException(GeolocationError.Unauthorized));
			}
		}

		public override void Failed(CLLocationManager manager, NSError error)
		{
			switch ((CLError) (int) error.Code)
			{
				case CLError.Network:
					StopListening();
					tcs.SetException(new GeolocationException(GeolocationError.PositionUnavailable));
					break;
				case CLError.LocationUnknown:
					StopListening();
					tcs.TrySetException(new GeolocationException(GeolocationError.PositionUnavailable));
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

			position.HasAccuracy = true;
			position.Accuracy = newLocation.HorizontalAccuracy;
			position.HasAltitude = newLocation.VerticalAccuracy > -1;
			position.Altitude = newLocation.Altitude;
			position.AltitudeAccuracy = newLocation.VerticalAccuracy;
			position.HasLatitudeLongitude = newLocation.HorizontalAccuracy > -1;
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
}