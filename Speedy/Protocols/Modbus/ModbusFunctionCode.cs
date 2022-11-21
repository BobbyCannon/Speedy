namespace Speedy.Protocols.Modbus;

/// <summary>
/// https://www.modbus.org/docs/PI_MBUS_300.pdf
/// </summary>
public enum ModbusFunctionCode
{
	/// <summary>
	/// Reads the binary contents of holding registers in the slave.
	/// </summary>
	ReadHoldingRegisters = 3,

	/// <summary>
	/// Reads the binary contents of input registers in the slave.
	/// </summary>
	ReadInputRegisters = 4
}