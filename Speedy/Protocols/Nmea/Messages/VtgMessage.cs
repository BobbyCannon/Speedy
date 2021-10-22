#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a VTG message.
	/// </summary>
	public class VtgMessage : NmeaMessage
	{
		#region Constructors

		public VtgMessage() : base(NmeaMessageType.VTG)
		{
		}

		#endregion

		#region Properties

		public string GroundSpeed { get; set; }

		public string GroundSpeedKilometersPerHour { get; set; }

		public string GroundSpeedKilometersPerHourUnit { get; set; }

		public string GroundSpeedUnit { get; set; }

		public string MagneticCourse { get; set; }

		public string MagneticCourseUnit { get; set; }

		public ModeIndicator ModeIndicator { get; set; }

		public string TrueCourse { get; set; }

		public string TrueCourseUnit { get; set; }

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
			// $GPVTG,140.88,T,,M,8.04,N,14.89,K,D*05
			//
			// .      0   1 2   3 4   5 6   7 8 9
			//        |   | |   | |   | |   | | |
			// $--VTG,x.x,a,x.x,a,x.x,a,x.x,a,a*hh
			//
			// 0) Track made good (degrees true)
			// 1) T: track made good is relative to true north
			// 2) Track made good (degrees magnetic)
			// 3) M: track made good is relative to magnetic north
			// 4) Speed, in knots
			// 5) N: speed is measured in knots
			// 6) Speed over ground in kilometers/hour (kph)
			// 7) K: speed over ground is measured in kph
			// 8) Mode Indicator
			//    A = Autonomous
			//    D = Differential
			//    E - Estimated
			//    M - Manual
			//    N - No Fix
			//    P - Precise
			//    R - Real Time Kinematic
			//    S - Simulator
			// 9) Checksum

			StartParse(sentence);

			TrueCourse = GetArgument(0);
			TrueCourseUnit = GetArgument(1);
			MagneticCourse = GetArgument(2);
			MagneticCourseUnit = GetArgument(3);
			GroundSpeed = GetArgument(4);
			GroundSpeedUnit = GetArgument(5);
			GroundSpeedKilometersPerHour = GetArgument(6);
			GroundSpeedKilometersPerHourUnit = GetArgument(7);
			ModeIndicator = Arguments.Count >= 9
				? new ModeIndicator(GetArgument(8))
				: null;

			OnNmeaMessageParsed(this);
		}

		public override string ToString()
		{
			var start = string.Join(",",
				NmeaParser.GetSentenceStart(this),
				TrueCourse,
				TrueCourseUnit,
				MagneticCourse,
				MagneticCourseUnit,
				GroundSpeed,
				GroundSpeedUnit,
				GroundSpeedKilometersPerHour,
				GroundSpeedKilometersPerHourUnit
			);

			if (ModeIndicator != null && ModeIndicator.IsSet())
			{
				start += $",{ModeIndicator}";
			}

			return $"{start}*{Checksum}";
		}

		#endregion
	}
}