#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscBitConverterTests
	{
		#region Methods

		[TestMethod]
		public void Decimal()
		{
			var scenarios = new[]
			{
				0m,
				0.123456789m,
				123.456789m,
				123456789m,
				decimal.MinValue,
				decimal.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToDecimal(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void Double()
		{
			var scenarios = new[]
			{
				0d,
				0.123456789d,
				123.456789d,
				123456789d,
				double.MinValue,
				double.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToDouble(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void Float()
		{
			var scenarios = new[]
			{
				0f,
				0.123456789f,
				123.456789f,
				123456789f,
				float.MinValue,
				float.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToFloat(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void Int32()
		{
			var scenarios = new[]
			{
				0,
				123,
				123456789,
				-123,
				-123456789,
				int.MinValue,
				int.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToInt32(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void Int64()
		{
			var scenarios = new[]
			{
				0L,
				123L,
				123456789L,
				-123L,
				-123456789L,
				long.MaxValue,
				long.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToInt64(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void UInt32()
		{
			var scenarios = new[]
			{
				(uint) 0,
				(uint) 123,
				(uint) 123456789,
				uint.MinValue,
				uint.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToUInt32(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		[TestMethod]
		public void UInt64()
		{
			var scenarios = new[]
			{
				0UL,
				123UL,
				123456789UL,
				ulong.MaxValue,
				ulong.MaxValue
			};

			foreach (var scenario in scenarios)
			{
				var bytes = OscBitConverter.GetBytes(scenario);
				var actual = OscBitConverter.ToUInt64(bytes);
				Assert.AreEqual(scenario, actual);
			}
		}

		#endregion
	}
}