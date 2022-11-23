#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Location;

#endregion

namespace Speedy.UnitTests.Devices.Location;

[TestClass]
public class LocationStateComparerTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void LocationComparerShouldWork()
	{
		var location1 = new VerticalLocation();
		var location2 = new VerticalLocation();
		var comparer = new LocationInformationComparer<VerticalLocation>();
		AreEqual(location1, location2);

		location2.Altitude = 123.45;
		location2.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		location2.Accuracy = 1.234;
		location2.AccuracyReference = AccuracyReferenceType.Meters;
		location2.SourceName = "VP1";
		location2.StatusTime = new DateTime(2022, 11, 04, 11, 21, 13, DateTimeKind.Utc);

		AreEqual(true, comparer.ShouldUpdate(location1, location2));
		AreEqual(true, comparer.UpdateWith(ref location1, location2));

		// These should have changed
		AreEqual(123.45, location1.Altitude);
		AreEqual(AltitudeReferenceType.Ellipsoid, location1.AltitudeReference);
		AreEqual(1.234, location1.Accuracy);
		AreEqual(AccuracyReferenceType.Meters, location1.AccuracyReference);
		AreEqual("VP1", location1.SourceName);
		AreEqual(new DateTime(2022, 11, 04, 11, 21, 13, DateTimeKind.Utc), location1.StatusTime);

		//
		// A less accurate device should not update
		//
		location2.Altitude = 123.45;
		location2.AltitudeReference = AltitudeReferenceType.Ellipsoid;
		location2.Accuracy = 1.234;
		location2.AccuracyReference = AccuracyReferenceType.Unspecified;
		location2.SourceName = "VP2";
		location2.StatusTime = new DateTime(2022, 11, 04, 11, 21, 14, DateTimeKind.Utc);

		AreEqual(false, comparer.ShouldUpdate(location1, location2));
	}

	#endregion
}