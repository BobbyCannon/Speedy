#region References

using System;
using System.Globalization;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

/// <summary>
/// Time tags are represented by a 64 bit fixed point number. The first 32 bits specify the number of seconds since midnight on January 1, 1900, and
/// the last 32 bits specify fractional parts of a second to a precision of about 200 picoseconds. This is the representation used by Internet NTP
/// timestamps.The time tag value consisting of 63 zero bits followed by a one in the least significant bit is a special case meaning "immediately."
/// </summary>
public struct OscTimeTag : IOscArgument, IComparable<OscTimeTag>, IComparable, IEquatable<OscTimeTag>
{
	#region Constants

	public const string Name = "Time";

	#endregion

	#region Fields

	/// <summary>
	/// The maximum OSC date time for any OscTimeTag.
	/// </summary>
	public static readonly OscTimeTag MaxValue;

	/// <summary>
	/// The minimum date for any OscTimeTag.
	/// </summary>
	public static readonly DateTime MaxDateTime;

	/// <summary>
	/// The minimum date for any OscTimeTag.
	/// </summary>
	public static readonly DateTime MinDateTime;

	/// <summary>
	/// The minimum OSC date time for any OscTimeTag.
	/// </summary>
	public static readonly OscTimeTag MinValue;

	#endregion

	#region Constructors

	public OscTimeTag(ulong value)
	{
		Value = value;
	}

	public OscTimeTag(DateTime value)
		: this(FromDateTime(value).Value)
	{
	}

