#region References

using System;
using System.Threading;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Speedy.Data.Location;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Application.Xamarin;

[Preserve(AllMembers = true)]
internal class LocationProviderSingleUpdateDelegate<T, THorizontal, TVertical> : CLLocationManagerDelegate
	where T : class, ILocation<THorizontal, TVertical>, new()
	where THorizontal : class, IHorizontalLocation, IUpdatable<THorizontal>
	where TVertical : class, IVerticalLocation, IUpdatable<TVertical>
{
	#region Fields

	private readonly double _desiredAccuracy;
	private bool _haveLocation;
	private readonly CLLocationManager _manager;
	private readonly T _position;
	private readonly TaskCompletionSource<T> _tcs;

	#endregion

	#region Constructors

	public LocationProviderSingleUpdateDelegate(CLLocationManager manager, double desiredAccuracy, int timeout, CancellationToken cancelToken)
	{
		_manager = manager;
		_position = new T();
		_tcs = new TaskCompletionSource<T>(manager);
		_desiredAccuracy = desiredAccuracy;

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
				_tcs.SetException(new LocationProviderException(LocationProviderError.LocationUnavailable));
				break;
			case CLError.LocationUnknown:
				StopListening();
				_tcs.TrySetException(new LocationProviderException(LocationProviderError.LocationUnavailable));
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

		if (_haveLocation && (newLocation.HorizontalAccuracy > _position.HorizontalLocation.Accuracy))
		{
			return;
		}

		_position.VerticalLocation.Altitude = newLocation.EllipsoidalAltitude;
		_position.VerticalLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;

		_position.HorizontalLocation.Accuracy = newLocation.HorizontalAccuracy;
		_position.HorizontalLocation.AccuracyReference = newLocation.HorizontalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;
		
		_position.HorizontalLocation.Latitude = newLocation.Coordinate.Latitude;
		_position.HorizontalLocation.Longitude = newLocation.Coordinate.Longitude;

		_position.HorizontalLocation.HasHeading = newLocation.Course > -1;
		_position.HorizontalLocation.Heading = newLocation.Course;

		_position.HorizontalLocation.HasSpeed = newLocation.Speed > -1;
		_position.HorizontalLocation.Speed = newLocation.Speed;

		_position.VerticalLocation.Accuracy = newLocation.VerticalAccuracy;
		_position.VerticalLocation.AccuracyReference = newLocation.VerticalAccuracy > 0 ? AccuracyReferenceType.Meters : AccuracyReferenceType.Unspecified;

		try
		{
			var statusTime = newLocation.Timestamp.ToDateTime().ToUniversalTime();
			_position.HorizontalLocation.StatusTime = statusTime;
			_position.VerticalLocation.StatusTime = statusTime;
		}
		catch (Exception)
		{
			var statusTime = TimeService.UtcNow;
			_position.HorizontalLocation.StatusTime = statusTime;
			_position.VerticalLocation.StatusTime = statusTime;
		}

		_haveLocation = true;
	}

	private void StopListening()
	{
		_manager.StopUpdatingLocation();
	}

	#endregion
}