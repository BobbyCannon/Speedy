#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Samples
{
	public struct SampleCustomValue : IOscArgument, IEquatable<SampleCustomValue>
	{
		#region Constructors

		public SampleCustomValue(byte start, byte end, int volume)
		{
			Start = start;
			End = end;
			Volume = volume;
		}

		#endregion

		#region Properties

		public byte End { get; set; }

		public byte Start { get; set; }

		public int Volume { get; set; }

		#endregion

		#region Methods

		public override bool Equals(object obj)
		{
			switch (obj)
			{
				case SampleCustomValue value:
					return (Start == value.Start)
						&& (End == value.End)
						&& (Volume == value.Volume);

				default:
					return false;
			}
		}

		public bool Equals(SampleCustomValue other)
		{
			return (End == other.End) && (Start == other.Start) && (Volume == other.Volume);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = End.GetHashCode();
				hashCode = (hashCode * 397) ^ Start.GetHashCode();
				hashCode = (hashCode * 397) ^ Volume;
				return hashCode;
			}
		}

		public char GetOscBinaryType()
		{
			return 'a';
		}

		public string GetOscStringType()
		{
			return "SampleValue";
		}

		public byte[] GetOscValueBytes()
		{
			var response = new List<byte>();
			response.AddRange(OscBitConverter.GetBytes(Start));
			response.AddRange(OscBitConverter.GetBytes(End));
			response.AddRange(OscBitConverter.GetBytes(Volume));
			return response.ToArray();
		}

		public string GetOscValueString()
		{
			return $"{Start},{End},{Volume}";
		}

		public static bool operator ==(SampleCustomValue a, SampleCustomValue b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(SampleCustomValue a, SampleCustomValue b)
		{
			return !a.Equals(b);
		}

		public static SampleCustomValue Parse(string value)
		{
			return Parse(value, CultureInfo.InvariantCulture);
		}

		public static SampleCustomValue Parse(string value, IFormatProvider provider)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new Exception($"Not a sample value '{value}'");
			}

			var parts = value.Split(',');

			if (parts.Length < 3)
			{
				throw new Exception($"Not a sample value '{value}'");
			}

			var index = 0;
			var start = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);
			var end = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);
			var volume = int.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);

			if (index != parts.Length)
			{
				throw new Exception($"Not a sample value '{value}'");
			}

			return new SampleCustomValue(start, end, volume);
		}

		public void ParseOscValue(byte[] value, ref int index)
		{
			Start = End = OscBitConverter.ToByte(value, index);
			End = OscBitConverter.ToByte(value, index + 4);
			Volume = OscBitConverter.ToInt32(value, index + 8);
			index += 12;
		}

		public void ParseOscValue(string value)
		{
			var argument = Parse(value);
			Start = argument.Start;
			End = argument.End;
			Volume = argument.Volume;
		}

		#endregion
	}
}