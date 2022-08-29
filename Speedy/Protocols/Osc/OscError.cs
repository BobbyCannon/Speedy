#region References

using System;
using System.ComponentModel;
using System.Text;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscError : OscPacket
	{
		#region Constructors

		public OscError(DateTime time, Message message, params object[] arguments)
		{
			Code = message;
			Description = message.GetDescription(arguments);
			Time = time;
		}

		#endregion

		#region Properties

		public Message Code { get; }

		public string Description { get; }

		#endregion

		#region Methods

		public override byte[] ToByteArray()
		{
			return Encoding.UTF8.GetBytes(Description);
		}

		public override string ToString()
		{
			return Description;
		}

		#endregion

		#region Enumerations

		public enum Message
		{
			[Description("Failed to parse arguments. %s")]
			FailedParsingArguments,

			[Description("Not a valid bundle.")]
			InvalidBundle,

			[Description("Invalid CRC for the extended bundle.")]
			InvalidBundleCrc,

			[Description("Invalid bundle indent. %s")]
			InvalidBundleIdent,

			[Description("Invalid bundle start.")]
			InvalidBundleStart,

			[Description("Invalid message address.")]
			InvalidMessageAddress,

			[Description("Misaligned OSC Packet data. Address string is not padded correctly and does not align to 4 byte interval.")]
			InvalidMessageAddressMisAligned,

			[Description("Invalid message address. The address was empty.")]
			InvalidMessageAddressWasEmpty,

			[Description("Invalid parsed message.")]
			InvalidParsedMessage,

			[Description("Missing '{{'. Found '%s'")]
			InvalidParsedMessageArray,

			[Description("The provided string is null or empty.")]
			InvalidParseOscPacketInput,

			[Description("OSC type tag '%s' is unknown.")]
			UnknownTagType,

			[Description("OSC does not support nested arrays")]
			UnsupportedNestedArrays
		}

		#endregion
	}
}