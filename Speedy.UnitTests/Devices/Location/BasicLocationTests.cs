#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Devices.Location;

#endregion

namespace Speedy.UnitTests.Devices.Location
{
	[TestClass]
	public class BasicLocationTests
	{
		#region Methods

		[TestMethod]
		public void Compare()
		{
			// Commented out scenario will be handled below
			var scenarios = new[]
			{
				//new BasicLocation(0, 0, 0),
				new BasicLocation(0, 0, 1, AltitudeReferenceType.Unspecified),
				new BasicLocation(0, 1, 0, AltitudeReferenceType.Ellipsoid),
				new BasicLocation(0, 1, 1, AltitudeReferenceType.Terrain),
				new BasicLocation(1, 0, 1, AltitudeReferenceType.Ellipsoid),
				//new BasicLocation(1, 1, 1),
				new BasicLocation(0, 4.321, 0, AltitudeReferenceType.Geoid),
				new BasicLocation(0, 8.456, 456.45, AltitudeReferenceType.Unspecified),
				new BasicLocation(9123, 0, 0, AltitudeReferenceType.Terrain),
				new BasicLocation(745.76, 0, 0.00001, AltitudeReferenceType.Geoid),
				new BasicLocation(134165456, 0.687840, 0, AltitudeReferenceType.Unspecified)
			};

			foreach (var scenario in scenarios)
			{
				// todo: loop all type of references
				// Should equal 
				Assert.AreEqual(0, new BasicLocation(0, 0, 0, AltitudeReferenceType.Ellipsoid)
					.CompareTo(new BasicLocation(0, 0, 0, AltitudeReferenceType.Ellipsoid)));
				Assert.AreEqual(0, new BasicLocation(1, 1, 1, AltitudeReferenceType.Geoid)
					.CompareTo(new BasicLocation(1, 1, 1, AltitudeReferenceType.Geoid)));
				Assert.AreEqual(0, scenario.CompareTo(new BasicLocation(scenario.Latitude, scenario.Longitude, scenario.Altitude, scenario.AltitudeReference)));

				// Should not match
				Assert.AreEqual(1, scenario.CompareTo(new BasicLocation(0, 0, 0, AltitudeReferenceType.Unspecified)));
				Assert.AreEqual(1, scenario.CompareTo(new BasicLocation(1, 1, 1, AltitudeReferenceType.Unspecified)));
			}
		}

		[TestMethod]
		public void FromLocation()
		{
			var location = new Speedy.Devices.Location.Location
			{
				HorizontalLocation =
				{
					Latitude = 1.234,
					Longitude = 4.321
				},
				VerticalLocation =
				{
					AltitudeReference = AltitudeReferenceType.Ellipsoid,
					Altitude = 123.45
				}
			};

			BasicLocation actual = location;
			Assert.AreEqual(1.234, actual.Latitude);
			Assert.AreEqual(4.321, actual.Longitude);
			Assert.AreEqual(AltitudeReferenceType.Ellipsoid, actual.AltitudeReference);
			Assert.AreEqual(123.45, actual.Altitude);
		}

		#endregion
	}
}