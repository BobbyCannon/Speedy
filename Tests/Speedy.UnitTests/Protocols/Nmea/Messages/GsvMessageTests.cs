#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class GsvMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, GsvMessage expected)[]
			{
				(
					"$GPGSV,3,1,10,01,50,304,26,03,24,245,16,08,56,204,28,10,21,059,20*77",
					new GsvMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						NumberOfSentences = 3,
						SentenceNr = 1,
						NumberOfSatellitesInView = 10,
						Satellites =
						{
							new Satellite("01", "50", "304", "26"),
							new Satellite("03", "24", "245", "16"),
							new Satellite("08", "56", "204", "28"),
							new Satellite("10", "21", "059", "20")
						},
						Checksum = "16"
					}
				),
				(
					"$GPGSV,4,1,14,02,55,085,20,05,77,250,26,06,17,095,18,09,05,032,*7A",
					new GsvMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						NumberOfSentences = 4,
						SentenceNr = 1,
						NumberOfSatellitesInView = 14,
						Satellites =
						{
							new Satellite("02", "55", "085", "20"),
							new Satellite("05", "77", "250", "26"),
							new Satellite("06", "17", "095", "18"),
							new Satellite("09", "05", "032", "")
						},
						Checksum = "7A"
					}
				),
				(
					"$GPGSV,4,4,14,48,22,237,39,51,33,218,39*79",
					new GsvMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						NumberOfSentences = 4,
						SentenceNr = 4,
						NumberOfSatellitesInView = 14,
						Satellites =
						{
							new Satellite("48", "22", "237", "39"),
							new Satellite("51", "33", "218", "39")
						},
						Checksum = "79"
					}
				)
			});
		}

		#endregion
	}
}