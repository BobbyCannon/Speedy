#region References

using System;

#endregion

namespace Speedy.Protocols.Modbus.Commands;

/// <summary>
/// The response of the read input registers command.
/// </summary>
public class ReadInputRegistersModbusResponse : ModbusCommand
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the modbus response.
	/// </summary>
	public ReadInputRegistersModbusResponse()
		: base(0, 0, ModbusFunctionCode.ReadInputRegisters)
	{
	}

	#endregion

	#region Properties

	/// <summary>
	/// The quantity of registers to read.
	/// </summary>
	public ushort QuantityOfRegisters { get; protected set; }

	/// <summary>
	/// Register data that was read.
	/// </summary>
	public ushort[] Registers { get; private set; }

	#endregion

	#region Methods

	/// <summary>
	/// </summary>
	/// <param name="buffer"> The buffer containing the data. </param>
	/// <param name="offset"> The offset where the data starts. </param>
	/// <param name="length"> The length of the data. </param>
	public void FromBuffer(byte[] buffer, int offset, int length)
	{
		if (length < 6)
		{
			return;
		}

		if ((byte) FunctionCode != buffer[1])
		{
			return;
		}

		var crc = ModbusHelper.CalculateCrc(buffer, offset, length - 2);
		var crcH = ModbusHelper.High(crc);
		var crcL = ModbusHelper.Low(crc);

		if ((buffer[(offset + length) - 1] != crcH)
			|| (buffer[(offset + length) - 2] != crcL))
		{
			return;
		}

		DeviceAddress = buffer[0 + offset];
		QuantityOfRegisters = buffer[2];
		Registers = ModbusHelper.DecodeWords(buffer, offset + 3, QuantityOfRegisters);
	}

	/// <summary>
	/// Convert the offset to a decimal value.
	/// </summary>
	/// <param name="offset"> The offset of the data. </param>
	/// <param name="conversion"> The value of the conversion. Ex. 0.1 to convert 512 to 51.2 </param>
	/// <returns> The decimal value. </returns>
	public decimal GetDecimal(int offset, decimal conversion)
	{
		return Registers[offset] * conversion;
	}

	/// <summary>
	/// Convert the offset to a decimal value.
	/// </summary>
	/// <param name="offset"> The high offset of the data. </param>
	/// <param name="offset2"> The low offset of the data. </param>
	/// <param name="conversion"> The value of the conversion. Ex. 0.1 to convert 512 to 51.2 </param>
	/// <returns> The decimal value. </returns>
	public decimal GetDecimal(int offset, int offset2, decimal conversion)
	{
		return ModbusHelper.GetInt(Registers[offset], Registers[offset2]) * conversion;
	}

	/// <summary>
	/// Convert the offset and following value to an unsigned integer.
	/// </summary>
	/// <param name="offset"> The offset of the data. High ushort followed by the low ushort value. </param>
	/// <returns> The unsigned integer value. </returns>
	public uint GetUInt(int offset)
	{
		return ModbusHelper.GetUInt(Registers[offset], Registers[offset + 1]);
	}

	/// <inheritdoc />
	public override byte[] ToBuffer()
	{
		throw new NotImplementedException();
	}

	#endregion
}