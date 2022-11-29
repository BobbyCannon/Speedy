#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a globally unique identifier (GUID) with a shorter string value.
/// </summary>
public struct ShortGuid
{
	#region Fields

	/// <summary>
	/// A read-only instance of the ShortGuid class whose value
	/// is guaranteed to be all zeroes.
	/// </summary>
	public static readonly ShortGuid Empty = new ShortGuid(Guid.Empty);

	#endregion

	#region Constructors

	/// <summary>
	/// Creates a ShortGuid.
	/// </summary>
	public ShortGuid()
	{
		Guid = Guid.NewGuid();
		Value = Encode(Guid);
	}

	/// <summary>
	/// Creates a ShortGuid from a base64 encoded string.
	/// </summary>
	/// <param name="value">
	/// The encoded guid as a base64 string.
	/// </param>
	public ShortGuid(string value)
	{
		Value = value;
		Guid = Decode(value);
	}

	/// <summary>
	/// Creates a ShortGuid from a Guid.
	/// </summary>
	/// <param name="guid"> The Guid to encode. </param>
	public ShortGuid(Guid guid)
	{
		Value = Encode(guid);
		Guid = guid;
	}

	#endregion

	#region Properties

	/// <summary>
	/// Gets/sets the underlying Guid.
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Gets/sets the underlying base64 encoded string.
	/// </summary>
	public string Value { get; set; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		return obj switch
		{
			ShortGuid sGuid => Guid.Equals(sGuid.Guid),
			Guid guid => Guid.Equals(guid),
			string sValue => Guid.Equals(new ShortGuid(sValue).Guid),
			_ => false
		};
	}

	/// <summary>
	/// Returns the HashCode for underlying Guid.
	/// </summary>
	/// <returns> </returns>
	public override int GetHashCode()
	{
		return Guid.GetHashCode();
	}

	/// <summary>
	/// Initializes a new instance of the ShortGuid class.
	/// </summary>
	/// <returns> </returns>
	public static ShortGuid NewGuid()
	{
		return new ShortGuid(Guid.NewGuid());
	}

	/// <summary>
	/// Determines if both ShortGuids have the same underlying.
	/// Guid value.
	/// </summary>
	/// <param name="x"> The guid to be tested. </param>
	/// <param name="y"> The guid to be tested against. </param>
	/// <returns> Returns true if the values match or otherwise false if they do not match. </returns>
	public static bool operator ==(ShortGuid x, ShortGuid y)
	{
		return x.Guid == y.Guid;
	}

	/// <summary>
	/// Implicitly converts the ShortGuid to it's string equivalent
	/// </summary>
	/// <param name="shortGuid"> </param>
	/// <returns> </returns>
	public static implicit operator string(ShortGuid shortGuid)
	{
		return shortGuid.Value;
	}

	/// <summary>
	/// Implicitly converts the ShortGuid to it's Guid equivalent
	/// </summary>
	/// <param name="shortGuid"> </param>
	/// <returns> </returns>
	public static implicit operator Guid(ShortGuid shortGuid)
	{
		return shortGuid.Guid;
	}

	/// <summary>
	/// Implicitly converts the string to a ShortGuid
	/// </summary>
	/// <param name="shortGuid"> </param>
	/// <returns> </returns>
	public static implicit operator ShortGuid(string shortGuid)
	{
		return new ShortGuid(shortGuid);
	}

	/// <summary>
	/// Implicitly converts the Guid to a ShortGuid
	/// </summary>
	/// <param name="guid"> </param>
	/// <returns> </returns>
	public static implicit operator ShortGuid(Guid guid)
	{
		return new ShortGuid(guid);
	}

	/// <summary>
	/// Determines if both ShortGuids do not have the
	/// same underlying Guid value.
	/// </summary>
	/// <param name="x"> The guid to be tested. </param>
	/// <param name="y"> The guid to be tested against. </param>
	/// <returns> Returns true if the values do not match or otherwise false if they do match. </returns>
	public static bool operator !=(ShortGuid x, ShortGuid y)
	{
		return !(x == y);
	}

	/// <summary>
	/// Creates a new short Guid from the provided full Guid.
	/// </summary>
	/// <param name="guid"> The guid to be parsed. </param>
	/// <returns> The short guid version of the full guid. </returns>
	public static ShortGuid ParseGuid(string guid)
	{
		return new ShortGuid(Guid.Parse(guid));
	}

	/// <summary>
	/// Creates a new short Guid from the provided short Guid string.
	/// </summary>
	/// <param name="sguid"> The short guid string to be parsed. </param>
	/// <returns> The short guid version of the short guid string. </returns>
	public static ShortGuid ParseShortGuid(string sguid)
	{
		return new ShortGuid(sguid);
	}

	/// <summary>
	/// Returns the base64 encoded guid as a string.
	/// </summary>
	/// <returns> </returns>
	public override string ToString()
	{
		return Value;
	}

	/// <summary>
	/// Decodes the given base64 string.
	/// </summary>
	/// <param name="value"> The base64 encoded string of a Guid. </param>
	/// <returns> A new Guid </returns>
	private static Guid Decode(string value)
	{
		// Reverting HTML URI characters
		//  '-' > '+' (Indicates a space) 
		//  '_' > '/' (Separate domain and directories)

		value = value.Replace("_", "/").Replace("-", "+");
		var buffer = Convert.FromBase64String(value + "==");
		return new Guid(buffer);
	}

	/// <summary>
	/// Encodes the given Guid as a base64 string that is 22.
	/// characters long.
	/// </summary>
	/// <param name="guid"> The Guid to encode. </param>
	/// <returns> </returns>
	private static string Encode(Guid guid)
	{
		var encoded = Convert.ToBase64String(guid.ToByteArray());

		// Changing HTML URI characters
		//  '+' (Indicates a space) to '-'
		//  '/' (Separate domain and directories) to '_'

		encoded = encoded.Replace("/", "_").Replace("+", "-");
		return encoded.Substring(0, 22);
	}

	#endregion
}