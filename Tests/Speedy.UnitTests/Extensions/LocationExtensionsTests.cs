#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.Location;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class LocationExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void DistanceBetween()
	{
		LocationExtensions.DistanceBetween(0.0, 0.0, 0.0, 0.1).Dump();
		LocationExtensions.DistanceBetween(0.0, 0.0, 0.0, -0.1).Dump();
		LocationExtensions.DistanceBetween(0.0, 0.0, 0.05, 0.0).Dump();
		LocationExtensions.DistanceBetween(0.0, 0.0, -0.05, 0.0).Dump();
		LocationExtensions.DistanceBetween(32, -75, 32, -75.05).Dump();

		LocationExtensions.DistanceBetween(0.0, 0.0, 0.0, 0.0, 0.0, 1).Dump("Altitude up distance meters");
	}

	[TestMethod]
	public void GetEllipsoidValue()
	{
		var scenarios = new List<(IMinimalVerticalLocation location, IMinimalVerticalLocation relativeTo, double expected)>
		{
			// 10 Terrain above 100 Ellipsoid should be 110
			(new VerticalLocation { Altitude = 10, AltitudeReference = AltitudeReferenceType.Terrain },
				new VerticalLocation { Altitude = 100, AltitudeReference = AltitudeReferenceType.Ellipsoid },
				110),
			// 10 Ellipsoid should just be 10
			(new VerticalLocation { Altitude = 10, AltitudeReference = AltitudeReferenceType.Ellipsoid },
				new VerticalLocation { Altitude = 100, AltitudeReference = AltitudeReferenceType.Ellipsoid },
				10),
			// Terrain above another Terrain should just be the original Terrain
			(new VerticalLocation { Altitude = 12, AltitudeReference = AltitudeReferenceType.Ellipsoid },
				new VerticalLocation { Altitude = 24, AltitudeReference = AltitudeReferenceType.Terrain },
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
		var locations = new ILocationInformation[]
		{
			new HorizontalLocation(), new VerticalLocation()
		};

		foreach (var location in locations)
		{
			void validate(bool expected)
			{
				AreEqual(expected, location.HasAccuracy());
				AreEqual(expected, location.HasAccuracy);
			}

			validate(false);

			var expectedValues = new[]
			{
				AccuracyReferenceType.Meters
			};

			foreach (var value in expectedValues)
			{
				location.AccuracyReference = value;

				validate(true);

				switch (location)
				{
					case IHorizontalLocation:
					{
						LocationExtensions.SupportedAccuracyReferenceTypesForHorizontal.Remove(value);
						break;
					}
					case IVerticalLocation:
					{
						LocationExtensions.SupportedAccuracyReferenceTypesForVertical.Remove(value);
						break;
					}
				}

				validate(false);
			}
		}

		//// All values should no longer be supported
		//var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

		//foreach (var value in allValues)
		//{
		//	location.AltitudeReference = value;
		//	validate(false);
		//}
	}

	[TestMethod]
	public void SupportedAltitudeReferenceTypesShouldAffectExtensionMethods()
	{
		var location = new VerticalLocation();
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
			AreEqual(true, location.HasSupportedAltitude());

			LocationExtensions.SupportedAltitudeReferenceTypes.Remove(value);
			AreEqual(false, location.HasSupportedAltitude());
		}

		// All values should no longer be supported
		var allValues = EnumExtensions.GetEnumValues<AltitudeReferenceType>();

		foreach (var value in allValues)
		{
			location.AltitudeReference = value;
			AreEqual(false, location.HasValue);
		}
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