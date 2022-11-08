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
	#region Fields

	private readonly double _desiredAccuracy;
	private bool _haveLocation;
	private readonly bool _includeHeading;
	private readonly CLLocationManager _manager;
	private readonly T _position;
	private readonly TaskCompletionSource<T> _tcs;

	#endregion

	#region Constructors

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

		manager.ShouldDisplayHeadingCalibration += locationManager =>
		{
			locationManager.DismissHeadingCalibrationDisplay();
			return false;
		};

		cancelToken.Register(() =>
		{
			StopListening();
			_tcs.TrySetCanceled();
		});
	}

	#endregion

	#region Properties

	public Task<T> Task => _tcs?.Task;

	#endregion

	#region Methods

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

	public override bool ShouldDisplayHeadingCalibration(CLLocationManager locationManager)
	{
		locationManager.DismissHeadingCalibrationDisplay();
		return false;
	}

	public override void UpdatedLocation(CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
	{
		if (newLocation.HorizontalAccuracy < 0)
		{
			return;
		}

		if (_haveLocation && (newLocation.HorizontalAccuracy > _position.HorizontalAccuracy))
		{
			return;
		}

		_position.Altitude = newLocation.EllipsoidalAltitude;
		_position.AltitudeReference = AltitudeReferenceType.Ellipsoid;

		_position.HorizontalAccuracy = newLocation.HorizontalAccuracy;
		_position.HorizontalAccuracyReference = newLocation.HorizontalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;
		
		_position.Latitude = newLocation.Coordinate.Latitude;
		_position.Longitude = newLocation.Coordinate.Longitude;

		_position.HasHorizontalSpeed = newLocation.Speed > -1;
		_position.HorizontalSpeed = newLocation.Speed;

		_position.VerticalAccuracy = newLocation.VerticalAccuracy;
		_position.VerticalAccuracyReference = newLocation.VerticalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;

		if (_includeHeading)
		{
			_position.HasHorizontalHeading = newLocation.Course > -1;
			_position.HorizontalHeading = newLocation.Course;
		}

		try
		{
			var statusTime = newLocation.Timestamp.ToDateTime().ToUniversalTime();
			_position.HorizontalStatusTime = statusTime;
			_position.VerticalStatusTime = statusTime;
		}
		catch (Exception)
		{
			var statusTime = TimeService.UtcNow;
			_position.HorizontalStatusTime = statusTime;
			_position.VerticalStatusTime = statusTime;
		}
		_haveLocation = true;
	}

	private void StopListening()
	{
		_manager.StopUpdatingLocation();
	}

	#endregion
}