#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// GP: GlobalPositioningSystem
/// To provide information specific to the GPS constellation when more than one constellation is used
/// for the differential position fix.
/// GL: GlonassReceiver
/// To provide information specific to the GLONASS constellation when more than one constellation is used
/// for the differential position fix
/// GN: GlobalNavigationSatelliteSystem
/// GNSS position fix from more than one constellation (eg. GPS + GLONASS)
/// </summary>
/// <remarks>
/// Priority should be GN > GL > GP.
/// </remarks>
public class GnsMessage : NmeaMessage
{
	#region Constructors

	public GnsMessage() : base(NmeaMessageType.GNS)
	{
	}

	#endregion

	#region Properties

	public string AgeOfDifferentialData { get; set; }

	public double Altitude { get; set; }

	public string DifferentialReferenceStationId { get; set; }

	public double HeightOfGeoid { get; set; }

	public double HorizontalDilutionOfPrecision { get; set; }

	public NmeaLocation Latitude { get; set; }

	public NmeaLocation Longitude { get; set; }

	public string ModeIndicator { get; set; }

	public int NumberOfSatellites { get; set; }

	public double Time { get; set; }

	#endregion

	#region Methods

	public override void Parse(string sentence)
	{
		// $GNGNS,014035.00,4332.69262,S,17235.48549,E,RR,13,0.9,25.63,11.24,,*70
		// $GPGNS,002958.00,4253.65201,N,07852.11903,W,DA,14,0.76,253.1,-35.4,,0000*4B
		//
		// .      0         1       2 3        4 5 6  7   8   9   10 11   12
		//        |         |       | |        | | |  |   |   |   |  |    |
		// $--GNS,hhmmss.ss,llll.ll,a,yyyyy.yy,a,x,xx,x.x,x.x,x.x,xx,xxxx*hh
		//
		//  0) Time (UTC) - hhmmss.ss
		//  1) Latitude
		//  2) Direction of latitude:
		//     N: North
		//     S: South
		//  3) Longitude
		//  4) Direction of longitude:
		//     E: East
		//     W: West
		//  5) Mode indicator:
		//     * Variable character field with one character for each supported constellation.
		//     * First character is for GPS
		//     * Second character is for GLONASS
		//     * Subsequent characters will be added for new constellation
		//
		//     Each character will be one of the following:
		//     A = Autonomous. Satellite system used in non-differential mode in position fix
		// 	   D = Differential (including all OmniSTAR services). Satellite system used in differential mode in position fix
		//     E = Estimated (dead reckoning) Mode
		//     F = Float RTK. Satellite system used in real time kinematic mode with floating integers
		//     M = Manual Input Mode
		//     N = No fix. Satellite system not used in position fix, or fix not valid
		//     P = Precise. Satellite system used in precision mode. Precision mode is defined as: no deliberate degradation (such as Selective Availability) and higher resolution code (P-code) is used to compute position fix
		//     R = Real Time Kinematic. Satellite system used in RTK mode with fixed integers
		//     S = Simulator Mode
		//  6) Number of Satellites in use,range 00–99
		//  7) HDOP calculated using all the satellites (GPS, GLONASS, and any future satellites) used in computing the solution reported in each GNS sentence.
		//  8) Antenna Altitude above/below mean-sea-level (geoid)
		//  9) Geoidal separation, the difference between the WGS-84 earth ellipsoid and mean-sea-level (geoid), "-" means mean-sea-level below ellipsoid
		// 10) Age of differential data - Null if talker ID is GN, additional GNS messages follow with GP and/or GL Age of differential data
		// 11) Reference station ID1, range 0000-4095
		//     - Null if talker ID is GN, additional GNS messages follow with GP and/or GL Reference station ID
		// 12) Checksum

		StartParse(sentence);

		Time = Convert.ToDouble(GetArgument(0, "0"));
		Latitude = new NmeaLocation(GetArgument(1), GetArgument(2));
		Longitude = new NmeaLocation(GetArgument(3), GetArgument(4));
		ModeIndicator = GetArgument(5);
		NumberOfSatellites = Convert.ToInt32(GetArgument(6));
		HorizontalDilutionOfPrecision = Convert.ToDouble(GetArgument(7, "0"));
		Altitude = Convert.ToDouble(GetArgument(8, "0"));
		HeightOfGeoid = Convert.ToDouble(GetArgument(9, "0"));
		AgeOfDifferentialData = GetArgument(10);
		DifferentialReferenceStationId = GetArgument(11);

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
			ModeIndicator,
			NumberOfSatellites,
			HorizontalDilutionOfPrecision,
			Altitude.ToString("0.0#"),
			HeightOfGeoid,
			AgeOfDifferentialData,
			DifferentialReferenceStationId
		);

		return $"{start}*{Checksum}";
	}

	#endregion
}