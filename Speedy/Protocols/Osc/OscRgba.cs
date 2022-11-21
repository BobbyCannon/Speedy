#region References

using System;
using System.Globalization;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public struct OscRgba : IOscArgument, IEquatable<OscRgba>
{
	#region Constants

	public const string Name = "Color";

	#endregion

	#region Constructors

	public OscRgba(byte red, byte green, byte blue, byte alpha)
	{
		R = red;
		G = green;
		B = blue;
		A = alpha;
	}

	public OscRgba(params byte[] values)
	{
		R = values.Length >= 1 ? values[0] : (byte) 0;
		G = values.Length >= 2 ? values[1] : (byte) 0;
		B = values.Length >= 3 ? values[2] : (byte) 0;
		A = values.Length >= 4 ? values[3] : (byte) 0;
	}

	#endregion

	#region Properties

	public byte A { get; set; }

	public byte B { get; set; }

	public byte G { get; set; }

	public byte R { get; set; }

	#endregion

	#region Methods

	public override bool Equals(object obj)
	{
		switch (obj)
		{
			case OscRgba oscRgba:
				return Equals(oscRgba);

			case byte[] bytes:
				if (bytes.Length < 4)
				{
					return false;
				}

				return (R == bytes[0]) && (G == bytes[1]) && (B == bytes[2]) && (A == bytes[3]);

			default:
				return false;
		}
	}

	public bool Equals(OscRgba other)
	{
		return (A == other.A) && (B == other.B) && (G == other.G) && (R == other.R);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			var hashCode = R.GetHashCode();
			hashCode = (hashCode * 397) ^ G.GetHashCode();
			hashCode = (hashCode * 397) ^ B.GetHashCode();
			hashCode = (hashCode * 397) ^ A.GetHashCode();
			return hashCode;
		}
	}

	public char GetOscBinaryType()
	{
		return 'r';
	}

	public string GetOscStringType()
	{
		return Name;
	}

	public byte[] GetOscValueBytes()
	{
		return new[] { R, G, B, A };
	}

	public string GetOscValueString()
	{
		return $"{R},{G},{B},{A}";
	}

	public static bool operator ==(OscRgba a, OscRgba b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(OscRgba a, OscRgba b)
	{
		return !a.Equals(b);
	}

	public static OscRgba Parse(params byte[] value)
	{
		if (value.Length != 4)
		{
			throw new Exception($"Invalid color \'{value}\'");
		}

		return new OscRgba(value);
	}

	public static OscRgba Parse(string value)
	{
		return Parse(value, CultureInfo.InvariantCulture);
	}

	public static OscRgba Parse(string value, IFormatProvider provider)
	{
		var pieces = value.Split(',');

		if (pieces.Length != 4)
		{
			throw new Exception($"Invalid color \'{value}\'");
		}

		var r = byte.Parse(pieces[0].Trim(), NumberStyles.None, provider);
		var g = byte.Parse(pieces[1].Trim(), NumberStyles.None, provider);
		var b = byte.Parse(pieces[2].Trim(), NumberStyles.None, provider);
		var a = byte.Parse(pieces[3].Trim(), NumberStyles.None, provider);

		return new OscRgba(r, g, b, a);
	}

	public void ParseOscValue(byte[] value, ref int index)
	{
		R = value[index++];
		G = value[index++];
		B = value[index++];
		A = value[index++];
	}

	public void ParseOscValue(string value)
	{
		var rgba = Parse(value);
		R = rgba.R;
		G = rgba.G;
		B = rgba.B;
		A = rgba.A;
	}

	public override string ToString()
	{
		return $"{Name}: {R},{G},{B},{A}";
	}

	#endregion
}