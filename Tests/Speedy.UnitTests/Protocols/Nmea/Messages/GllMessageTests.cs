#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class GllMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, GllMessage expected)[]
			{
				(
					"$GNGLL,4513.13795,N,01859.19702,E,143717.00,A,A*72",
					new GllMessage
					{
						Prefix = NmeaMessagePrefix.GlobalNavigationSatelliteSystem,
						Latitude = new Speedy.Protocols.Nmea.NmeaLocation("4513.13795", "N"),
						Longitude = new Speedy.Protocols.Nmea.NmeaLocation("01859.19702", "E"),
						Time = 143717.00,
						DataValid = "A",
						ModeIndicator = new ModeIndicator("Autonomous"),
						Checksum = "40"
					}
				)
			});
		}

		#endregion
	}
}