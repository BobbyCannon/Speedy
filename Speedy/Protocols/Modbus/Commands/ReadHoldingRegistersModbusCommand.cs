namespace Speedy.Protocols.Modbus.Commands
{
	/// <summary>
	/// Reads the binary contents of holding registers in the slave.
	/// </summary>
	public class ReadHoldingRegistersModbusCommand : ModbusCommand
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the modbus command to Read Holding Registers (3).
		/// </summary>
		/// <param name="deviceAddress"> The address of the slave device. </param>
		/// <param name="startAddress"> The starting address of the register. </param>
		/// <param name="quantityOfRegisters"> The quantity of registers to read. </param>
		public ReadHoldingRegistersModbusCommand(byte deviceAddress, ushort startAddress, ushort quantityOfRegisters)
			: base(deviceAddress, startAddress, ModbusFunctionCode.ReadHoldingRegisters)
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
			return $"[{ModbusFunctionCode.ReadHoldingRegisters}: Slave={DeviceAddress}, Address={StartAddress}, Count={QuantityOfRegisters}]";
		}

		#endregion
	}
}