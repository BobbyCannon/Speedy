#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Devices.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class LocationExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void GetEllipsoidValue()
	{
		Assert.Fail("aoeu");
	//	var scenarios = new List<(IMinimalVerticalLocation location, IMinimalVerticalLocation relativeTo, double expected)>
	//	{
	//		// 10 Terrain above 100 Ellipsoid should be 110
	//		(new Location { Altitude = 10, AltitudeReference = AltitudeReferenceType.Terrain },
	//			new Location { Altitude = 100, AltitudeReference = AltitudeReferenceType.Ellipsoid },
	//			110),
	//		// 10 Ellipsoid should just be 10
	//		(new Location { Altitude = 10, AltitudeReference = AltitudeReferenceType.Ellipsoid },
	//			new Location { Altitude = 100, AltitudeReference = AltitudeReferenceType.Ellipsoid },
	//			10),
	//		// Terrain above another Terrain should just be the original Terrain
	//		(new Location { Altitude = 12, AltitudeReference = AltitudeReferenceType.Ellipsoid },
	//			new Location { Altitude = 24, AltitudeReference = AltitudeReferenceType.Terrain },
	//			12)
	//	};

	//	foreach (var x in scenarios)
	//	{
	//		var actual = x.location.GetEllipsoidAltitude(x.relativeTo);
	//		Assert.AreEqual(x.expected, actual);
	//	}
	//}

	//[TestMethod]
	//public void SupportedAccuracyReferenceTypesShouldAffectExtensionMethods()
	//{
	//	var location = new Location();

	//	void validate(bool expected)
	//	{
	//		AreEqual(expected, location.HasSupportedHorizontalAccuracy());
	//		AreEqual(expected, location.HasHorizontalAccuracy);
	//		AreEqual(expected, location.HasSupportedVerticalAccuracy());
	//		AreEqual(expected, location.HasVerticalAccuracy);
	//	}

	//	validate(false);

	//	var expectedValues = new[]
	//	{
	//		AccuracyReferenceType.Meters
	//	};

	//	foreach (var value in expectedValues)
	//	{
	//		location.HorizontalAccuracyReference = value;
	//		location.VerticalAccuracyReference = value;

	//		validate(true);

	//		LocationExtensions.SupportedAccuracyReferenceTypesForHorizontal.Remove(value);
	//		LocationExtensions.SupportedAccuracyReferenceTypesForVertical.Remove(value);

	//		validate(false);
	//	}

	//	// All values should no longer be supported
	//	var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

	//	foreach (var value in allValues)
	//	{
	//		location.AltitudeReference = value;
	//		validate(false);
	//	}
	//}

	//[TestMethod]
	//public void SupportedAltitudeReferenceTypesShouldAffectExtensionMethods()
	//{
	//	var location = new Location();
	//	AreEqual(false, location.HasSupportedAltitude());

	//	var expectedValues = new[]
	//	{
	//		AltitudeReferenceType.Ellipsoid,
	//		AltitudeReferenceType.Geoid,
	//		AltitudeReferenceType.Terrain
	//	};

	//	foreach (var value in expectedValues)
	//	{
	//		location.AltitudeReference = value;
	//		AreEqual(true, location.HasAltitude);

	//		LocationExtensions.SupportedAltitudeReferenceTypes.Remove(value);
	//		AreEqual(false, location.HasAltitude);
	//	}

	//	// All values should no longer be supported
	//	var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

	//	foreach (var value in allValues)
	//	{
	//		location.AltitudeReference = value;
	//		AreEqual(false, location.HasAltitude);
	//	}
	}

	[TestMethod]
	public void SupportedTypesDefaults()
	{
		AreEqual(new[] { AccuracyReferenceType.Meters },
			LocationExtensions.SupportedAccuracyReferenceTypesForVertical);

		AreEqual(new[]
			{
				AltitudeReferenceType.Ellipsoid,
				AltitudeReferenceType.Geoid,
				AltitudeReferenceType.Terrain
			},
			LocationExtensions.SupportedAltitudeReferenceTypes);
	}

	#endregion
}