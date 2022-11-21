#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// Represents a GLL message.
/// </summary>
public class GllMessage : NmeaMessage
{
	#region Constructors

	public GllMessage() : base(NmeaMessageType.GLL)
	{
	}

	#endregion

	#region Properties

	public string DataValid { get; set; }

	public NmeaLocation Latitude { get; set; }

	public NmeaLocation Longitude { get; set; }

	public ModeIndicator ModeIndicator { get; set; }

	/// <summary>
	/// Time in the hhmmss.ss format.
	/// </summary>
	public double Time { get; set; }

	#endregion

	#region Methods

	public override void Parse(string sentence)
	{
		//        0          1 2           3 4         5 6 7   
		// $GNGLL,4513.13795,N,01859.19702,E,143717.00,A,A*72
		//
		// .      0       1 2        3 4         5 6
		//        |       | |        | |         | |
		// $--GLL,llll.ll,a,yyyyy.yy,a,hhmmss.ss,A*hh
		//
		// 0) Latitude - DDmm.mm
		// 1) Direction
		//    N - North
		//    S - South
		// 2) Longitude - DDDmm.mm
		// 3) Direction
		//    E - East
		//    W - West
		// 4) Time (UTC) - hhmmss.ss
		// 5) Status
		//    A - Data Valid
		//    V - Data Invalid
		// 6) Mode Indicator
		//    A = Autonomous
		//    D = Differential
		//    E - Estimated
		//    M - Manual
		//    N - No Fix
		//    P - Precise
		//    R - Real Time Kinematic
		//    S - Simulator
		// 7) Checksum

		StartParse(sentence);

		Latitude = new NmeaLocation(GetArgument(0), GetArgument(1));
		Longitude = new NmeaLocation(GetArgument(2), GetArgument(3));
		Time = Convert.ToDouble(GetArgument(4, "0"));
		DataValid = GetArgument(5);

		ModeIndicator = Arguments.Count > 6
			? new ModeIndicator(GetArgument(6))
			: null;

		OnNmeaMessageParsed(this);
	}

	public override string ToString()
	{
		var start = string.Join(",",
			NmeaParser.GetSentenceStart(this),
			Latitude.Degree,
			Latitude.Indicator,
			Longitude.Degree,
			Longitude.Indicator,
			Time.ToString("000000.00"),
			DataValid
		);

		if ((ModeIndicator != null) && ModeIndicator.IsSet())
		{
			start += $",{ModeIndicator}";
		}

		return $"{start}*{Checksum}";
	}

	#endregion
}