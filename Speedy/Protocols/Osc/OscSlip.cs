#region References

using System.Collections.Generic;
using System.IO;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc;

public class OscSlip
{
	#region Fields

	private readonly byte[] _buffer;

	private readonly byte[] _packetBytes;

	#endregion

	#region Constructors

	public OscSlip(int bufferSize = 2048)
	{
		_buffer = new byte[bufferSize];
		_packetBytes = new byte[bufferSize];
	}

	#endregion

	#region Properties

	public int Count { get; private set; }

	#endregion

	#region Methods

	public static byte[] EncodePacket(OscPacket packet)
	{
		var data = packet.ToByteArray();
		return EncodePacket(data, 0, data.Length);
	}

	public static byte[] EncodePacket(byte[] bytes, int index, int count)
	{
		using (var stream = new MemoryStream(count))
		{
			for (var i = 0; i < count; i++)
			{
				switch (bytes[index + i])
				{
					case (byte) OscSlipBytes.End:
						stream.WriteByte((byte) OscSlipBytes.Escape);
						stream.WriteByte((byte) OscSlipBytes.EscapeEnd);
						break;

					case (byte) OscSlipBytes.Escape:
						stream.WriteByte((byte) OscSlipBytes.Escape);
						stream.WriteByte((byte) OscSlipBytes.EscapeEscape);
						break;

					default:
						stream.WriteByte(bytes[index + i]);
						break;
				}
			}

			stream.WriteByte((byte) OscSlipBytes.End);

			return stream.ToArray();
		}
	}

	public OscPacket ProcessByte(byte b)
	{
		// Add byte to buffer
		_buffer[Count] = b;

		// Increment index with overflow
		if (++Count >= _buffer.Length)
		{
			Count = 0;
		}

		// Decode packet if END byte
		if (b == (byte) OscSlipBytes.End)
		{
			Count = 0;

			var packetLength = DecodePacket();
			if (packetLength > 0)
			{
				return OscPacket.Parse(_packetBytes, packetLength);
			}
		}

		return null;
	}

	public OscPacket ProcessBytes(byte[] bytes, ref int start, int count)
	{
		for (var i = 0; i < count; i++)
		{
			var b = bytes[start + i];
			var oscPacket = ProcessByte(b);
			if (oscPacket != null)
			{
				start += i;
				return oscPacket;
			}
		}

		return null;
	}

	public IEnumerable<OscPacket> ProcessStream(Stream stream)
	{
		if (stream.CanSeek)
		{
			stream.Position = 0;
		}

		while (stream.Position < stream.Length)
		{
			var packet = ProcessByte((byte) stream.ReadByte());
			if (packet != null)
			{
				yield return packet;
			}
		}
	}

	public void Reset()
	{
		Count = 0;
	}

	private int DecodePacket()
	{
		var i = 0;
		var packetLength = 0;

		while (_buffer[i] != (byte) OscSlipBytes.End)
		{
			if (_buffer[i] == (byte) OscSlipBytes.Escape)
			{
				switch (_buffer[++i])
				{
					case (byte) OscSlipBytes.EscapeEnd:
						_packetBytes[packetLength++] = (byte) OscSlipBytes.End;
						break;

					case (byte) OscSlipBytes.EscapeEscape:
						_packetBytes[packetLength++] = (byte) OscSlipBytes.Escape;
						break;

					default:
						return 0; // error: unexpected byte value
				}
			}
			else
			{
				_packetBytes[packetLength++] = _buffer[i];
			}

			if (packetLength > _packetBytes.Length)
			{
				return 0; // error: decoded packet too large
			}

			i++;
		}

		return packetLength;
	}

	#endregion
}