#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscSlipTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void DecodeOscBundleWithManyBundles()
		{
			var data = new byte[] { 0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00, 0xDF, 0xEE, 0xB4, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x2F, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x73, 0x54, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, 0x66, 0x6F, 0x6F, 0x00, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x64, 0x65, 0x6C, 0x61, 0x79, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x01, 0x41, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscBundle;
			Assert.IsNotNull(actual);
			Assert.AreEqual(2, actual.Count);
		}

		[TestMethod]
		public void DecodeOscMessageWithAllTypes()
		{
			var message = new OscMessage(TimeService.UtcNow, "/Address");
			message.Arguments.Add(123);
			message.Arguments.Add(123.45f);
			message.Arguments.Add("Boom");
			message.Arguments.Add(new byte[] { 1, 2, 3 });
			message.Arguments.Add((long) 321);
			message.Arguments.Add(new OscTimeTag(new DateTime(2019, 1, 20, 07, 53, 56, DateTimeKind.Local)).Value);
			message.Arguments.Add(new OscTimeTag(new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Local)));
			message.Arguments.Add(54.321d);
			message.Arguments.Add(new object[] { true, 123, "fox", null });
			message.Arguments.Add('A');
			message.Arguments.Add(true);
			message.Arguments.Add(false);
			message.Arguments.Add(float.PositiveInfinity);
			message.Arguments.Add(float.NegativeInfinity);
			message.Arguments.Add(null);
			message.Arguments.Add(double.PositiveInfinity);
			message.Arguments.Add(double.NegativeInfinity);
			message.Arguments.Add(new OscSymbol("Test"));
			message.Arguments.Add(new OscRgba(1, 2, 3, 4));
			message.Arguments.Add(new OscMidi());

			var packets = new List<OscPacket>();
			var processor = new OscSlip();
			var data = OscSlip.EncodePacket(message);

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var index = 0;
			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/Address", actual.Address);
			Assert.AreEqual(20, actual.Arguments.Count);
			Assert.AreEqual(123, actual.Arguments[index++]);
			Assert.AreEqual(123.45f, actual.Arguments[index++]);
			Assert.AreEqual("Boom", actual.Arguments[index++]);
			TestHelper.AreEqual(new byte[] { 1, 2, 3 }, actual.Arguments[index++]);
			Assert.AreEqual((long) 321, actual.Arguments[index++]);
			Assert.AreEqual(new OscTimeTag(new DateTime(2019, 1, 20, 07, 53, 56, DateTimeKind.Local)), actual.Arguments[index++]);
			Assert.AreEqual(new OscTimeTag(new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Local)), actual.Arguments[index++]);
			Assert.AreEqual(54.321d, actual.Arguments[index++]);
			TestHelper.AreEqual(new List<object> { true, 123, "fox", null }, actual.Arguments[index++]);
			Assert.AreEqual('A', actual.Arguments[index++]);
			Assert.AreEqual(true, actual.Arguments[index++]);
			Assert.AreEqual(false, actual.Arguments[index++]);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			Assert.AreEqual(null, actual.Arguments[index++]);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			TestHelper.AreEqual(new OscSymbol("Test"), actual.Arguments[index++]);
			TestHelper.AreEqual(new OscRgba(1, 2, 3, 4), actual.Arguments[index++]);
			TestHelper.AreEqual(new OscMidi(), actual.Arguments[index++]);
		}

		[TestMethod]
		public void DecodeOscMessageWithDoubleArgument()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x64, 0x00, 0x00, 0x40, 0x93, 0x4A, 0x45, 0x6D, 0x5C, 0xFA, 0xAD, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(1234.5678d, actual.Arguments[0]);
		}

		[TestMethod]
		public void DecodeOscMessageWithFloatArgument()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x66, 0x00, 0x00, 0x44, 0x9A, 0x52, 0x2B, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(1234.5678f, actual.Arguments[0]);
		}

		[TestMethod]
		public void DecodeOscMessageWithIntArgument()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(1234, actual.Arguments[0]);
		}

		[TestMethod]
		public void DecodeOscMessageWithNoArguments()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(0, actual.Arguments.Count);
		}

		[TestMethod]
		public void DecodeOscMessageWithStringArgument()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x46, 0x6F, 0x6F, 0x42, 0x61, 0x72, 0x00, 0x00, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual("FooBar", actual.Arguments[0]);
		}

		[TestMethod]
		public void DecodeProcessStream()
		{
			var data = new byte[] { 0x23, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x00, 0xDF, 0xEE, 0xB4, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x2F, 0x6D, 0x65, 0x73, 0x73, 0x61, 0x67, 0x65, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x73, 0x54, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, 0x66, 0x6F, 0x6F, 0x00, 0x00, 0x00, 0x00, 0x10, 0x2F, 0x64, 0x65, 0x6C, 0x61, 0x79, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x01, 0x41, 0xC0 };
			var stream = new MemoryStream(data);
			var processor = new OscSlip();
			var packets = processor.ProcessStream(stream).ToList();

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscBundle;
			Assert.IsNotNull(actual);
			Assert.AreEqual(2, actual.Count);
		}

		[TestMethod]
		public void EncodePacket()
		{
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(123);

			var actual = OscSlip.EncodePacket(message);
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, 0xC0 };

			actual.Dump();

			TestHelper.AreEqual(expected, actual);
			Assert.IsTrue((actual.Length % 4) == 1);
		}

		[TestMethod]
		public void EncodeThenDecodeOscMessageWithRgbaArgument()
		{
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(new OscRgba(1, 2, 3, 4));

			var data = OscSlip.EncodePacket(message);
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x72, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0xC0 };

			data.Dump();

			TestHelper.AreEqual(expected, data);
			Assert.IsTrue((data.Length % 4) == 1);

			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(new OscRgba(1, 2, 3, 4), actual.Arguments[0]);
		}

		[TestMethod]
		public void EncodeThenDecodeOscMessageWithStringArgument()
		{
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add("Test");

			var data = OscSlip.EncodePacket(message);
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x54, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0xC0 };

			data.Dump();

			TestHelper.AreEqual(expected, data);
			Assert.IsTrue((data.Length % 4) == 1);

			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual("Test", actual.Arguments[0]);
		}

		[TestMethod]
		public void EncodeThenDecodeOscMessageWithSymbolArgument()
		{
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(new OscSymbol("Test"));

			// The OscSymbol of test should force another 4 bytes for null terminator
			var data = OscSlip.EncodePacket(message);
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x53, 0x00, 0x00, 0x54, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0xC0 };

			data.Dump();

			TestHelper.AreEqual(expected, data);
			Assert.IsTrue((data.Length % 4) == 1);

			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(new OscSymbol("Test"), actual.Arguments[0]);
		}

		[TestMethod]
		public void ResetThenDecodeMessage()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x00, 0x00, 0x00, 0xC0 };
			var packets = new List<OscPacket>();
			var processor = new OscSlip();

			processor.ProcessByte(0x55);
			processor.ProcessByte(0xAA);
			processor.ProcessByte(0x0F);
			processor.ProcessByte(0xF0);

			Assert.AreEqual(4, processor.Count);

			processor.Reset();

			foreach (var d in data)
			{
				var p = processor.ProcessByte(d);
				if (p != null)
				{
					packets.Add(p);
				}
			}

			Assert.AreEqual(1, packets.Count);

			var actual = packets[0] as OscMessage;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(0, actual.Arguments.Count);
		}

		#endregion
	}
}