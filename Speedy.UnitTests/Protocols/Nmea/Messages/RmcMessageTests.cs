#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class RmcMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, RmcMessage expected)[]
			{
				(
					"$GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A*65",
					new RmcMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						Time = "143718.00",
						Status = "A",
						Latitude = new Location("4513.13793", "N"),
						Longitude = new Location("01859.19704", "E"),
						Speed = "0.050",
						Course = "",
						DateOfFix = "290719",
						MagneticVariation = "",
						MagneticVariationUnit = "",
						ModeIndicator = new ModeIndicator("A"),
						Checksum = "65"
					}
				),
				(
					"$GPRMC,002959.00,A,4253.65205,N,07852.11902,W,0.022,,020521,,,D*63",
					new RmcMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "002959.00",
						Status = "A",
						Latitude = new Location("4253.65205", "N"),
						Longitude = new Location("07852.11902", "W"),
						Speed = "0.022",
						Course = "",
						DateOfFix = "020521",
						MagneticVariation = "",
						MagneticVariationUnit = "",
						ModeIndicator = new ModeIndicator("D"),
						Checksum = "63"
					}
				),
				(
					"$GPRMC,210230,A,3855.4487,N,09446.0071,W,0.0,076.2,130495,003.8,E*69",
					new RmcMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "210230",
						Status = "A",
						Latitude = new Location("3855.4487", "N"),
						Longitude = new Location("09446.0071", "W"),
						Speed = "0.0",
						Course = "076.2",
						DateOfFix = "130495",
						MagneticVariation = "003.8",
						MagneticVariationUnit = "E",
						Checksum = "63"
					}
				),
				(
					// Message with garbage at the beginning
					"  ?D?v?$$$GPRMC,002959.00,A,4253.65205,N,07852.11902,W,0.022,,020521,,,D*63",
					new RmcMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "002959.00",
						Status = "A",
						Latitude = new Location("4253.65205", "N"),
						Longitude = new Location("07852.11902", "W"),
						Speed = "0.022",
						Course = "",
						DateOfFix = "020521",
						MagneticVariation = "",
						MagneticVariationUnit = "",
						ModeIndicator = new ModeIndicator("D"),
						Checksum = "63"
					}
				),
				(
					// Message with garbage at the beginning
					"      lQ?b ?? ?  ?c?4?$GPRMC,003007.00,A,4253.65269,N,07852.11899,W,0.054,,020521,,,D*68",
					new RmcMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "003007.00",
						Status = "A",
						Latitude = new Location("4253.65269", "N"),
						Longitude = new Location("07852.11899", "W"),
						Speed = "0.054",
						Course = "",
						DateOfFix = "020521",
						MagneticVariation = "",
						MagneticVariationUnit = "",
						ModeIndicator = new ModeIndicator("D"),
						Checksum = "68"
					}
				)
			});
		}

		#endregion
	}
}