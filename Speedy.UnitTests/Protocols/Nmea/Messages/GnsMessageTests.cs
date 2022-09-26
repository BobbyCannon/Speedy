#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class GnsMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void ShouldParse()
		{
			ProcessParseScenarios(new (string sentance, GnsMessage expected)[]
			{
				(
					"$GNGNS,014035.00,4332.69262,S,17235.48549,E,RR,13,0.9,25.63,11.24,,*70",
					new GnsMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						Time = 14035.00,
						Latitude = new Speedy.Protocols.Nmea.NmeaLocation("4332.69262", "S"),
						Longitude = new Speedy.Protocols.Nmea.NmeaLocation("17235.48549", "E"),
						ModeIndicator = "RR",
						NumberOfSatellites = 13,
						HorizontalDilutionOfPrecision = 0.9,
						Altitude = 25.63,
						HeightOfGeoid = 11.24,
						AgeOfDifferentialData = "",
						DifferentialReferenceStationId = "",
						Checksum = "70"
					}
				),
				(
					"$GPGNS,003000.00,4253.65208,N,07852.11903,W,DA,14,0.76,253.0,-35.4,,0000*46",
					new GnsMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = 3000,
						Latitude = new Speedy.Protocols.Nmea.NmeaLocation("4253.65208", "N"),
						Longitude = new Speedy.Protocols.Nmea.NmeaLocation("07852.11903", "W"),
						ModeIndicator = "DA",
						NumberOfSatellites = 14,
						HorizontalDilutionOfPrecision = 0.76,
						Altitude = 253.0,
						HeightOfGeoid = -35.4,
						AgeOfDifferentialData = "",
						DifferentialReferenceStationId = "0000",
						Checksum = "46"
					}
				),
				(
					// Message with garbage at the beginning
					"?\t\f     \b\f ? ?$???\r\a,? O  \a$$GPGNS,003000.00,4253.65208,N,07852.11903,W,DA,14,0.76,253.0,-35.4,,0000*46",
					new GnsMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = 3000,
						Latitude = new Speedy.Protocols.Nmea.NmeaLocation("4253.65208", "N"),
						Longitude = new Speedy.Protocols.Nmea.NmeaLocation("07852.11903", "W"),
						ModeIndicator = "DA",
						NumberOfSatellites = 14,
						HorizontalDilutionOfPrecision = 0.76,
						Altitude = 253.0,
						HeightOfGeoid = -35.4,
						AgeOfDifferentialData = "",
						DifferentialReferenceStationId = "0000",
						Checksum = "46"
					}
				)
			});
		}

		#endregion
	}
}