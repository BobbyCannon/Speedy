#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class TxtMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, TxtMessage expected)[]
			{
				(
					"$GNTXT,01,01,02,u-blox AG - www.u-blox.com*4E",
					new TxtMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						TotalNumberOfSentences = 1,
						SentenceNumber = 1,
						TextIdentifier = "02",
						Text = "u-blox AG - www.u-blox.com",
						Checksum = "4E"
					}
				),
				(
					"$GNTXT,03,02,01,hello world*71",
					new TxtMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						TotalNumberOfSentences = 3,
						SentenceNumber = 2,
						TextIdentifier = "01",
						Text = "hello world",
						Checksum = "71"
					}
				),
				(
					"$GPTXT,12,04,03,foo bar*7C",
					new TxtMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						TotalNumberOfSentences = 12,
						SentenceNumber = 4,
						TextIdentifier = "03",
						Text = "foo bar",
						Checksum = "7C"
					}
				),
				(
					"$GLTXT,99,99,99,foobar*44",
					new TxtMessage
					{
						Prefix = NmeaMessagePrefix.GlonassReceiver,
						TotalNumberOfSentences = 99,
						SentenceNumber = 99,
						TextIdentifier = "99",
						Text = "foobar",
						Checksum = "44"
					}
				)
			});
		}

		#endregion
	}
}