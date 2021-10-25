#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages
{
	/// <summary>
	/// Represents a GGA message.
	/// </summary>
	public class GgaMessage : NmeaMessage
	{
		#region Constructors

		public GgaMessage() : base(NmeaMessageType.GGA)
		{
		}

		#endregion

		#region Properties

		public double Altitude { get; set; }

		public string AltitudeUnit { get; set; }

		public string FixQuality { get; set; }

		public double HeightOfGeoid { get; set; }

		public string HeightOfGeoidUnit { get; set; }

		public double HorizontalDilutionOfPrecision { get; set; }

		public Location Latitude { get; set; }

		public Location Longitude { get; set; }

		public int NumberOfSatellites { get; set; }

		public string SecondsSinceLastUpdateDgps { get; set; }

		public string StationIdNumberDgps { get; set; }

		/// <summary>
		/// Time in the hhmmss.ss format.
		/// </summary>
		public double Time { get; set; }

		#endregion

		#region Methods

		public override void Parse(string sentence)
		{
			// $GNGGA,143718.00,4513.13793,N,01859.19704,E,1,05,1.86,108.1,M,38.1,M,,*40
			//
			// .      0         1       2 3        4 5 6  7   8   9 10 11 12 13   14
			//	      |         |       | |        | | |  |   |   | |   | |  |    |
			// $--GGA,hhmmss.ss,llll.ll,a,yyyyy.yy,a,x,xx,x.x,x.x,M,x.x,M,xx,xxxx*hh
			//
			//  0) Time (UTC) - hhmmss.ss
			//  1) Latitude - DDmm.mm
			//  2) Direction
			//     N - North
			//     S - South
			//  3) Longitude - DDDmm.mm
			//  4) Direction
			//     E - East
			//     W - West
			//  5) GPS Quality Indicator
			//     This is always a single number
			//     0 - Invalid, not fixed
			//     1 - Autonomous GPS fix, no correction data used
			//     2 - Differential GPS fix, OmniSTAR VBS
			//     3 - PPS
			//     4 - Real-Time Kinematic, fixed integers
			//     5 - Real-Time Kinematic, float integers, OmniSTAR XP/HP or Location RTK
			//     6 - Dead Reckoning Mode
			//     7 - Manual Input Mode
			//     8 - Simulation Mode
			//     9 - WAAS (SBAS)
			//  6) Number of satellites in view, 00 - 12
			//  7) Horizontal Dilution of precision
			//  8) Antenna Altitude above/below mean-sea-level (geoid)
			//  9) Units of antenna altitude
			//     M - Meters
			// 10) Geoidal separation, the difference between the WGS-84 earth ellipsoid and mean-sea-level (geoid), "-" means mean-sea-level below ellipsoid
			// 11) Units of geoidal separation
			//     M - Meters
			// 12) Age of differential GPS data - xx
			//     Time in seconds since last SC104 type 1 or 9 update, null field when DGPS is not used
			//     Empty when no differential data is present.
			// 13) Differential reference station ID - xxxx
			//     Empty when no differential data is present.
			// 14) Checksum

			StartParse(sentence);

			Time = Convert.ToDouble(GetArgument(0, "0"));
			Latitude = new Location(GetArgument(1), GetArgument(2));
			Longitude = new Location(GetArgument(3), GetArgument(4));
			FixQuality = GetArgument(5);
			NumberOfSatellites = Convert.ToInt32(GetArgument(6));
			HorizontalDilutionOfPrecision = Convert.ToDouble(GetArgument(7, "0"));
			Altitude = Convert.ToDouble(GetArgument(8, "0"));
			AltitudeUnit = GetArgument(9);
			HeightOfGeoid = Convert.ToDouble(GetArgument(10, "0"));
			HeightOfGeoidUnit = GetArgument(11);
			SecondsSinceLastUpdateDgps = GetArgument(12);
			StationIdNumberDgps = GetArgument(13);

			OnNmeaMessageParsed(this);
		}

		public override string ToString()
		{
			var start = string.Join(",",
				NmeaParser.GetSentenceStart(this),
				Time.ToString("000000.00"),
				Latitude.Degree,
				Latitude.Indicator,
				Longitude.Degree,
				Longitude.Indicator,
				FixQuality,
				NumberOfSatellites.ToString("00"),
				HorizontalDilutionOfPrecision,
				Altitude,
				AltitudeUnit,
				HeightOfGeoid.ToString("0.0"),
				HeightOfGeoidUnit,
				SecondsSinceLastUpdateDgps,
				StationIdNumberDgps
			);

			return $"{start}*{Checksum}";
		}

		#endregion
	}
}