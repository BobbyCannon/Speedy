#region References

using System;
using System.Globalization;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public struct OscMidi : IOscArgument, IEquatable<OscMidi>
{
	#region Constants

	public const string Name = "Midi";

	#endregion

	#region Constructors

	public OscMidi(uint value) : this(OscBitConverter.GetBytes(value))
	{
	}

	public OscMidi(byte port, byte status, byte data1, byte data2)
	{
		Port = port;
		Status = status;
		Data1 = data1;
		Data2 = data2;
	}

	public OscMidi(params byte[] values)
	{
		if (values.Length < 4)
		{
			throw new ArgumentOutOfRangeException(nameof(values), "Not enough data");
		}

		Port = values[0];
		Status = values[1];
		Data1 = values[2];
		Data2 = values[3];
	}

	#endregion

	#region Properties

	public byte Data1 { get; set; }

	public byte Data2 { get; set; }

	public byte Port { get; set; }

	public byte Status { get; set; }

	#endregion

	#region Methods

	public override bool Equals(object obj)
	{
		switch (obj)
		{
			case OscMidi midi:
				return Equals(midi);

			case byte[] bytes:
				if (bytes.Length < 4)
				{
					return false;
				}
				return (Port == bytes[0])
					&& (Status == bytes[1])
					&& (Data1 == bytes[2])
					&& (Data2 == bytes[3]);

			default:
				return false;
		}
	}

	public bool Equals(OscMidi other)
	{
		return (Data1 == other.Data1) && (Data2 == other.Data2) && (Port == other.Port) && (Status == other.Status);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = Port.GetHashCode();
			hashCode = (hashCode * 397) ^ Status.GetHashCode();
			hashCode = (hashCode * 397) ^ Data1.GetHashCode();
			hashCode = (hashCode * 397) ^ Data2.GetHashCode();
			return hashCode;
		}
	}

	public char GetOscBinaryType()
	{
		return 'm';
	}

	public string GetOscStringType()
	{
		return Name;
	}

	public byte[] GetOscValueBytes()
	{
		return new[] { Port, Status, Data1, Data2 };
	}

	public string GetOscValueString()
	{
		return $"{Port},{Status},{Data1},{Data2}";
	}

	public static bool operator ==(OscMidi a, OscMidi b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(OscMidi a, OscMidi b)
	{
		return !a.Equals(b);
	}

	public static OscMidi Parse(string value)
	{
		return Parse(value, CultureInfo.InvariantCulture);
	}

	public static OscMidi Parse(string value, IFormatProvider provider)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new Exception($"Not a midi message '{value}'");
		}

		var parts = value.Split(',');

		if (parts.Length < 4)
		{
			throw new Exception($"Not a midi message '{value}'");
		}

		var index = 0;
		var port = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);
		var status = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);
		var data1 = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);
		var data2 = byte.Parse(parts[index++].Trim(), NumberStyles.Integer, provider);

		if (data1 > 0x7F)
		{
			throw new ArgumentOutOfRangeException(nameof(data1));
		}

		if (data2 > 0x7F)
		{
			throw new ArgumentOutOfRangeException(nameof(data2));
		}

		if (index != parts.Length)
		{
			throw new Exception($"Not a midi message '{value}'");
		}

		return new OscMidi(port, status, data1, data2);
	}

	public void ParseOscValue(byte[] value, ref int index)
	{
		if (value.Length <= (index + 3))
		{
			throw new IndexOutOfRangeException();
		}

		Port = value[index++];
		Status = value[index++];
		Data1 = value[index++];
		Data2 = value[index++];
	}

	public void ParseOscValue(string value)
	{
		var midi = Parse(value);
		Port = midi.Port;
		Status = midi.Status;
		Data1 = midi.Data1;
		Data2 = midi.Data2;
	}

	public override string ToString()
	{
		return $"{Name}: {Port},{Status},{Data1},{Data2}";
	}

	#endregion
}