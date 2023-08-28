#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea.Messages
{
	[TestClass]
	public class VtgMessageTests : BaseMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodParse()
		{
			ProcessParseScenarios(new (string sentance, VtgMessage expected)[]
			{
				(
					"$GPVTG,103.85,T,92.79,M,0.14,N,0.25,K,D*1E",
					new VtgMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						TrueCourse = "103.85",
						TrueCourseUnit = "T",
						MagneticCourse = "92.79",
						MagneticCourseUnit = "M",
						GroundSpeed = "0.14",
						GroundSpeedUnit = "N",
						GroundSpeedKilometersPerHour = "0.25",
						GroundSpeedKilometersPerHourUnit = "K",
						ModeIndicator = new ModeIndicator("D"),
						Checksum = "1E"
					}
				),
				(
					"$GPVTG,,T,654.21,M,2.44,N,1.05,K,E*3B",
					new VtgMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						TrueCourse = "",
						TrueCourseUnit = "T",
						MagneticCourse = "654.21",
						MagneticCourseUnit = "M",
						GroundSpeed = "2.44",
						GroundSpeedUnit = "N",
						GroundSpeedKilometersPerHour = "1.05",
						GroundSpeedKilometersPerHourUnit = "K",
						ModeIndicator = new ModeIndicator("E"),
						Checksum = "3B"
					}
				),
				(
					"$GPVTG,,,,,,,,*52",
					new VtgMessage
					{
						Prefix = NmeaMessagePrefix.GlobalPositioningSystem,
						TrueCourse = "",
						TrueCourseUnit = "",
						MagneticCourse = "",
						MagneticCourseUnit = "",
						GroundSpeed = "",
						GroundSpeedUnit = "",
						GroundSpeedKilometersPerHour = "",
						GroundSpeedKilometersPerHourUnit = "",
						Checksum = "52"
					}
				)
			});
		}

		#endregion
	}
}