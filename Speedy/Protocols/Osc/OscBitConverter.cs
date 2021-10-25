#region References

using System;
using System.Text;
using Speedy.Extensions;

#endregion

#pragma warning disable 1591

namespace Speedy.Protocols.Osc
{
	public static class OscBitConverter
	{
		#region Methods

		/// <summary>
		/// Get bytes ensuring the byte array is the correct OSC length.
		/// </summary>
		/// <param name="value"> The value to get bytes for. </param>
		/// <returns> The correctly length byte array value. </returns>
		public static byte[] GetBytes(byte[] value)
		{
			// Calculate the correct length (size + bytes)
			var len = 4 + value.Length;
			if (len % 4 > 0)
			{
				len += 4 - len % 4;
			}

			var buffer = new byte[len];
			var size = GetBytes(value.Length);
			size.CopyTo(buffer, 0);
			value.CopyTo(buffer, 4);
			return buffer;
		}

		/// <summary>
		/// A char still is expected to be 4 bytes long due to OSC byte structure.
		/// </summary>
		/// <param name="value"> The value of the char. </param>
		/// <returns> The OSC bytes for the value. </returns>
		public static byte[] GetBytes(char value)
		{
			var output = new byte[4];
			output[0] = 0;
			output[1] = 0;
			output[2] = 0;
			output[3] = (byte) value;
			return output;
		}

		public static byte[] GetBytes(OscCrc crc)
		{
			return BitConverter.GetBytes(crc.Value);
		}

		public static byte[] GetBytes(double value)
		{
			var rev = BitConverter.GetBytes(value);
			var output = new byte[8];
			output[0] = rev[7];
			output[1] = rev[6];
			output[2] = rev[5];
			output[3] = rev[4];
			output[4] = rev[3];
			output[5] = rev[2];
			output[6] = rev[1];
			output[7] = rev[0];
			return output;
		}

		public static byte[] GetBytes(float value)
		{
			var buffer = new byte[4];
			var bytes = BitConverter.GetBytes(value);
			buffer[0] = bytes[3];
			buffer[1] = bytes[2];
			buffer[2] = bytes[1];
			buffer[3] = bytes[0];
			return buffer;
		}

		public static byte[] GetBytes(int value)
		{
			var buffer = new byte[4];
			var bytes = BitConverter.GetBytes(value);
			buffer[0] = bytes[3];
			buffer[1] = bytes[2];
			buffer[2] = bytes[1];
			buffer[3] = bytes[0];
			return buffer;
		}

		public static byte[] GetBytes(long value)
		{
			var rev = BitConverter.GetBytes(value);
			var output = new byte[8];
			output[0] = rev[7];
			output[1] = rev[6];
			output[2] = rev[5];
			output[3] = rev[4];
			output[4] = rev[3];
			output[5] = rev[2];
			output[6] = rev[1];
			output[7] = rev[0];
			return output;
		}

		public static byte[] GetBytes(string value)
		{
			// Make sure we have room for the null terminator
			var len = value.Length + (4 - value.Length % 4);
			if (len <= value.Length)
			{
				len += 4;
			}

			var buffer = new byte[len];
			var bytes = Encoding.ASCII.GetBytes(value);

			bytes.CopyTo(buffer, 0);

			return buffer;
		}

		public static byte[] GetBytes(uint value)
		{
			var buffer = new byte[4];
			var bytes = BitConverter.GetBytes(value);
			buffer[0] = bytes[3];
			buffer[1] = bytes[2];
			buffer[2] = bytes[1];
			buffer[3] = bytes[0];
			return buffer;
		}

		public static byte[] GetBytes(ulong value)
		{
			var rev = BitConverter.GetBytes(value);
			var output = new byte[8];
			output[0] = rev[7];
			output[1] = rev[6];
			output[2] = rev[5];
			output[3] = rev[4];
			output[4] = rev[3];
			output[5] = rev[2];
			output[6] = rev[1];
			output[7] = rev[0];
			return output;
		}

		public static byte[] SetOscType<T>(T oscType) where T : IOscArgument
		{
			return oscType.GetOscValueBytes();
		}

		public static byte[] ToBlob(byte[] buffer, int index)
		{
			var size = ToInt32(buffer, index);
			return buffer.SubArray(index + 4, size);
		}

		public static byte ToByte(byte[] buffer, int index)
		{
			return buffer[index + 3];
		}

		public static char ToChar(byte[] buffer, int index)
		{
			return (char) buffer[index + 3];
		}

		public static OscCrc ToCrc(byte[] buffer, int index)
		{
			return new OscCrc(BitConverter.ToUInt16(buffer, index));
		}

		public static double ToDouble(byte[] buffer, int index)
		{
			var var = new byte[8];
			var[7] = buffer[index];
			var[6] = buffer[index + 1];
			var[5] = buffer[index + 2];
			var[4] = buffer[index + 3];
			var[3] = buffer[index + 4];
			var[2] = buffer[index + 5];
			var[1] = buffer[index + 6];
			var[0] = buffer[index + 7];
			return BitConverter.ToDouble(var, 0);
		}

		public static float ToFloat(byte[] buffer, int index)
		{
			var reversed = new byte[4];
			reversed[3] = buffer[index];
			reversed[2] = buffer[index + 1];
			reversed[1] = buffer[index + 2];
			reversed[0] = buffer[index + 3];
			return BitConverter.ToSingle(reversed, 0);
		}

		public static int ToInt32(byte[] buffer, int index)
		{
			return (buffer[index] << 24)
				+ (buffer[index + 1] << 16)
				+ (buffer[index + 2] << 8)
				+ buffer[index + 3];
		}

		public static long ToInt64(byte[] buffer, int index)
		{
			var var = new byte[8];
			var[7] = buffer[index];
			var[6] = buffer[index + 1];
			var[5] = buffer[index + 2];
			var[4] = buffer[index + 3];
			var[3] = buffer[index + 4];
			var[2] = buffer[index + 5];
			var[1] = buffer[index + 6];
			var[0] = buffer[index + 7];
			return BitConverter.ToInt64(var, 0);
		}

		public static T ToOscType<T>(byte[] buffer, ref int index) where T : IOscArgument, new()
		{
			var response = new T();
			response.ParseOscValue(buffer, ref index);
			return response;
		}

		public static string ToString(byte[] buffer, ref int index)
		{
			string output = null;
			var i = index + 4;

			for (; i - 1 < buffer.Length; i += 4)
			{
				if (buffer[i - 1] == 0)
				{
					output = Encoding.ASCII.GetString(buffer.SubArray(index, i - index));
					break;
				}
			}

			if (i >= buffer.Length && output == null)
			{
				throw new Exception("No null terminator after type string");
			}

			index = i;
			return output?.Replace("\0", "");
		}

		public static uint ToUInt32(byte[] buffer, int index)
		{
			return ((uint) buffer[index] << 24)
				+ ((uint) buffer[index + 1] << 16)
				+ ((uint) buffer[index + 2] << 8)
				+ buffer[index + 3];
		}

		public static ulong ToUInt64(byte[] buffer, int index)
		{
			return ((ulong) buffer[index] << 56)
				+ ((ulong) buffer[index + 1] << 48)
				+ ((ulong) buffer[index + 2] << 40)
				+ ((ulong) buffer[index + 3] << 32)
				+ ((ulong) buffer[index + 4] << 24)
				+ ((ulong) buffer[index + 5] << 16)
				+ ((ulong) buffer[index + 6] << 8)
				+ buffer[index + 7];
		}

		#endregion
	}
}