#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class ZdaMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, ZdaMessage expected)[]
			{
				(
					"$GPZDA,172809.45,12,07,1996,00,00*61",
					new ZdaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "172809.45",
						Day = 12,
						Month = 7,
						Year = 1996,
						HourOffset = 0,
						MinuteOffset = 0,
						Checksum = "61"
					}
				),
				(
					"$GPZDA,002958.00,02,05,2021,00,00*66",
					new ZdaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "002958.00",
						Day = 2,
						Month = 5,
						Year = 2021,
						HourOffset = 0,
						MinuteOffset = 0,
						Checksum = "66"
					}
				),
				(
					"$GPZDA,012345.67,31,12,2021,85,14*6E",
					new ZdaMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						Time = "012345.67",
						Day = 31,
						Month = 12,
						Year = 2021,
						HourOffset = 85,
						MinuteOffset = 14,
						Checksum = "6E"
					}
				)
			});
		}

		#endregion
	}
}