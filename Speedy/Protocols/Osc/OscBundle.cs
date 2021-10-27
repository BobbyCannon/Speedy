#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public class OscBundle : OscPacket, IEnumerable<OscPacket>
	{
		#region Fields

		private readonly byte[] _bundleBytes = { 0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00 };
		private readonly byte[] _extendedBundleBytes = { 0x2B, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00 };
		private readonly List<OscPacket> _packets;

		#endregion

		#region Constructors

		public OscBundle(params OscPacket[] packets)
			: this(OscTimeTag.UtcNow, packets)
		{
		}

		public OscBundle(ulong time, params OscPacket[] packets)
			: this(new OscTimeTag(time), packets)
		{
		}

		public OscBundle(DateTime dateTime, params OscPacket[] packets)
			: this(OscTimeTag.FromDateTime(dateTime), packets)
		{
		}

		public OscBundle(OscTimeTag timeTag, params OscPacket[] packets)
		{
			_packets = new List<OscPacket>();

			Time = timeTag;

			Add(packets);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The number of packets in the bundle.
		/// </summary>
		public int Count
		{
			get
			{
				lock (_packets)
				{
					return _packets.Count;
				}
			}
		}

		/// <summary>
		/// If true this is an extended message that contains a CRC.
		/// </summary>
		public bool IsExtended { get; set; }

		/// <summary>
		/// Access bundle messages by index
		/// </summary>
		/// <param name="index"> the index of the message </param>
		/// <returns> message at the supplied index </returns>
		public OscPacket this[int index]
		{
			get
			{
				lock (_packets)
				{
					return _packets[index];
				}
			}
		}

		#endregion

		#region Methods

		public void Add(params OscPacket[] packets)
		{
			lock (_packets)
			{
				_packets.AddRange(packets);

				foreach (var packet in _packets)
				{
					SetPacketTime(packet);
				}
			}
		}

		public void Clear()
		{
			lock (_packets)
			{
				_packets.Clear();
			}
		}

		public IEnumerator<OscPacket> GetEnumerator()
		{
			lock (_packets)
			{
				return _packets.GetEnumerator();
			}
		}

		public override byte[] ToByteArray()
		{
			var outMessages = new List<byte[]>();

			lock (_packets)
			{
				foreach (var packets in _packets)
				{
					outMessages.Add(packets.ToByteArray());
				}
			}

			// 16 (8 header string, 8 time tag)
			var total = 16 + outMessages.Sum(x => x.Length + 4) + (IsExtended ? 4 : 0);
			var response = new byte[total];
			var responseIndex = 0;

			// Add header string
			if (IsExtended)
			{
				_extendedBundleBytes.CopyTo(response, responseIndex);
				responseIndex += _extendedBundleBytes.Length;
			}
			else
			{
				_bundleBytes.CopyTo(response, responseIndex);
				responseIndex += _bundleBytes.Length;
			}

			// Add the time tag
			var time = OscBitConverter.GetBytes(Time.Value);
			time.CopyTo(response, responseIndex);
			responseIndex += time.Length;

			foreach (var msg in outMessages)
			{
				var size = OscBitConverter.GetBytes(msg.Length);
				size.CopyTo(response, responseIndex);
				responseIndex += size.Length;

				// msg size is always a multiple of 4
				msg.CopyTo(response, responseIndex);
				responseIndex += msg.Length;
			}

			if (IsExtended)
			{
				var crc = response.CalculateCrc16(total - 4);
				var data = BitConverter.GetBytes(crc);
				data.CopyTo(response, responseIndex);
			}

			return response;
		}

		/// <summary>
		/// Takes in an OSC bundle package in byte form and parses it into a more usable OscBundle object
		/// </summary>
		/// <param name="bundle"> The bundle data in byte format. </param>
		/// <param name="length"> The length of the data. </param>
		/// <returns> Bundle containing elements and a time tag </returns>
		internal static OscPacket ParseBundle(byte[] bundle, int length)
		{
			var index = 0;
			var messages = new List<OscPacket>();
			var bundleTag = Encoding.ASCII.GetString(bundle.SubArray(0, 8));
			index += 8;

			var time = OscBitConverter.ToUInt64(bundle, index);
			var timeTag = new OscTimeTag(time);
			index += 8;

			if ((bundleTag != "#bundle\0") && (bundleTag != "+bundle\0"))
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidBundle);
			}

			var isExtended = bundleTag == "+bundle\0";
			if (isExtended)
			{
				length -= 4;
			}

			while (index < length)
			{
				var size = OscBitConverter.ToInt32(bundle, index);
				index += 4;

				var messageBytes = bundle.SubArray(index, size);
				var packet = Parse(timeTag, messageBytes);
				if (packet is OscError error)
				{
					return error;
				}

				if (!(packet is OscMessage message))
				{
					// Should never get here but just in case
					return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidParsedMessage);
				}

				message.Time = timeTag;
				messages.Add(message);
				index += size;

				while ((index % 4) != 0)
				{
					index++;
				}
			}

			if (isExtended)
			{
				// Seems like we have a CRC
				var readCrc = BitConverter.ToUInt16(bundle, index);
				var calculatedCrc = bundle.CalculateCrc16(index);

				if (readCrc != calculatedCrc)
				{
					return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidBundleCrc);
				}
			}

			return new OscBundle(timeTag, messages.ToArray()) { IsExtended = isExtended };
		}

		/// <summary>
		/// Parse a bundle from a string using the supplied provider.
		/// </summary>
		/// <param name="value"> A string containing the OSC bundle data. </param>
		/// <param name="provider"> The format provider to use during parsing. </param>
		/// <returns> The parsed OSC bundle. </returns>
		internal static OscPacket ParseBundle(string value, IFormatProvider provider)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidParseOscPacketInput);
			}

			var start = 0;
			var end = value.IndexOf(',', start);

			if (end <= start)
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidBundleStart);
			}

			var ident = value.Substring(start, end - start).Trim();

			if (!"#bundle".Equals(ident, StringComparison.InvariantCulture) && !"+bundle".Equals(ident, StringComparison.InvariantCulture))
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidBundleIdent, ident);
			}

			start = end + 1;
			end = value.IndexOf(',', start);

			if (end < 0)
			{
				end = value.Length;
			}

			var timeStampValue = value.Substring(start, end - start);
			var timeStamp = OscTimeTag.Parse(timeStampValue.Trim(), provider);

			start = end + 1;

			if (start >= value.Length)
			{
				return new OscBundle(timeStamp);
			}

			end = value.IndexOf('{', start);

			if (end < 0)
			{
				end = value.Length;
			}

			var gap = value.Substring(start, end - start);

			if (string.IsNullOrWhiteSpace(gap) == false)
			{
				return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidParsedMessageArray, gap);
			}

			start = end;

			var messages = new List<OscPacket>();

			while ((start > 0) && (start < value.Length))
			{
				end = Extensions.ScanForwardObject(value, start);
				messages.Add(Parse(value.Substring(start + 1, end - (start + 1)).Trim(), provider));
				start = end + 1;
				end = value.IndexOf('{', start);

				if (end < 0)
				{
					end = value.Length;
				}

				gap = value.Substring(start, end - start).Trim();

				if ((gap.Equals(",") == false) && (string.IsNullOrWhiteSpace(gap) == false))
				{
					return new OscError(OscTimeTag.UtcNow, OscError.Message.InvalidParsedMessageArray, gap);
				}

				start = end;
			}

			return new OscBundle(timeStamp, messages.ToArray());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void SetPacketTime(OscPacket packet)
		{
			switch (packet)
			{
				case OscMessage message:
					message.Time = Time;
					break;

				case OscBundle bundle:
				{
					foreach (var bunglePacket in bundle)
					{
						SetPacketTime(bunglePacket);
					}
					break;
				}
			}
		}

		#endregion
	}
}