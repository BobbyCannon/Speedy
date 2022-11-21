#region References

using System.Text;

#endregion

namespace Speedy.Protocols.Modbus;

/// <summary>
/// A helper for calculating specific modbus type methods.
/// </summary>
public static class ModbusHelper
{
	#region Methods

	/// <summary>
	/// CRC calculation for RTU.
	/// </summary>
	/// <param name="buffer"> The buffer containing the data. </param>
	/// <returns> The CRC calculated for the data. </returns>
	public static ushort CalculateCrc(byte[] buffer)
	{
		return CalculateCrc(buffer, 0, buffer.Length);
	}

	/// <summary>
	/// CRC calculation for RTU.
	/// </summary>
	/// <param name="buffer"> The buffer containing the data. </param>
	/// <param name="offset"> The offset where the data starts. </param>
	/// <param name="length"> The length of the data. </param>
	/// <returns> The CRC calculated for the data. </returns>
	public static ushort CalculateCrc(byte[] buffer, int offset, int length)
	{
		ushort crc = 0xFFFF;
		for (var pos = 0; pos < length; pos++)
		{
			crc ^= buffer[pos + offset];
			for (var i = 8; i > 0; i--)
			{
				if ((crc & 0x0001) != 0)
				{
					crc >>= 1;
					crc ^= 0xA001;
				}
				else
				{
					crc >>= 1;
				}
			}
		}
		return crc;
	}

	/// <summary>
	/// LRC calculation for ASCII.
	/// </summary>
	/// <param name="data"> The buffer containing the data. </param>
	/// <returns> The CRC calculated for the data. </returns>
	public static string CalculateLrc(string data)
	{
		ushort response = 0;
		var buffer = Encoding.UTF8.GetBytes(data);
		for (var i = 0; i < buffer.Length; i++)
		{
			response -= buffer[i];
		}
		return $"{response:X1}";
	}

	/// <summary>
	/// Converts the byte array to a ushort array.
	/// </summary>
	/// <param name="buffer"> The buffer containing the data. </param>
	/// <param name="offset"> The offset where the data starts. </param>
	/// <param name="length"> The length of the data. </param>
	/// <returns> The words for the buffer data. </returns>
	public static ushort[] DecodeWords(byte[] buffer, int offset, int length)
	{
		var results = new ushort[length];
		for (var i = 0; i < length; i++)
		{
			results[i] = (ushort) ((buffer[offset + (2 * i)] << 8) | buffer[offset + (2 * i) + 1]);
		}
		return results;
	}

	/// <summary>
	/// Converts two ushort values into a int value.
	/// </summary>
	/// <param name="uh"> The high ushort value of the int. </param>
	/// <param name="ul"> The high ushort value of the int. </param>
	/// <returns> The int value of the two ushort values. </returns>
	public static int GetInt(ushort uh, ushort ul)
	{
		return (uh << 16) | (ul & 0xFFFF);
	}

	/// <summary>
	/// Converts two ushort values into a uint value.
	/// </summary>
	/// <param name="uh"> The high ushort value of the uint. </param>
	/// <param name="ul"> The high ushort value of the uint. </param>
	/// <returns> The uint value of the two ushort values. </returns>
	public static uint GetUInt(ushort uh, ushort ul)
	{
		return (uint) (
			((uh << 16) & 0xFFFF0000)
			| (uint) (ul & 0xFFFF)
		);
	}

	/// <summary>
	/// The high value of the ushort value.
	/// </summary>
	/// <param name="value"> The ushort value. </param>
	/// <returns> The high value of the ushort value. </returns>
	public static byte High(ushort value)
	{
		return (byte) ((value >> 8) & 0xff);
	}

	/// <summary>
	/// The low value of the ushort value.
	/// </summary>
	/// <param name="value"> The ushort value. </param>
	/// <returns> The low value of the ushort value. </returns>
	public static byte Low(ushort value)
	{
		return (byte) ((value >> 0) & 0xff);
	}

	#endregion
}