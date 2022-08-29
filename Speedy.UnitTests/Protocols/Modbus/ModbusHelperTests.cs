#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Protocols.Modbus;

#endregion

namespace Speedy.UnitTests.Protocols.Modbus
{
	[TestClass]
	public class ModbusHelperTests
	{
		#region Methods

		[TestMethod]
		public void Crc16()
		{
			var expected = new byte[] { 0x01, 0x04, 0x00, 0x14, 0x00, 0x01, 0x71, 0xCE };
			var crc = ModbusHelper.CalculateCrc(expected, 0, 6);
			var crcH = ModbusHelper.High(crc);
			var crcL = ModbusHelper.Low(crc);
			Assert.AreEqual(0x71, crcL, new[] { crcL }.ToHexString());
			Assert.AreEqual(0xCE, crcH, new[] { crcH }.ToHexString());

			expected = new byte[] { 0x01, 0x04, 0x02, 0x09, 0x96, 0x3F, 0x0E };
			crc = ModbusHelper.CalculateCrc(expected, 0, 5);
			crcH = ModbusHelper.High(crc);
			crcL = ModbusHelper.Low(crc);
			Assert.AreEqual(0x3F, crcL, new[] { crcL }.ToHexString());
			Assert.AreEqual(0x0E, crcH, new[] { crcH }.ToHexString());
		}

		[TestMethod]
		public void CrcLrc()
		{
			var scenarios = new Dictionary<string, string>
			{
				{ "20014A88E00200", "FD21" },
				{ "20024A88E00200", "FD20" },
				{ "20034A88E00200", "FD1F" },
				{ "20024A88105A00150600000B5F0B55035B03E80000000000000000000000000018271000010305000D25010D23000B55000B55", "EB88" },
				{ "20034A88105A00150900000B5F0B5F033403E8000000000000000000000000001828B300020303020D27010D25000B55000B55", "EB69" },
				{ "20034A88105A00151700000B5F0B5F03BA03E8000000000000000000000000001828B300020303090D30080D2D000B5F000B5F", "EB15" },
				{ "20044A88E00200", "FD1E" },
				{ "20054A88E00200", "FD1D" },
				{ "20064A88E00200", "FD1C" },
				{ "20074A88E00200", "FD1B" },
				{ "20084A88E00200", "FD1A" },
				{ "20094A88E00200", "FD19" },
				{ "200A4A88E00200", "FD11" },
				{ "200B4A88E00200", "FD10" },
				{ "200C4A88E00200", "FD0F" },
				{ "200D4A88E00200", "FD0E" },
				{ "200E4A88E00200", "FD0D" },
				{ "200F4A88E00200", "FD0C" }
			};

			foreach (var scenario in scenarios)
			{
				Assert.AreEqual(scenario.Value, ModbusHelper.CalculateLrc(scenario.Key));
			}
		}

		#endregion
	}
}