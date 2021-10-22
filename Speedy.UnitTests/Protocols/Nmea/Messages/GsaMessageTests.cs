#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class GsaMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, GsaMessage expected)[]
			{
				(
					"$GNGSA,A,3,01,18,32,08,11,,,,,,,,6.16,1.86,5.88*16",
					new GsaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						AutoSelection = "A",
						Fix3D = "3",
						PrnsOfSatellitesUsedForFix = { 1, 18, 32, 08, 11 },
						PositionDilutionOfPrecision = 6.16,
						HorizontalDilutionOfPrecision = 1.86,
						VerticalDilutionOfPrecision = 5.88,
						Checksum = "16"
					}
				),
				(
					"$GPGSA,A,1,,,,,,,,,,,,,0.0,0.0,0.0*30",
					new GsaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						AutoSelection = "A",
						Fix3D = "1",
						PositionDilutionOfPrecision = 0,
						HorizontalDilutionOfPrecision = 0,
						VerticalDilutionOfPrecision = 0,
						Checksum = "30"
					}
				),
				(
					"$GPGSA,A,3,25,05,02,20,06,13,29,51,46,12,15,,1.43,0.76,1.21*0B",
					new GsaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						AutoSelection = "A",
						Fix3D = "3",
						PrnsOfSatellitesUsedForFix = { 25, 05, 02, 20, 06, 13, 29, 51, 46, 12, 15 },
						PositionDilutionOfPrecision = 1.43,
						HorizontalDilutionOfPrecision = 0.76,
						VerticalDilutionOfPrecision = 1.21,
						Checksum = "0B"
					}
				),
				(
					"$GPGSA,A,3,79,87,81,,,,,,,,,,1.43,0.76,1.21*0F",
					new GsaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						AutoSelection = "A",
						Fix3D = "3",
						PrnsOfSatellitesUsedForFix = { 79, 87, 81 },
						PositionDilutionOfPrecision = 1.43,
						HorizontalDilutionOfPrecision = 0.76,
						VerticalDilutionOfPrecision = 1.21,
						Checksum = "0F"
					}
				)
			});
		}

		#endregion
	}
}