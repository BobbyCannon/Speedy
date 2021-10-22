#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class GgaMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, GgaMessage expected)[]
			{
				(
					"$GNGGA,143718.00,4513.13793,N,01859.19704,E,1,05,1.86,108.1,M,38.1,M,,*40",
					new GgaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						Time = 143718.00,
						Latitude = new Location("4513.13793", "N"),
						Longitude = new Location("01859.19704", "E"),
						FixQuality = "1",
						NumberOfSatellites = 5,
						HorizontalDilutionOfPrecision = 1.86,
						Altitude = 108.1,
						AltitudeUnit = "M",
						HeightOfGeoid = 38.1,
						HeightOfGeoidUnit = "M",
						SecondsSinceLastUpdateDgps = "",
						StationIdNumberDgps = "",
						Checksum = "40"
					}
				),
				(
					"$GPGGA,014349.36,,,,,0,00,,,M,0.0,M,,0000*68",
					new GgaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = 014349.36,
						Latitude = new Location("", ""),
						Longitude = new Location("", ""),
						FixQuality = "0",
						NumberOfSatellites = 0,
						HorizontalDilutionOfPrecision = 0,
						Altitude = 0,
						AltitudeUnit = "M",
						HeightOfGeoid = 0,
						HeightOfGeoidUnit = "M",
						SecondsSinceLastUpdateDgps = "",
						StationIdNumberDgps = "0000",
						Checksum = "68"
					}
				)
			});
		}

		#endregion
	}
}