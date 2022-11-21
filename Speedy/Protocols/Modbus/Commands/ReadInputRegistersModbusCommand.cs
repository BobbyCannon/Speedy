namespace Speedy.Protocols.Modbus.Commands;

/// <summary>
/// Read the binary contents of input registers in the slave.
/// </summary>
public class ReadInputRegistersModbusCommand : ModbusCommand
{
	#region Constructors

	/// <summary>
	/// Instantiates an instance of the modbus command.
	/// </summary>
	/// <param name="deviceAddress"> The address of the slave device. </param>
	/// <param name="startAddress"> The starting address of the registers. </param>
	/// <param name="quantityOfRegisters"> The quantity of registers to read. </param>
	public ReadInputRegistersModbusCommand(byte deviceAddress, ushort startAddress, ushort quantityOfRegisters)
		: base(deviceAddress, startAddress, ModbusFunctionCode.ReadInputRegisters)
	{
		QuantityOfRegisters = quantityOfRegisters;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The quantity of registers to read.
	/// </summary>
	public ushort QuantityOfRegisters { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override byte[] ToBuffer()
	{
		var buffer = new byte[8];
		buffer[0] = DeviceAddress;
		buffer[1] = (byte) FunctionCode;
		buffer[2] = ModbusHelper.High(StartAddress);
		buffer[3] = ModbusHelper.Low(StartAddress);
		buffer[4] = ModbusHelper.High(QuantityOfRegisters);
		buffer[5] = ModbusHelper.Low(QuantityOfRegisters);

		UpdateCrc(buffer, 0, 6);

		return buffer;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"[{ModbusFunctionCode.ReadInputRegisters}: Slave={DeviceAddress}, Address={StartAddress}, QuantityOfRegisters={QuantityOfRegisters}]";
	}

	#endregion
}