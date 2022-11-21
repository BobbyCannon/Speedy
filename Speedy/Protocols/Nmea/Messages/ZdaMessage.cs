#pragma warning disable 1591

namespace Speedy.Protocols.Nmea.Messages;

/// <summary>
/// Represents a ZDA message.
/// </summary>
public class ZdaMessage : NmeaMessage
{
	#region Constructors

	public ZdaMessage() : base(NmeaMessageType.ZDA)
	{
	}

	#endregion

	#region Properties

	public int Day { get; set; }

	public int HourOffset { get; set; }

	public int MinuteOffset { get; set; }

	public int Month { get; set; }

	/// <summary>
	/// Time in the hhmmss.ss format.
	/// </summary>
	public string Time { get; set; }

	public int Year { get; set; }

	#endregion

	#region Methods

	public override void Parse(string sentence)
	{
		// $GPZDA,172809.45,12,07,1996,00,00*45
		//
		// .      0         1  2  3    4  5  6
		//        |         |  |  |    |  |  |
		// $--ZDA,hhmmss.ss,xx,xx,xxxx,xx,xx*hh
		//
		// 0) Time (UTC) - hhmmss.ss
		// 1) Day - xx - Range 01 to 31
		// 2) Month - xx - Range 01 to 12
		// 3) Year - xxxx
		// 4) Hour offset of local time from GMT - Range 00 to ±13
		// 5) Minute offset of local time from GMT - Range 00 to 59
		// 6) Checksum

		StartParse(sentence);

		Time = GetArgument(0);
		Day = GetArgumentAsInteger(1, 0);
		Month = GetArgumentAsInteger(2, 0);
		Year = GetArgumentAsInteger(3, 0);
		HourOffset = GetArgumentAsInteger(4, 0);
		MinuteOffset = GetArgumentAsInteger(5, 0);

		OnNmeaMessageParsed(this);
	}

	public override string ToString()
	{
		var start = string.Join(",",
			NmeaParser.GetSentenceStart(this),
			Time,
			Day.ToString("00"),
			Month.ToString("00"),
			Year.ToString("0000"),
			HourOffset.ToString("00"),
			MinuteOffset.ToString("00")
		);

		return $"{start}*{Checksum}";
	}

	#endregion
}