	static OscTimeTag()
	{
		MaxDateTime = new DateTime(2036, 2, 7, 6, 28, 16, 0, DateTimeKind.Utc);
		MaxValue = new OscTimeTag(0xffffffffffffffff);
		MinDateTime = new DateTime(1900, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		MinValue = new OscTimeTag(0);
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets a OscTimeTag object that is set to the current date and time on this computer, expressed as the local time.
	/// </summary>
	public static OscTimeTag Now => FromDateTime(TimeService.Now);

	/// <summary>
	/// Gets the number of seconds including fractional parts since midnight on January 1, 1900.
	/// </summary>
	public decimal PreciseValue => Seconds + (SubSeconds / (decimal) uint.MaxValue);

	/// <summary>
	/// Gets the number of seconds since midnight on January 1, 1900. This is the first 32 bits of the 64 bit fixed point OscTimeTag value.
	/// </summary>
	public uint Seconds => (uint) ((Value >> 32) & uint.MaxValue);

	/// <summary>
	/// Gets the fractional parts of a second. This is the 32 bits of the 64 bit fixed point OscTimeTag value.
	/// </summary>
	public uint SubSeconds => (uint) (Value & uint.MaxValue);

	/// <summary>
	/// Gets a OscTimeTag object that is set to the current date and time on this computer, expressed as the Coordinated Universal Time (UTC).
	/// </summary>
	public static OscTimeTag UtcNow => FromDateTime(TimeService.UtcNow);

	/// <summary>
	/// Gets or set the value of the tag.
	/// </summary>
	public ulong Value { get; set; }

	#endregion

	#region Methods

	/// <summary>
	/// Adds a timespan to this time tag.
	/// </summary>
	/// <param name="span"> The time span to be added. </param>
	/// <returns> The adjusted time. </returns>
	public OscTimeTag Add(TimeSpan span)
	{
		var dt = ToDateTime();
		return new OscTimeTag(dt.Add(span));
	}

	/// <summary>
	/// Adds milliseconds to this time tag.
	/// </summary>
	/// <param name="value"> The milliseconds to be added. </param>
	/// <returns> The adjusted time. </returns>
	public OscTimeTag AddMilliseconds(double value)
	{
		return Add(TimeSpan.FromMilliseconds(value));
	}

	/// <summary>
	/// Adds seconds to this time tag.
	/// </summary>
	/// <param name="value"> The seconds to be added. </param>
	/// <returns> The adjusted time. </returns>
	public OscTimeTag AddSeconds(double value)
	{
		return Add(TimeSpan.FromSeconds(value));
	}

	public int CompareTo(object obj)
	{
		return CompareTo((OscTimeTag) obj);
	}

	public int CompareTo(OscTimeTag other)
	{
		if (PreciseValue == other.PreciseValue)
		{
			return 0;
		}

		if (PreciseValue < other.PreciseValue)
		{
			return -1;
		}

		return 1;
	}

	public override bool Equals(object obj)
	{
		return obj switch
		{
			OscTimeTag tag => Value == tag.Value,
			ulong value => Value == value,
			_ => false
		};
	}

	public bool Equals(OscTimeTag other)
	{
		return PreciseValue == other.PreciseValue;
	}

	/// <summary>
	/// Get a OscTimeTag from a DateTime value.
	/// </summary>
	/// <param name="datetime"> DateTime value. </param>
	/// <returns> The equivalent value as an osc time tag. </returns>
	public static OscTimeTag FromDateTime(DateTime datetime)
	{
		if ((datetime <= DateTime.MinValue)
			|| (datetime <= MinDateTime)
			|| (datetime.ToUniversalTime() == DateTime.MinValue)
			|| (datetime.ToUniversalTime() == MinDateTime))
		{
			return MinValue;
		}

		if ((datetime >= DateTime.MaxValue)
			|| (datetime >= MaxDateTime)
			|| (datetime.ToUniversalTime() == DateTime.MaxValue)
			|| (datetime.ToUniversalTime() == MaxDateTime))
		{
			return MaxValue;
		}

		var udt = datetime.ToUniversalTime();
		var span = udt.Subtract(MinDateTime);
		return FromTimeSpan(span);
	}

	public static implicit operator DateTime(OscTimeTag value)
	{
		return value.ToDateTime();
	}

	public static OscTimeTag FromMilliseconds(float value)
	{
		var span = TimeSpan.FromMilliseconds(value);
		return FromTimeSpan(span);
	}

	/// <summary>
	/// Get a OscTimeTag from a TimeSpan value.
	/// </summary>
	/// <param name="span"> The span of time. </param>
	/// <returns> The equivalent value as an osc time tag. </returns>
	public static OscTimeTag FromTimeSpan(TimeSpan span)
	{
		var seconds = span.TotalSeconds;
		var secondsUInt = (uint) seconds;
		var remainingTicks = span.Ticks - ((decimal) secondsUInt * TimeSpan.TicksPerSecond);
		var fraction = (remainingTicks / TimeSpan.TicksPerMillisecond / 1000.0m) * uint.MaxValue;
		var preciseValue = ((ulong) (secondsUInt & 0xFFFFFFFF) << 32) | ((ulong) fraction & 0xFFFFFFFF);
		return new OscTimeTag(preciseValue);
	}

	public static OscTimeTag FromTicks(long value)
	{
		var span = TimeSpan.FromTicks(value);
		return FromTimeSpan(span);
	}

	public override int GetHashCode()
	{
		return (int) (((uint) (Value >> 32) + (uint) (Value & 0x00000000FFFFFFFF)) / 2);
	}

	public char GetOscBinaryType()
	{
		return 't';
	}

	public string GetOscStringType()
	{
		return Name;
	}

	public byte[] GetOscValueBytes()
	{
		return OscBitConverter.GetBytes(Value);
	}

	public string GetOscValueString()
	{
		return ToString();
	}

	public static OscTimeTag operator +(OscTimeTag a, TimeSpan b)
	{
		return new OscTimeTag(a.ToDateTime().Add(b));
	}

	public static bool operator ==(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue == b.PreciseValue;
	}

	public static bool operator >(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue > b.PreciseValue;
	}

	public static bool operator >=(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue >= b.PreciseValue;
	}

	public static bool operator !=(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue != b.PreciseValue;
	}

	public static bool operator <(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue < b.PreciseValue;
	}

	public static bool operator <=(OscTimeTag a, OscTimeTag b)
	{
		return a.PreciseValue <= b.PreciseValue;
	}

	public static OscTimeTag operator -(OscTimeTag a, TimeSpan b)
	{
		return new OscTimeTag(a.ToDateTime().Subtract(b));
	}

	public static TimeSpan operator -(OscTimeTag d1, OscTimeTag d2)
	{
		return d1.ToDateTime() - d2.ToDateTime();
	}

	public static OscTimeTag Parse(string value)
	{
		return Parse(value, CultureInfo.InvariantCulture);
	}

	public static OscTimeTag Parse(string value, IFormatProvider provider)
	{
		if (TryParse(value, provider, out var result))
		{
			return result;
		}

		throw new Exception($"Invalid OscTimeTag string \'{value}\'");
	}

	public void ParseOscValue(byte[] value, ref int index)
	{
		Value = BitConverter.ToUInt64(value, index);
		index += 8;
	}

	public void ParseOscValue(string value)
	{
		Value = Parse(value).Value;
	}

	/// <summary>
	/// Get the equivalent DateTime value from the OscTimeTag.
	/// </summary>
	/// <returns>
	/// The equivalent value as DateTime type.
	/// </returns>
	public DateTime ToDateTime()
	{
		var ticks1 = Math.Round(((decimal) SubSeconds / uint.MaxValue) * 1000, 4, MidpointRounding.AwayFromZero);
		var ticks2 = ticks1 * TimeSpan.TicksPerMillisecond;
		return MinDateTime.AddSeconds(Seconds).AddTicks((long) ticks2);
	}

	public double ToMilliseconds()
	{
		return ToDateTime().Subtract(MinDateTime).TotalMilliseconds;
	}

	public override string ToString()
	{
		return ToDateTime().ToUtcString();
	}

	public string ToString(string format)
	{
		return ToDateTime().ToString(format);
	}

	public TimeSpan ToTimeSpan()
	{
		return this < MinValue ? TimeSpan.Zero : this - MinValue;
	}

	public static bool TryParse(string value, out OscTimeTag result)
	{
		return TryParse(value, CultureInfo.InvariantCulture, out result);
	}

	public static bool TryParse(string value, IFormatProvider provider, out OscTimeTag result)
	{
		var style = DateTimeStyles.AssumeUniversal;

		if (value.Trim().EndsWith("Z"))
		{
			style = DateTimeStyles.AssumeUniversal;
			value = value.Trim().TrimEnd('Z');
		}

		// https://en.wikipedia.org/wiki/ISO_8601
		// yyyy = four-digit year
		// MM   = two-digit month (01=January, etc.)
		// dd   = two-digit day of month (01 through 31)
		// HH   = two digits of hour (00 through 23) (am/pm NOT allowed)
		// mm   = two digits of minute (00 through 59)
		// ss   = two digits of second (00 through 59)
		// f    = one or more digits representing a decimal fraction of a second
		// TZD  = time zone designator (Z or +hh:mm or -hh:mm)

		// Examples
		// Year: YYYY (eg 1997)
		// Year and month: YYYY-MM (eg 1997-07)
		// Complete date: YYYY-MM-DD (eg 1997-07-16)
		// Complete date plus hours and minutes:
		//		YYYY-MM-DDThh:mmTZD (eg 1997-07-16T19:20+01:00)
		// Complete date plus hours, minutes and seconds:
		//		YYYY-MM-DDThh:mm:ssTZD (eg 1997-07-16T19:20:30+01:00)
		// Complete date plus hours, minutes, seconds and a decimal fraction of a second
		//		YYYY-MM-DDThh:mm:ss.sTZD (eg 1997-07-16T19:20:30.45+01:00)
		string[] formats =
		{
			"yyyy",
			"yyyy-MM",
			"yyyy-MM-dd",
			"HH:mm",
			"HH:mm:ss",
			"HH:mm:ss.f",
			"HH:mm:ss.ff",
			"HH:mm:ss.fff",
			"HH:mm:ss.ffff",
			"HH:mm:ss.fffff",
			"HH:mm:ss.ffffff",
			"HH:mm:ss.fffffff",
			"yyyy-MM-ddTHH:mm",
			"yyyy-MM-ddTHH:mm:ss",
			"yyyy-MM-ddTHH:mm:ss.f",
			"yyyy-MM-ddTHH:mm:ss.ff",
			"yyyy-MM-ddTHH:mm:ss.fff",
			"yyyy-MM-ddTHH:mm:ss.ffff",
			"yyyy-MM-ddTHH:mm:ss.fffff",
			"yyyy-MM-ddTHH:mm:ss.ffffff",
			"yyyy-MM-ddTHH:mm:ss.fffffff"
		};

		if (DateTime.TryParseExact(value, formats, provider, style, out var datetime))
		{
			result = FromDateTime(datetime);
			return true;
		}

		if (value.StartsWith("0x") && ulong.TryParse(value.Substring(2), NumberStyles.HexNumber, provider, out var value64))
		{
			result = new OscTimeTag(value64);
			return true;
		}

		if (ulong.TryParse(value, NumberStyles.Integer, provider, out value64))
		{
			result = new OscTimeTag(value64);
			return true;
		}

		result = default;
		return false;
	}

	public static implicit operator OscTimeTag(DateTime value)
	{
		return new OscTimeTag(value);
	}

	public static implicit operator OscTimeTag(ulong value)
	{
		return new OscTimeTag(value);
	}

	#endregion
}