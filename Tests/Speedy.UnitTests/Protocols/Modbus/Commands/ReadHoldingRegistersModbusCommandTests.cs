#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Modbus.Commands;

#endregion

namespace Speedy.UnitTests.Protocols.Modbus.Commands
{
	[TestClass]
	public class ReadHoldingRegistersModbusCommandTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ToBuffer()
		{
			var command = new ReadHoldingRegistersModbusCommand(1, 18, 1);
			var expected = new byte[] { 0x01, 0x03, 0x00, 0x12, 0x00, 0x01, 0x24, 0x0F };
			var actual = command.ToBuffer();
			AreEqual(expected, actual);
		}

		#endregion
	}
	
	[TestClass]
	public class ReadHoldingRegistersModbusResponseTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void FromToBuffer()
		{
			var command = new ReadHoldingRegistersModbusCommand(1, 18, 1);
			var expected = new byte[] { 0x01, 0x03, 0x00, 0x12, 0x00, 0x01, 0x24, 0x0F };
			var actual = command.ToBuffer();
			AreEqual(expected, actual);
		}

		#endregion
	}
}