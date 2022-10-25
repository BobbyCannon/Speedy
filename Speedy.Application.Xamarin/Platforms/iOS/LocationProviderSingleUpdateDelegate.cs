#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Speedy.Devices.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

[Preserve(AllMembers = true)]
internal class LocationProviderSingleUpdateDelegate<T> : CLLocationManagerDelegate
	where T : class, ILocation, new()
{
	private bool _haveLocation;
	private readonly T _position;
	private readonly double _desiredAccuracy;
	private readonly bool _includeHeading;
	private readonly TaskCompletionSource<T> _tcs;
	private readonly CLLocationManager _manager;

	public LocationProviderSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, bool includeHeading, int timeout, CancellationToken cancelToken)
	{
		_manager = manager;
		_position = new T();
		_tcs = new TaskCompletionSource<T>(manager);
		_desiredAccuracy = desiredAccuracy;
		_includeHeading = includeHeading;

		if (timeout != Timeout.Infinite)
		{
			Timer t = null;
			t = new Timer(s =>
			{
				if (_haveLocation)
				{
					var response = new T();
					response.UpdateWith(_position);
					_tcs.TrySetResult(response);
				}
				else
				{
					_tcs.TrySetCanceled();
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
			_tcs.TrySetCanceled();
		});
	}

	public Task<T> Task => _tcs?.Task;

	public override void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status)
	{
		// If user has services disabled, we're just going to throw an exception for consistency.
		if ((status == CLAuthorizationStatus.Denied) || (status == CLAuthorizationStatus.Restricted))
		{
			StopListening();
			_tcs.TrySetException(new LocationProviderException(LocationProviderError.Unauthorized));
		}
	}

	public override void Failed(CLLocationManager manager, NSError error)
	{
		switch ((CLError) (int) error.Code)
		{
			case CLError.Network:
				StopListening();
				_tcs.SetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
				break;
			case CLError.LocationUnknown:
				StopListening();
				_tcs.TrySetException(new LocationProviderException(LocationProviderError.PositionUnavailable));
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

		if (_haveLocation && (newLocation.HorizontalAccuracy > _position.Accuracy))
		{
			return;
		}

		_position.Accuracy = newLocation.HorizontalAccuracy;
		_position.AccuracyReference = newLocation.HorizontalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;

		_position.Altitude = newLocation.EllipsoidalAltitude;
		_position.AltitudeAccuracy = newLocation.VerticalAccuracy;
		_position.AltitudeAccuracyReference = newLocation.VerticalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;
		_position.AltitudeReference = AltitudeReferenceType.Ellipsoid;

		_position.Latitude = newLocation.Coordinate.Latitude;
		_position.Longitude = newLocation.Coordinate.Longitude;

		#if __IOS__ || __MACOS__
		_position.HasSpeed = newLocation.Speed > -1;
		_position.Speed = newLocation.Speed;

		if (_includeHeading)
		{
			_position.HasHeading = newLocation.Course > -1;
			_position.Heading = newLocation.Course;
		}
		#endif

		try
		{
			_position.StatusTime = newLocation.Timestamp.ToDateTime().ToUniversalTime();
		}
		catch (Exception)
		{
			_position.StatusTime = TimeService.UtcNow;
		}
		_haveLocation = true;
	}

	private void StopListening()
	{
		_manager.StopUpdatingLocation();
	}
}