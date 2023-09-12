#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Modbus.Commands;

#endregion

namespace Speedy.UnitTests.Protocols.Modbus.Commands
{
	[TestClass]
	public class ReadInputRegistersModbusCommandTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ToBuffer()
		{
			var command = new ReadInputRegistersModbusCommand(1, 20, 1);
			var expected = new byte[] { 0x01, 0x04, 0x00, 0x14, 0x00, 0x01, 0x71, 0xCE };
			var actual = command.ToBuffer();
			AreEqual(expected, actual);
		}

		#endregion
	}
}