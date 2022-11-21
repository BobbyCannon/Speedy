#region References

using System;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public struct OscCrc : IEquatable<OscCrc>
{
	#region Constructors

	public OscCrc(ushort crc)
	{
		Value = crc;
	}

	#endregion

	#region Properties

	public ushort Value { get; set; }

	#endregion

	#region Methods

	public override bool Equals(object obj)
	{
		switch (obj)
		{
			case OscCrc crc:
				return Equals(crc);

			case ushort value:
				return Value == value;

			default:
				return false;
		}
	}

	public bool Equals(OscCrc other)
	{
		return Value == other.Value;
	}

	public override int GetHashCode()
	{
		return Value.GetHashCode();
	}

	public static bool operator ==(OscCrc a, OscCrc b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(OscCrc a, OscCrc b)
	{
		return !a.Equals(b);
	}

	#endregion
}