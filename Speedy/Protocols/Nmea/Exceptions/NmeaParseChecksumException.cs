#region References

using System;
using System.Runtime.Serialization;

#endregion

namespace Speedy.Protocols.Nmea.Exceptions;

/// <summary>
/// Represents a checksum error during parsing.
/// </summary>
[Serializable]
public class NmeaParseChecksumException : Exception
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	public NmeaParseChecksumException()
	{
	}

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	public NmeaParseChecksumException(string message) : base(message)
	{
	}

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	public NmeaParseChecksumException(string message, Exception inner) : base(message, inner)
	{
	}

	/// <summary>
	/// Instantiates an instance of the exception.
	/// </summary>
	protected NmeaParseChecksumException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}

	#endregion
}