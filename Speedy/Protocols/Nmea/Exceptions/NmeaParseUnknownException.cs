#region References

using System;

#endregion

namespace Speedy.Protocols.Nmea.Exceptions
{
	/// <summary>
	/// Represents a unknown error during parsing.
	/// </summary>
	public class NmeaParseUnknownException : Exception
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public NmeaParseUnknownException()
		{
		}

		/// <summary>
		/// Instantiates an instance of the exception.
		/// </summary>
		public NmeaParseUnknownException(string message) : base(message)
		{
		}

		#endregion
	}
}