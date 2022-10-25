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
		var scenarios = new List<(IVerticalLocation location, IVerticalLocation relativeTo, double expected)>
		{
			// 10 Terrain above 100 Ellipsoid should be 110
			(new VerticalLocation(10, AltitudeReferenceType.Terrain),
				new VerticalLocation(100, AltitudeReferenceType.Ellipsoid),
				110),
			// 10 Ellipsoid should just be 10
			(new VerticalLocation(10, AltitudeReferenceType.Ellipsoid),
				new VerticalLocation(100, AltitudeReferenceType.Ellipsoid),
				10),
			// Terrain above another Terrain should just be the original Terrain
			(new VerticalLocation(12, AltitudeReferenceType.Ellipsoid),
				new VerticalLocation(24, AltitudeReferenceType.Terrain),
				12)
		};

		foreach (var x in scenarios)
		{
			var actual = x.location.GetEllipsoidAltitude(x.relativeTo);
			Assert.AreEqual(x.expected, actual);
		}
	}

	[TestMethod]
	public void SupportedAccuracyReferenceTypesShouldAffectExtensionMethods()
	{
		var location = new Location();

		void assert(bool expected)
		{
			AreEqual(expected, location.HasSupportedAccuracy());
			AreEqual(expected, location.HasAccuracy);
			AreEqual(expected, location.HasSupportedAltitudeAccuracy());
			AreEqual(expected, location.HasAltitudeAccuracy);
		}

		assert(false);

		var expectedValues = new[]
		{
			AccuracyReferenceType.Meters
		};

		foreach (var value in expectedValues)
		{
			location.AccuracyReference = value;
			location.AltitudeAccuracyReference = value;

			assert(true);

			LocationExtensions.SupportedAccuracyReferenceTypes.Remove(value);

			assert(false);
		}

		// All values should no longer be supported
		var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

		foreach (var value in allValues)
		{
			location.AltitudeReference = value;
			assert(false);
		}
	}

	[TestMethod]
	public void SupportedAltitudeReferenceTypesShouldAffectExtensionMethods()
	{
		var location = new Location();
		AreEqual(false, location.HasSupportedAltitude());

		var expectedValues = new[]
		{
			AltitudeReferenceType.Ellipsoid,
			AltitudeReferenceType.Geoid,
			AltitudeReferenceType.Terrain
		};

		foreach (var value in expectedValues)
		{
			location.AltitudeReference = value;
			AreEqual(true, location.HasAltitude);

			LocationExtensions.SupportedAltitudeReferenceTypes.Remove(value);
			AreEqual(false, location.HasAltitude);
		}

		// All values should no longer be supported
		var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

		foreach (var value in allValues)
		{
			location.AltitudeReference = value;
			AreEqual(false, location.HasAltitude);
		}
	}

	[TestMethod]
	public void SupportedTypesDefaults()
	{
		AreEqual(new[] { AccuracyReferenceType.Meters },
			LocationExtensions.SupportedAccuracyReferenceTypes);

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