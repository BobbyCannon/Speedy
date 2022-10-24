#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscPacketTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void FromBytes()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x54, 0x00, 0x00 };
			var actual = (OscMessage) OscPacket.Parse(data);

			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(true, actual.Arguments[0]);
		}

		[TestMethod]
		public void FromBytesOfDoubleNegativeInfinity()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x49, 0x00, 0x00 };
			var actual = (OscMessage) OscPacket.Parse(data);

			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[0]);
		}

		[TestMethod]
		public void FromBytesOfFloatNegativeInfinity()
		{
			var data = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x49, 0x00, 0x00 };
			var actual = (OscMessage) OscPacket.Parse(data);

			Assert.AreEqual("/test", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[0]);
		}

		[TestMethod]
		public void GetBytes()
		{
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(true);
			message.Arguments.Add(123);

			var actual = message.ToByteArray();
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x54, 0x69, 0x00, 0x00, 0x00, 0x00, 0x7B };

			actual.Dump();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void GetBytesForAllInfinity()
		{
			var expected = new byte[] { 0x2F, 0x74, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x2C, 0x49, 0x00, 0x00 };
			var message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(float.PositiveInfinity);
			var actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);

			message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(float.NegativeInfinity);
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);

			message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(double.PositiveInfinity);
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);

			message = new OscMessage(TimeService.UtcNow, "/test");
			message.Arguments.Add(double.NegativeInfinity);
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ParseAllTypes()
		{
			var command = "/command,\"value1, value2\",0.1234d";
			var packet = OscPacket.Parse(TimeService.UtcNow, command, CultureInfo.CurrentCulture);
			var actual = packet as OscMessage;

			Assert.IsNotNull(actual);
			Assert.AreEqual("/command", actual.Address);
			Assert.AreEqual(2, actual.Arguments.Count);
			Assert.AreEqual("value1, value2", actual.Arguments[0]);
			Assert.AreEqual(0.1234, actual.Arguments[1]);
		}

		[TestMethod]
		public void ParseDouble()
		{
			var message = new OscMessage("/command", 1234.5678);
			var actualString = message.ToString();
			var command = "/command,1234.5678d";

			Assert.AreEqual(command, actualString);

			var packet = OscPacket.Parse(TimeService.UtcNow, command, CultureInfo.CurrentCulture);
			var actual = packet as OscMessage;

			Assert.IsNotNull(actual);
			Assert.AreEqual("/command", actual.Address);
			Assert.AreEqual(1, actual.Arguments.Count);
			Assert.AreEqual(1234.5678, actual.Arguments[0]);
		}

		[TestMethod]
		public void ParseHexNumbers()
		{
			var data = "/command,0x0000007B, 0x00000000000004D2";
			var expected = new OscMessage("/command", 123, 1234L);
			var actual = (OscMessage) OscPacket.Parse(data);

			TestHelper.AreEqual(expected, actual, false, nameof(OscMessage.Time));
		}

		[TestMethod]
		public void ParseString()
		{
			var commands = new[]
			{
				"/command,\"value1, value2\",0.1234d",
				"/command, \"value1, value2\", 0.1234d",
				"/command,  \"value1, value2\",  0.1234d"
			};

			foreach (var command in commands)
			{
				var packet = OscPacket.Parse(TimeService.UtcNow, command, CultureInfo.CurrentCulture);
				var actual = packet as OscMessage;

				Assert.IsNotNull(actual);
				Assert.AreEqual("/command", actual.Address);
				Assert.AreEqual(2, actual.Arguments.Count);
				Assert.AreEqual("value1, value2", actual.Arguments[0]);
				Assert.AreEqual(0.1234, actual.Arguments[1]);
			}

			commands = new[]
			{
				"/command,0.1234d,\"value1, value2\"",
				"/command, 0.1234d, \"value1, value2\"",
				"/command,  0.1234d,  \"value1, value2\""
			};

			foreach (var command in commands)
			{
				var packet = OscPacket.Parse(TimeService.UtcNow, command, CultureInfo.CurrentCulture);
				var actual = packet as OscMessage;

				Assert.IsNotNull(actual);
				Assert.AreEqual("/command", actual.Address);
				Assert.AreEqual(2, actual.Arguments.Count);
				Assert.AreEqual(0.1234, actual.Arguments[0]);
				Assert.AreEqual("value1, value2", actual.Arguments[1]);
			}
		}

		[TestMethod]
		public void ParseSymbol()
		{
			var values = new Dictionary<string, string>
			{
				{ "foo", "foo" },
				{ " foo", "foo" },
				{ " foo ", "foo" },
				{ " foo bar", "foo bar" },
				{ " foo bar ", "foo bar" }
			};

			foreach (var item in values)
			{
				var command = $"/system/time, {item.Key}";
				var message = (OscMessage) OscPacket.Parse(command);
				Assert.AreEqual("/system/time", message.Address);
				Assert.AreEqual(1, message.Arguments.Count);
				Assert.AreEqual(item.Value, ((OscSymbol) message.Arguments[0]).Value);
			}
		}

		[TestMethod]
		public void ParseTime()
		{
			var values = new Dictionary<string, OscTimeTag>
			{
				{ "2019-04-05T00:00:59.1234Z", OscTimeTag.Parse("2019-04-05T00:00:59.1234Z") },
				{ "2019-04-05T00:00:59Z", OscTimeTag.Parse("2019-04-05T00:00:59Z") },
				{ "2019-04-05", OscTimeTag.Parse("2019-04-05") }
			};

			foreach (var e in values)
			{
				var command = $"/system/time, {{ Time: {e.Key} }}";
				var message = (OscMessage) OscPacket.Parse(command);
				Assert.AreEqual("/system/time", message.Address);
				Assert.AreEqual(1, message.Arguments.Count);
				Assert.AreEqual(e.Value, ((OscTimeTag) message.Arguments[0]).Value);
			}
		}

		[TestMethod]
		public void ParseUnknownCommand()
		{
			var message = (OscMessage) OscPacket.Parse("/some/command/name, {{ Unknown: FooBar }}");
			Assert.AreEqual("/some/command/name", message.Address);
			Assert.AreEqual(1, message.Arguments.Count);
			Assert.AreEqual("FooBar", message.Arguments[0]);

			message = (OscMessage) OscPacket.Parse("/some/command/name, {{Unknown: FooBar}}");
			Assert.AreEqual("/some/command/name", message.Address);
			Assert.AreEqual(1, message.Arguments.Count);
			Assert.AreEqual("FooBar", message.Arguments[0]);

			message = (OscMessage) OscPacket.Parse("/some/command/name,{{Unknown:FooBar}}");
			Assert.AreEqual("/some/command/name", message.Address);
			Assert.AreEqual(1, message.Arguments.Count);
			Assert.AreEqual("FooBar", message.Arguments[0]);

			message = (OscMessage) OscPacket.Parse("/some/command/name,foo bar ,32");
			Assert.AreEqual("/some/command/name", message.Address);
			Assert.AreEqual(2, message.Arguments.Count);
			Assert.AreEqual(new OscSymbol("foo bar"), message.Arguments[0]);
			Assert.AreEqual(32, message.Arguments[1]);
		}

		[TestMethod]
		public void ToStringAllTypes()
		{
			var oscMessage = new OscMessage("/command",
				123,
				(uint) 456,
				4321L,
				8765UL,
				12.34f,
				123.456d,
				"123456",
				true,
				false,
				new byte[] { 0, 1, 2 },
				new object[] { -123, (uint) 456, -4321L, 8765UL, -12.34f, -123.456d, true, false },
				new DateTime(2021, 02, 17, 09, 18, 41, 842, DateTimeKind.Utc),
				new OscTimeTag(new DateTime(2021, 02, 17, 09, 18, 41, 842, DateTimeKind.Utc))
			);

			// The non-hex version
			var expected = "/command,123,456u,4321L,8765U,12.34f,123.456d,\"123456\",True,False,{ Blob: 0x000102 },[-123,456u,-4321L,8765U,-12.34f,-123.456d,True,False],{ Time: 2021-02-17T09:18:41.8420000Z },{ Time: 2021-02-17T09:18:41.8420000Z }";
			var actualString = oscMessage.ToString();
			Assert.AreEqual(expected, actualString);
			var actualMessage = OscPacket.Parse(expected) as OscMessage;
			TestHelper.AreEqual(oscMessage, actualMessage, false, nameof(OscMessage.Time));

			// The hex version
			expected = "/command,0x0000007B,0x000001C8u,0x00000000000010E1L,0x000000000000223DU,12.34f,123.456d,\"123456\",True,False,{ Blob: 0x000102 },[0xFFFFFF85,0x000001C8u,0xFFFFFFFFFFFFEF1FL,0x000000000000223DU,-12.34f,-123.456d,True,False],{ Time: 2021-02-17T09:18:41.8420000Z },{ Time: 2021-02-17T09:18:41.8420000Z }";
			actualString = oscMessage.ToHexString();
			Assert.AreEqual(expected, actualString);
			actualMessage = OscPacket.Parse(expected) as OscMessage;
			TestHelper.AreEqual(oscMessage, actualMessage, false, nameof(OscMessage.Time));
		}

		[TestMethod]
		public void ToStringAsHexFormat()
		{
			var oscMessage = new OscMessage("/command", 123, 1234L);
			var expected = "/command,0x0000007B,0x00000000000004D2L";
			var actualString = oscMessage.ToHexString();

			Assert.AreEqual(expected, actualString);

			var actualMessage = OscPacket.Parse(expected) as OscMessage;
			TestHelper.AreEqual(oscMessage, actualMessage, false, nameof(OscMessage.Time));

			oscMessage = new OscMessage("/command", int.MinValue, long.MinValue);
			expected = "/command,0x80000000,0x8000000000000000L";
			actualString = oscMessage.ToHexString();

			Assert.AreEqual(expected, actualString);

			actualMessage = OscPacket.Parse(expected) as OscMessage;
			TestHelper.AreEqual(oscMessage, actualMessage, false, nameof(OscMessage.Time));

			oscMessage = new OscMessage("/command", int.MaxValue, long.MaxValue);
			expected = "/command,0x7FFFFFFF,0x7FFFFFFFFFFFFFFFL";
			actualString = oscMessage.ToHexString();

			Assert.AreEqual(expected, actualString);

			actualMessage = OscPacket.Parse(expected) as OscMessage;
			TestHelper.AreEqual(oscMessage, actualMessage, false, nameof(OscMessage.Time));
		}

		#endregion
	}
}