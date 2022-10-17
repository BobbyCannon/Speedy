#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Devices.Location;

#endregion

namespace Speedy.UnitTests.Devices.Location
{
	[TestClass]
	public class GeoLocationTests
	{
		#region Methods

		[TestMethod]
		public void Compare()
		{
			// Commented out scenario will be handled below
			var scenarios = new[]
			{
				//new GeoLocation(0, 0, 0),
				new GeoLocation(0, 0, 1, AltitudeReferenceType.Unspecified),
				new GeoLocation(0, 1, 0, AltitudeReferenceType.Ellipsoid),
				new GeoLocation(0, 1, 1, AltitudeReferenceType.Terrain),
				new GeoLocation(1, 0, 1, AltitudeReferenceType.Ellipsoid),
				//new GeoLocation(1, 1, 1),
				new GeoLocation(0, 4.321, 0, AltitudeReferenceType.Geoid),
				new GeoLocation(0, 8.456, 456.45, AltitudeReferenceType.Unspecified),
				new GeoLocation(9123, 0, 0, AltitudeReferenceType.Terrain),
				new GeoLocation(745.76, 0, 0.00001, AltitudeReferenceType.Geoid),
				new GeoLocation(134165456, 0.687840, 0, AltitudeReferenceType.Unspecified)
			};

			foreach (var scenario in scenarios)
			{
				// todo: loop all type of references
				// Should equal 
				Assert.AreEqual(0, new GeoLocation(0, 0, 0, AltitudeReferenceType.Ellipsoid)
					.CompareTo(new GeoLocation(0, 0, 0, AltitudeReferenceType.Ellipsoid)));
				Assert.AreEqual(0, new GeoLocation(1, 1, 1, AltitudeReferenceType.Geoid)
					.CompareTo(new GeoLocation(1, 1, 1, AltitudeReferenceType.Geoid)));
				Assert.AreEqual(0, scenario.CompareTo(new GeoLocation(scenario.Latitude, scenario.Longitude, scenario.Altitude, scenario.AltitudeReference)));

				// Should not match
				Assert.AreEqual(1, scenario.CompareTo(new GeoLocation(0, 0, 0, AltitudeReferenceType.Unspecified)));
				Assert.AreEqual(1, scenario.CompareTo(new GeoLocation(1, 1, 1, AltitudeReferenceType.Unspecified)));
			}
		}

		#endregion
	}
}