#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Collections;
using Speedy.Exceptions;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class EndianBufferTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void GetNumbers()
	{
		//                             0     1     2     3     4     5     6     7
		var expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45 };
		BitConverter.ToInt16(expected, 0).Dump();
		BitConverter.ToUInt16(expected, 0).Dump();
		BitConverter.ToInt32(expected, 0).Dump();
		BitConverter.ToUInt32(expected, 0).Dump();
		BitConverter.ToInt64(expected, 0).Dump();
		BitConverter.ToUInt64(expected, 0).Dump();

		BitConverter.GetBytes(-4981827302456632065).Dump();
		BitConverter.GetBytes(-4981827302456632065).Reverse().ToArray().Dump();
	}

	[TestMethod]
	public void LoadBuffers()
	{
		//                             0     1     2     3     4     5     6     7
		var expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45 };
		var little = LittleEndianBuffer.Load(expected);
		AreEqual(0, little.ReadIndex);
		AreEqual(0, little.WriteIndex);
		AreEqual(true, little.IsLittleEndian);
		AreEqual(0x2301, little.TryReadInt16(out var shortValue) ? shortValue : throw new Exception("Failed to read"));
		AreEqual(2, little.ReadIndex);
		AreEqual(0, little.WriteIndex);
		AreEqual(0x6745, little.TryReadUInt16(out var ushortValue) ? ushortValue : throw new Exception("Failed to read"));
		AreEqual(4, little.ReadIndex);
		AreEqual(0, little.WriteIndex);

		var big = BigEndianBuffer.Load(expected);
		AreEqual(0, big.ReadIndex);
		AreEqual(0, big.WriteIndex);
		AreEqual(false, big.IsLittleEndian);
		AreEqual(0x0123, big.TryReadInt16(out shortValue) ? shortValue : throw new Exception("Failed to read"));
		AreEqual(2, big.ReadIndex);
		AreEqual(0, big.WriteIndex);
		AreEqual(0x4567, big.TryReadUInt16(out ushortValue) ? ushortValue : throw new Exception("Failed to read"));
		AreEqual(4, big.ReadIndex);
		AreEqual(0, big.WriteIndex);
	}

	[TestMethod]
	public void ReadAndWriteIndexes()
	{
		var littleBuffer = new LittleEndianBuffer(10);
		AreEqual(0, littleBuffer.ReadIndex);
		AreEqual(0, littleBuffer.WriteIndex);

		IsTrue(littleBuffer.TryWriteInt64(-4981827302456632065));
		AreEqual(0, littleBuffer.ReadIndex);
		AreEqual(8, littleBuffer.WriteIndex);

		var expected = new byte[] { 0xFF, 0xDC, 0xBA, 0x98, 0x76, 0xFE, 0xDC, 0xBA, 0x00, 0x00 };
		AreEqual(expected, littleBuffer.ToArray());

		IsTrue(littleBuffer.TryReadInt64(out var i64));
		AreEqual(-4981827302456632065, i64);
		AreEqual(8, littleBuffer.ReadIndex);
		AreEqual(8, littleBuffer.WriteIndex);

		expected = new byte[] { 0xFF, 0xDC, 0xFF, 0xDC, 0x76, 0xFE, 0xDC, 0xBA, 0x00, 0x00 };
		littleBuffer.WriteIndex = 2;
		IsTrue(littleBuffer.TryWriteInt16(-8961));
		AreEqual(expected, littleBuffer.ToArray());

		littleBuffer.ReadIndex = 2;
		IsTrue(littleBuffer.TryReadInt16(out var i16));
		AreEqual(-8961, i16);
	}

	[TestMethod]
	public void ReadWithoutWritingShouldFail()
	{
		var indexScenarios = new[] { 0, 2, 20 };
		var scenarios = new EndianBuffer[]
		{
			new LittleEndianBuffer(8),
			new BigEndianBuffer(8)
		};

		foreach (var readIndex in indexScenarios)
		{
			foreach (var scenario in scenarios)
			{
				scenario.ReadIndex = readIndex;
				IsFalse(scenario.TryReadInt16(out _));
				IsFalse(scenario.TryReadInt32(out _));
				IsFalse(scenario.TryReadInt64(out _));
				IsFalse(scenario.TryReadUInt16(out _));
				IsFalse(scenario.TryReadUInt32(out _));
				IsFalse(scenario.TryReadUInt64(out _));
			}
		}
	}

	[TestMethod]
	public void ShouldWriteCorrectlyForInt16()
	{
		// -8961 in little endian format
		var expected = new byte[] { 0xFF, 0xDC };
		var littleBuffer = new LittleEndianBuffer(expected.Length);
		var result = littleBuffer.TryWriteInt16(-8961);
		IsTrue(result);
		var actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-8961, littleBuffer.TryReadInt16(out var sValue) ? sValue : throw new Exception("Failed to read"));

		// -8961 in big endian format
		expected = new byte[] { 0xDC, 0xFF };
		var bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteInt16(-8961);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-8961, bigBuffer.TryReadInt16(out sValue) ? sValue : throw new Exception("Failed to read"));

		// 8961 (unsigned) in little endian format
		expected = new byte[] { 0x01, 0x23 };
		littleBuffer = new LittleEndianBuffer(expected.Length);
		result = littleBuffer.TryWriteUInt16(8961);
		IsTrue(result);
		actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(8961, littleBuffer.TryReadUInt16(out var uValue) ? uValue : throw new Exception("Failed to read"));

		// 8961 (unsigned) in big endian format
		expected = new byte[] { 0x23, 0x01 };
		bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteUInt16(8961);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(8961, bigBuffer.TryReadUInt16(out uValue) ? uValue : throw new Exception("Failed to read"));
	}

	[TestMethod]
	public void ShouldWriteCorrectlyForInt32()
	{
		// -1732584193 in little endian format
		var expected = new byte[] { 0xFF, 0xDC, 0xBA, 0x98 };

		var littleBuffer = new LittleEndianBuffer(expected.Length);
		var result = littleBuffer.TryWriteInt32(-1732584193);
		IsTrue(result);
		var actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-1732584193, littleBuffer.TryReadInt32(out var iValue) ? iValue : throw new Exception("Failed to read"));

		// -1732584193 in big endian format
		expected = new byte[] { 0x98, 0xBA, 0xDC, 0xFF };
		var bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteInt32(-1732584193);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-1732584193, bigBuffer.TryReadInt32(out iValue) ? iValue : throw new Exception("Failed to read"));

		// 1732584193 (unsigned) in little endian format
		expected = new byte[] { 0x01, 0x23, 0x45, 0x67 };
		littleBuffer = new LittleEndianBuffer(expected.Length);
		result = littleBuffer.TryWriteUInt32(1732584193);
		IsTrue(result);
		actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(1732584193u, littleBuffer.TryReadUInt32(out var uValue) ? uValue : throw new Exception("Failed to read"));

		// 1732584193 (unsigned) in big endian format
		expected = new byte[] { 0x67, 0x45, 0x23, 0x01 };
		bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteUInt32(1732584193);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(1732584193u, bigBuffer.TryReadUInt32(out uValue) ? uValue : throw new Exception("Failed to read"));
	}

	[TestMethod]
	public void ShouldWriteCorrectlyForInt64()
	{
		// -4981827302456632065 in little endian format
		var expected = new byte[] { 0xFF, 0xDC, 0xBA, 0x98, 0x76, 0xFE, 0xDC, 0xBA };
		var littleBuffer = new LittleEndianBuffer(expected.Length);
		var result = littleBuffer.TryWriteInt64(-4981827302456632065);
		IsTrue(result);
		var actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-4981827302456632065, littleBuffer.TryReadInt64(out var sValue) ? sValue : throw new Exception("Failed to read"));

		// -4981827302456632065 in big endian format
		expected = new byte[] { 0xBA, 0xDC, 0xFE, 0x76, 0x98, 0xBA, 0xDC, 0xFF };
		var bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteInt64(-4981827302456632065);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(-4981827302456632065, bigBuffer.TryReadInt64(out sValue) ? sValue : throw new Exception("Failed to read"));

		// 4981827302456632065 (unsigned) in little endian format
		expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45 };
		littleBuffer = new LittleEndianBuffer(expected.Length);
		result = littleBuffer.TryWriteUInt64(4981827302456632065);
		IsTrue(result);
		actual = littleBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(4981827302456632065u, littleBuffer.TryReadUInt64(out var uValue) ? uValue : throw new Exception("Failed to read"));

		// 4981827302456632065 (unsigned) in big endian format
		expected = new byte[] { 0x45, 0x23, 0x01, 0x89, 0x67, 0x45, 0x23, 0x01 };
		bigBuffer = new BigEndianBuffer(expected.Length);
		result = bigBuffer.TryWriteUInt64(4981827302456632065);
		IsTrue(result);
		actual = bigBuffer.ToArray();
		AreEqual(expected, actual, actual.Dump());
		AreEqual(4981827302456632065u, bigBuffer.TryReadUInt64(out uValue) ? uValue : throw new Exception("Failed to read"));
	}

	[TestMethod]
	public void WriteArray()
	{
		//                             0     1     2     3     4     5     6     7
		var expected = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0x01, 0x23, 0x45 };
		var scenarios = new EndianBuffer[]
		{
			new LittleEndianBuffer(8),
			new BigEndianBuffer(8)
		};

		foreach (var scenario in scenarios)
		{
			AreEqual(0, scenario.ReadIndex);
			AreEqual(0, scenario.WriteIndex);
			scenario.WriteArray(expected);
			AreEqual(expected, scenario.ToArray());
			AreEqual(0, scenario.ReadIndex);
			AreEqual(8, scenario.WriteIndex);
		}
	}

	[TestMethod]
	public void WritesShouldException()
	{
		var scenarios = new EndianBuffer[]
		{
			new LittleEndianBuffer(1),
			new BigEndianBuffer(1)
		};

		foreach (var scenario in scenarios)
		{
			ExpectedException<SpeedyException>(() => scenario.WriteInt16(short.MaxValue), "Failed to write the int16 value.");
			ExpectedException<SpeedyException>(() => scenario.WriteInt32(int.MaxValue), "Failed to write the int32 value.");
			ExpectedException<SpeedyException>(() => scenario.WriteInt64(long.MaxValue), "Failed to write the int64 value.");
			ExpectedException<SpeedyException>(() => scenario.WriteUInt16(ushort.MaxValue), "Failed to write the uint16 value.");
			ExpectedException<SpeedyException>(() => scenario.WriteUInt32(uint.MaxValue), "Failed to write the uint32 value.");
			ExpectedException<SpeedyException>(() => scenario.WriteUInt64(ulong.MaxValue), "Failed to write the uint64 value.");
		}
	}

	[TestMethod]
	public void WriteWithBadIndexShouldFail()
	{
		var indexScenarios = new[] { 10, 99 };
		var scenarios = new EndianBuffer[]
		{
			new LittleEndianBuffer(8),
			new BigEndianBuffer(8)
		};

		foreach (var writeIndex in indexScenarios)
		{
			foreach (var scenario in scenarios)
			{
				scenario.WriteIndex = writeIndex;
				AreEqual(8, scenario.WriteIndex);
				IsFalse(scenario.TryWriteInt16(0));
				IsFalse(scenario.TryWriteInt32(0));
				IsFalse(scenario.TryWriteInt64(0));
				IsFalse(scenario.TryWriteUInt16(0));
				IsFalse(scenario.TryWriteUInt32(0));
				IsFalse(scenario.TryWriteUInt64(0));
			}
		}
	}

	#endregion
}