#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// Represents a RMC message.
/// </summary>
public sealed class RmcMessage : NmeaMessage
{
	#region Constructors

	public RmcMessage() : base(NmeaMessageType.RMC)
	{
	}

	#endregion

	#region Properties

	public string Course { get; set; }

	public string DateOfFix { get; set; }

	public NmeaLocation Latitude { get; set; }

	public NmeaLocation Longitude { get; set; }

	public string MagneticVariation { get; set; }

	public string MagneticVariationUnit { get; set; }

	public ModeIndicator ModeIndicator { get; set; }

	public NavigationStatus NavigationStatus { get; set; }

	public string Speed { get; set; }

	public string Status { get; set; }

	/// <summary>
	/// Time in the hhmmss.ss format.
	/// </summary>
	public string Time { get; set; }

	#endregion

	#region Methods

	public override void Parse(string sentence)
	{
		// $GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A*65
		//
		// .      0         1 2         3 4         5 6   7   8      9  10 1112
		//        |         | |         | |         | |   |   |      |   | | |
		// $--RMC,hhmmss.ss,A,ddmm.mmmm,N,ddmm.mmmm,W,x.x,x.x,ddmmyy,x.x,W,a*hh
		//
		//  0) Time (UTC) - hhmmss.ss
		//  1) Status
		//     A = Active
		//     V = Void
		//  2) Latitude - ddmm.mmmm
		//  3) Direction
		//     N - North
		//     S - South
		//  4) Longitude - DDDmm.mm
		//  5) Direction
		//     E - East
		//     W - West
		//  6) Speed over ground, knots
		//  7) Course over ground, degrees true
		//  8) Date, ddmmyy
		//  9) Magnetic Variation, degrees
		// 10) Direction
		//     E - East
		//     W - West
		// 11) Mode Indicator
		//     A = Autonomous
		//     D = Differential
		//     E - Estimated
		//     M - Manual
		//     N - No Fix
		//     P - Precise
		//     R - Real Time Kinematic
		//     S - Simulator
		// 12) Checksum

		StartParse(sentence);

		Time = GetArgument(0);
		Status = GetArgument(1);
		Latitude = new NmeaLocation(GetArgument(2), GetArgument(3));
		Longitude = new NmeaLocation(GetArgument(4), GetArgument(5));
		Speed = GetArgument(6);
		Course = GetArgument(7);
		DateOfFix = GetArgument(8);
		MagneticVariation = GetArgument(9);
		MagneticVariationUnit = GetArgument(10);

		// Optional Mode
		ModeIndicator = Arguments.Count >= 12
			? new ModeIndicator(GetArgument(11))
			: null;

		// Optional Navigation Status
		NavigationStatus = Arguments.Count >= 13
			? new NavigationStatus(GetArgument(12))
			: null;

		OnNmeaMessageParsed(this);
	}

	public override string ToString()
	{
		var start = string.Join(",",
			NmeaParser.GetSentenceStart(this),
			Time,
			Status,
			Latitude.Degree,
			Latitude.Indicator,
			Longitude.Degree,
			Longitude.Indicator,
			Speed,
			Course,
			DateOfFix,
			MagneticVariation,
			MagneticVariationUnit
		);

		if ((ModeIndicator != null) && ModeIndicator.IsSet())
		{
			start += $",{ModeIndicator}";
		}

		if ((NavigationStatus != null) && NavigationStatus.IsSet())
		{
			start += $",{NavigationStatus}";
		}

		return $"{start}*{Checksum}";
	}

	#endregion
}