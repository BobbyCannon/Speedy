#region References

using System;

#endregion

namespace Speedy.Protocols.Modbus.Commands
{
	/// <summary>
	/// Represents a command for Modbus
	/// </summary>
	public abstract class ModbusCommand
	{
		#region Constants

		/// <summary>
		/// Represents the message for an invalid code on a response message.
		/// </summary>
		public const string InvalidFunctionCode = "Invalid command function code";

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a modbus command.
		/// </summary>
		/// <param name="deviceAddress"> The address for the device. </param>
		/// <param name="startAddress"> The starting address of the data. </param>
		/// <param name="functionCode"> The function code of the command. </param>
		protected ModbusCommand(byte deviceAddress, ushort startAddress, ModbusFunctionCode functionCode)
		{
			StartAddress = startAddress;
			FunctionCode = functionCode;
			DeviceAddress = deviceAddress;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The address of the slave device.
		/// </summary>
		public byte DeviceAddress { get; protected set; }

		/// <summary>
		/// Two kinds of error-checking methods are used for standard Modbus networks.
		/// The error checking field contents depend upon the method that is being used.
		/// </summary>
		public ushort ErrorCheckingField { get; protected set; }

		/// <summary>
		/// The Function Code field tells the addressed slave what function to perform.
		/// </summary>
		public ModbusFunctionCode FunctionCode { get; }

		/// <summary>
		/// The address field of a message frame contains two characters (ASCII) or eight bits (RTU).
		/// The individual slave devices are assigned addresses in the range of 1 ... 247.
		/// </summary>
		public ushort StartAddress { get; protected set; }

		#endregion

		#region Methods

		/// <summary>
		/// Converts the command to a buffer.
		/// </summary>
		/// <returns> The byte array of the command. </returns>
		public abstract byte[] ToBuffer();

		/// <summary>
		/// Updates the CRC for the command.
		/// </summary>
		/// <param name="buffer"> The buffer containing the data. </param>
		/// <param name="offset"> The offset where the data starts. </param>
		/// <param name="length"> The length of the data. </param>
		protected void UpdateCrc(byte[] buffer, int offset, int length)
		{
			ErrorCheckingField = ModbusHelper.CalculateCrc(buffer, offset, length);

			buffer[offset + length + 1] = ModbusHelper.High(ErrorCheckingField);
			buffer[offset + length] = ModbusHelper.Low(ErrorCheckingField);
		}

		#endregion
	}
}