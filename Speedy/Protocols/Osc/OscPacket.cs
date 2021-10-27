#region References

using System;
using System.Globalization;
using System.Text;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public abstract class OscPacket : Bindable
	{
		#region Constructors

		protected OscPacket() : this(null)
		{
		}

		protected OscPacket(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the time of this bundle.
		/// </summary>
		public OscTimeTag Time { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="data"> The data for the message. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> Message containing various arguments and an address </returns>
		public static OscPacket Parse(byte[] data, params OscArgumentParser[] parsers)
		{
			return Parse(OscTimeTag.UtcNow, data, data.Length, parsers);
		}

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="time"> The created time of the message. </param>
		/// <param name="data"> The data for the message. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> Message containing various arguments and an address </returns>
		public static OscPacket Parse(OscTimeTag time, byte[] data, params OscArgumentParser[] parsers)
		{
			return Parse(time, data, data.Length, parsers);
		}

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="data"> The data for the message. </param>
		/// <param name="length"> The length for the message. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> Message containing various arguments and an address </returns>
		public static OscPacket Parse(byte[] data, int length, params OscArgumentParser[] parsers)
		{
			return Parse(OscTimeTag.UtcNow, data, length, parsers);
		}

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="time"> The created time of the message. </param>
		/// <param name="data"> The data for the message. </param>
		/// <param name="length"> The length for the message. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> Message containing various arguments and an address </returns>
		public static OscPacket Parse(OscTimeTag time, byte[] data, int length, params OscArgumentParser[] parsers)
		{
			try
			{
				if ((data[0] == '#') || (data[0] == '+'))
				{
					return OscBundle.ParseBundle(data, length);
				}

				return OscMessage.ParseMessage(time, data, length, parsers);
			}
			catch (Exception)
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidParsedMessage);
			}
		}

		/// <summary>
		/// Parse a packet from a string using the default provider InvariantCulture.
		/// </summary>
		/// <param name="value"> A string containing the OSC packet data. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> The parsed OSC packet. </returns>
		public static OscPacket Parse(string value, params OscArgumentParser[] parsers)
		{
			return Parse(value, CultureInfo.InvariantCulture, parsers);
		}

		/// <summary>
		/// Parse a packet from a string using the supplied provider.
		/// </summary>
		/// <param name="value"> A string containing the OSC packet data. </param>
		/// <param name="provider"> The format provider to use during parsing. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> The parsed OSC packet. </returns>
		public static OscPacket Parse(string value, IFormatProvider provider, params OscArgumentParser[] parsers)
		{
			return Parse(OscTimeTag.UtcNow, value, provider, parsers);
		}

		/// <summary>
		/// Parse a packet from a string using the supplied provider.
		/// </summary>
		/// <param name="time"> The time for the OscPacket. </param>
		/// <param name="value"> A string containing the OSC packet data. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> The parsed OSC packet. </returns>
		public static OscPacket Parse(OscTimeTag time, string value, params OscArgumentParser[] parsers)
		{
			return Parse(time, value, CultureInfo.InvariantCulture, parsers);
		}

		/// <summary>
		/// Parse a packet from a string using the supplied provider.
		/// </summary>
		/// <param name="time"> The time for the OscPacket. </param>
		/// <param name="value"> A string containing the OSC packet data. </param>
		/// <param name="provider"> The format provider to use during parsing. </param>
		/// <param name="parsers"> An optional set of OSC argument parsers. </param>
		/// <returns> The parsed OSC packet. </returns>
		public static OscPacket Parse(OscTimeTag time, string value, IFormatProvider provider, params OscArgumentParser[] parsers)
		{
			if (value.StartsWith("#") || value.StartsWith("+"))
			{
				return OscBundle.ParseBundle(value, provider);
			}

			return OscMessage.ParseMessage(time, value, provider, parsers);
		}

		public abstract byte[] ToByteArray();

		protected static string GetAddress(byte[] buffer, int index)
		{
			var i = index;
			var address = "";

			for (; i < buffer.Length; i += 4)
			{
				if (buffer[i] != ',')
				{
					continue;
				}

				if (i == 0)
				{
					return string.Empty;
				}

				address = Encoding.ASCII.GetString(buffer.SubArray(index, i - 1));
				break;
			}

			if ((i >= buffer.Length) && (address == null))
			{
				throw new Exception("no comma found");
			}

			return address.Replace("\0", "");
		}

		protected static char[] GetTypes(byte[] buffer, int index)
		{
			var i = index + 4;
			char[] types = null;

			for (; i <= buffer.Length; i += 4)
			{
				if (buffer[i - 1] == 0)
				{
					types = Encoding.ASCII.GetChars(buffer.SubArray(index, i - index));
					break;
				}
			}

			if ((i >= buffer.Length) && (types == null))
			{
				throw new Exception("No null terminator after type string");
			}

			return types;
		}

		#endregion
	}
}