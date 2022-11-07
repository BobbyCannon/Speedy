#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Devices.Location;

#endregion

namespace Speedy.UnitTests.Devices.Location;

[TestClass]
public class LocationStateComparerTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void LocationComparerShouldWork()
	{
		var comparer = new LocationComparer();
		var providerLocation = new Speedy.Devices.Location.Location();
		AreEqual(providerLocation, comparer.CurrentState);

		providerLocation.Altitude = 123.45;
		providerLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		providerLocation.VerticalAccuracy = 1.234;
		providerLocation.VerticalAccuracyReference = AccuracyReferenceType.Meters;
		providerLocation.VerticalSourceName = "VP1";
		providerLocation.VerticalStatusTime = new DateTime(2022, 11, 04, 11, 21, 13, DateTimeKind.Utc);

		comparer.Refresh(providerLocation);

		// These should have changed
		AreEqual(123.45, comparer.CurrentState.Altitude);
		AreEqual(AltitudeReferenceType.Ellipsoid, comparer.CurrentState.AltitudeReference);
		AreEqual(1.234, comparer.CurrentState.VerticalAccuracy);
		AreEqual(AccuracyReferenceType.Meters, comparer.CurrentState.VerticalAccuracyReference);
		AreEqual("VP1", comparer.CurrentState.VerticalSourceName);
		AreEqual(new DateTime(2022, 11, 04, 11, 21, 13, DateTimeKind.Utc), comparer.CurrentState.VerticalStatusTime);

		// These item should not have changed
		AreEqual(0, comparer.CurrentState.Latitude);
		AreEqual(0, comparer.CurrentState.Longitude);
		AreEqual(0, comparer.CurrentState.HorizontalAccuracy);
		AreEqual(AccuracyReferenceType.Unspecified, comparer.CurrentState.HorizontalAccuracyReference);
		AreEqual(null, comparer.CurrentState.HorizontalSourceName);
		AreEqual(DateTime.MinValue, comparer.CurrentState.HorizontalStatusTime);

		//
		// A less accurate device should not update
		//
		providerLocation.Altitude = 123.45;
		providerLocation.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		providerLocation.VerticalAccuracy = 1.234;
		providerLocation.VerticalAccuracyReference = AccuracyReferenceType.Unspecified;
		providerLocation.VerticalSourceName = "VP2";
		providerLocation.VerticalStatusTime = new DateTime(2022, 11, 04, 11, 21, 14, DateTimeKind.Utc);

		comparer.Refresh(providerLocation);

		// These should not have changed
		AreEqual(123.45, comparer.CurrentState.Altitude);
		AreEqual(AltitudeReferenceType.Ellipsoid, comparer.CurrentState.AltitudeReference);
		AreEqual(1.234, comparer.CurrentState.VerticalAccuracy);
		AreEqual(AccuracyReferenceType.Meters, comparer.CurrentState.VerticalAccuracyReference);
		AreEqual("VP1", comparer.CurrentState.VerticalSourceName);
		AreEqual(new DateTime(2022, 11, 04, 11, 21, 13, DateTimeKind.Utc), comparer.CurrentState.VerticalStatusTime);
	}

	#endregion
}