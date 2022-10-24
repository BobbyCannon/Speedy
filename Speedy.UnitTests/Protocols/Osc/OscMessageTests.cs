#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Speedy.Extensions;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscMessageTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void AddressEmpty()
		{
			var command = "/";
			var message = (OscError) OscPacket.Parse(command);
			Assert.AreEqual(OscError.Message.InvalidMessageAddress, message.Code);
		}

		[TestMethod]
		public void EscapeQuoteInString()
		{
			var data = "/account/update,\"8d337ca7-8f05-45c8-83f5-dcd5b7bc5725\",\"Dawson\\'s Body Shop\"";
			var packet = OscPacket.Parse(data);

			if (packet is OscError error)
			{
				Assert.Fail($"{error.Code}: {error.Description}");
			}

			var message = packet as OscMessage;
			Assert.IsNotNull(message);
			Assert.AreEqual("8d337ca7-8f05-45c8-83f5-dcd5b7bc5725", message.Arguments[0]);
			Assert.AreEqual("Dawson\'s Body Shop", message.Arguments[1]);
		}

		[TestMethod]
		public void FromBytes()
		{
			var actual = new byte[] { 0x62, 0x75, 0x6E, 0x64, 0x00, 0x79, 0x00, 0x61, 0x00, 0x6C, 0x69, 0x00, 0x00, 0xBB, 0xC0 };
			var slip = new OscSlip();
			var start = 0;
			var actualMessage = (OscError) slip.ProcessBytes(actual, ref start, actual.Length);
			actualMessage.Code.Dump();
			actualMessage.Description.Dump();
		}

		[TestMethod]
		public void MissingAddressPrefix()
		{
			var command = "name";
			var message = (OscError) OscPacket.Parse(command);
			Assert.AreEqual(OscError.Message.InvalidMessageAddress, message.Code);
		}

		[TestMethod]
		public void Parse()
		{
			var command = "/name";
			var message = (OscMessage) OscPacket.Parse(command);
			Assert.AreEqual("/name", message.Address);
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
		public void ParseWithStringInnerQuotes()
		{
			var data = "/update,\"This is \\\"a quote\\\". -John\"";
			var expected = new OscMessage("/update", "This is \"a quote\". -John");
			var actual = (OscMessage) OscPacket.Parse(data);

			Assert.AreEqual(expected.Address, actual.Address);
			Assert.AreEqual(expected.Arguments.Count, actual.Arguments.Count);
			Assert.AreEqual(expected.Arguments[0], actual.Arguments[0]);
		}

		[TestMethod]
		public void TestWithJsonSerializedObject()
		{
			var time = OscTimeTag.Now;
			var json = JsonConvert.SerializeObject(time);
			var message = new OscMessage("/object", json);
			var actual = message.ToString();
			actual.Escape().Dump();

			var actualMessage = OscPacket.Parse(actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			actualMessage.Arguments[0].Dump();

			var actualTime = JsonConvert.DeserializeObject<OscTimeTag>(actualMessage.GetArgument<string>(0));
			Assert.AreEqual(time, actualTime);
		}

		[TestMethod]
		public void ToBytes()
		{
			var message = OscPacket.Parse("/ahoy,\"Hello World\"");
			var actual = OscSlip.EncodePacket(message);
			actual.Dump();

			var slip = new OscSlip();
			var start = 0;
			var actualMessage = (OscMessage) slip.ProcessBytes(actual, ref start, actual.Length);
			actualMessage.Address.Dump();
			actualMessage.Arguments[0].Dump();
		}

		[TestMethod]
		public void ToBytesForBoolean()
		{
			var message = new OscMessage("/a", true);
			//                             /     a                 ,     T
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x54, 0x00, 0x00 };
			var actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", false);
			//                         /     a                 ,     F
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x46, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));
		}

		[TestMethod]
		public void ToBytesForByteArray()
		{
			var message = new OscMessage("/a", new byte[] { 0, 1, 1, 2, 3, 5, 8, 13 });
			//                             /     a                 ,     i                                   8     0     1     1     2     3     5     8    13
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x01, 0x01, 0x02, 0x03, 0x05, 0x08, 0x0D };
			var actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message[0], actualMessage[0]);

			message = new OscMessage("/a", new byte[] { 0, 1, 1, 2, 3, 5, 8 });
			//                         /     a                 ,     i                                   8     0     1     1     2     3     5     8
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x62, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x00, 0x01, 0x01, 0x02, 0x03, 0x05, 0x08, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message[0], actualMessage[0]);
		}
		
		[TestMethod]
		public void ToBytesForDecimal()
		{
			var message = new OscMessage("/a", 123.45678m);
			//                             /     a                 ,     M                 1                                                                                        16
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x4D, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBC, 0x61, 0x4E };
			var actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(123.45678m, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
			
			message = new OscMessage("/a", decimal.MinValue);
			//                         /     a                 ,     M                 1                                                                                        16
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x4D, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
			actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(decimal.MinValue, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
			
			message = new OscMessage("/a", decimal.MaxValue);
			//                         /     a                 ,     M                 1                                                                                        16
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x4D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
			actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(decimal.MaxValue, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
		}
		
		[TestMethod]
		public void ToBytesForIntegers()
		{
			var message = new OscMessage("/a", 1234);
			//                             /     a                 ,     i                 1                 4
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x00, 0x00, 0x04, 0xD2 };
			var actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(1234, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
			
			message = new OscMessage("/a", int.MinValue);
			//                         /     a                 ,     i                 1                 4
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(int.MinValue, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
			
			message = new OscMessage("/a", int.MaxValue);
			//                         /     a                 ,     i                 1                 4
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x69, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF };
			actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(int.MaxValue, message[0]);
			TestHelper.AreEqual(message[0], actualMessage[0]);
		}

		[TestMethod]
		public void ToBytesForStrings()
		{
			var message = new OscMessage("/a", "");
			//                         /     a                 ,     s                 
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			var actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", "1");
			//                         /     a                 ,     s                 1
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x31, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", "123");
			//                         /     a                 ,     s                 1     2     3
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x31, 0x32, 0x33, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", "1234");
			//                         /     a                 ,     s                 1     2     3     4
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x31, 0x32, 0x33, 0x34, 0x00, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", "12345");
			//                         /     a                 ,     s                 1     2     3     4     5
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x73, 0x00, 0x00, 0x31, 0x32, 0x33, 0x34, 0x35, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));
		}

		[TestMethod]
		public void ToBytesForTimeSpan()
		{
			var message = new OscMessage("/a", new TimeSpan(01, 12, 34, 56, 789));
			//                             /     a                 ,     p     
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x70, 0x00, 0x00, 0x00, 0x00, 0x01, 0x32, 0xA1, 0x67, 0x3C, 0x50 };
			var actual = message.ToByteArray();
			actual.Dump();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message[0], actualMessage[0]);
		}

		[TestMethod]
		public void ToBytesForTimeTag()
		{
			TimeService.AddNowProvider(() => new DateTime(2021, 02, 17, 08, 54, 00, DateTimeKind.Local));
			TimeService.AddUtcNowProvider(() => new DateTime(2021, 02, 18, 01, 54, 00, DateTimeKind.Utc));

			var message = new OscMessage("/a", new OscTimeTag(new DateTime(2020, 02, 13, 01, 45, 13, DateTimeKind.Utc)));
			((OscTimeTag) message.Arguments[0]).Value.Dump();

			//                             /     a                 ,     t     
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x74, 0x00, 0x00, 0xE1, 0xEF, 0x28, 0xA9, 0x00, 0x00, 0x00, 0x00 };
			//                             0,    1,    2,    3,    4,    5,    6,    7,    8,    9,    0,    1,    2,    3,    4,    5,
			var actual = message.ToByteArray();
			actual.Dump();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message[0], actualMessage[0]);
		}

		[TestMethod]
		public void ToBytesTypeCombinations()
		{
			var message = new OscMessage("/a", true, "12345");
			//                         /     a                 ,     T     s           1     2     3     4     5
			var expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x54, 0x73, 0x00, 0x31, 0x32, 0x33, 0x34, 0x35, 0x00, 0x00, 0x00 };
			var actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			var actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", 23, "12345");
			//                         /     a                 ,     i     s                            23     1     2     3     4     5
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x69, 0x73, 0x00, 0x00, 0x00, 0x00, 0x17, 0x31, 0x32, 0x33, 0x34, 0x35, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message, actualMessage, nameof(OscTimeTag.Now), nameof(OscTimeTag.UtcNow));

			message = new OscMessage("/a", 23, Guid.Parse("0354FF2E-508C-4CF6-8BEA-2A2870E78A9B"));
			//                         /     a                 ,     i     s                            23     0     3     5     4     F     F     2     E     -     5     0     8     C     -     4     C     F     6     -     8     B     E     A     -     2     A     2     8     7     0     E     7     8     A     9     B
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x69, 0x73, 0x00, 0x00, 0x00, 0x00, 0x17, 0x30, 0x33, 0x35, 0x34, 0x66, 0x66, 0x32, 0x65, 0x2D, 0x35, 0x30, 0x38, 0x63, 0x2D, 0x34, 0x63, 0x66, 0x36, 0x2D, 0x38, 0x62, 0x65, 0x61, 0x2D, 0x32, 0x61, 0x32, 0x38, 0x37, 0x30, 0x65, 0x37, 0x38, 0x61, 0x39, 0x62, 0x00, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			Assert.AreEqual(message[0], actualMessage[0]);
			Assert.AreEqual(message[1].ToString(), actualMessage[1]);

			message = new OscMessage("/a", new byte[] { 0, 1, 1, 2, 3, 5, 8, 13 }, Guid.Parse("0354FF2E-508C-4CF6-8BEA-2A2870E78A9B"));
			//                         /     a                 ,     i     s                             8     0     1     1     2     3     5     8    13     0     3     5     4     F     F     2     E     -     5     0     8     C     -     4     C     F     6     -     8     B     E     A     -     2     A     2     8     7     0     E     7     8     A     9     B
			expected = new byte[] { 0x2F, 0x61, 0x00, 0x00, 0x2C, 0x62, 0x73, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x01, 0x01, 0x02, 0x03, 0x05, 0x08, 0x0D, 0x30, 0x33, 0x35, 0x34, 0x66, 0x66, 0x32, 0x65, 0x2D, 0x35, 0x30, 0x38, 0x63, 0x2D, 0x34, 0x63, 0x66, 0x36, 0x2D, 0x38, 0x62, 0x65, 0x61, 0x2D, 0x32, 0x61, 0x32, 0x38, 0x37, 0x30, 0x65, 0x37, 0x38, 0x61, 0x39, 0x62, 0x00, 0x00, 0x00, 0x00 };
			actual = message.ToByteArray();
			TestHelper.AreEqual(expected, actual);
			actualMessage = OscPacket.Parse(message.Time, actual) as OscMessage;
			Assert.IsNotNull(actualMessage);
			TestHelper.AreEqual(message[0], actualMessage[0]);
			Assert.AreEqual(message[1].ToString(), actualMessage[1]);
		}

		[TestMethod]
		public void ToFromByteArrayOscMessageWithAllTypes()
		{
			TimeService.AddNowProvider(() => new DateTime(2021, 02, 17, 08, 54, 00, DateTimeKind.Local));
			TimeService.AddUtcNowProvider(() => new DateTime(2021, 02, 18, 01, 54, 00, DateTimeKind.Utc));

			var message = GetOscMessage();
			var expected = new byte[] { 0x2F, 0x41, 0x64, 0x64, 0x72, 0x65, 0x73, 0x73, 0x00, 0x00, 0x00, 0x00, 0x2C, 0x69, 0x75, 0x73, 0x62, 0x68, 0x48, 0x48, 0x74, 0x5B, 0x54, 0x69, 0x73, 0x4E, 0x5D, 0x63, 0x54, 0x46, 0x66, 0x49, 0x49, 0x4E, 0x64, 0x49, 0x49, 0x4D, 0x4D, 0x4D, 0x53, 0x72, 0x6D, 0x70, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, 0x00, 0x00, 0x01, 0xC8, 0x42, 0x6F, 0x6F, 0x6D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x01, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x41, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x8E, 0xDF, 0xEE, 0xA7, 0x94, 0x00, 0x00, 0x00, 0x00, 0xDF, 0xEE, 0xB4, 0xC4, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7B, 0x66, 0x6F, 0x78, 0x00, 0x00, 0x00, 0x00, 0x41, 0x42, 0xF6, 0xE6, 0x66, 0x40, 0x4B, 0x29, 0x16, 0x87, 0x2B, 0x02, 0x0C, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x1F, 0x71, 0xFB, 0x04, 0xCB, 0x80, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x65, 0x73, 0x74, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x50, 0x4C, 0x2A, 0x18, 0x00, 0x00, 0x0A, 0x92, 0xE6, 0x1F, 0xB2, 0xA0 };
			var actual = message.ToByteArray();
			actual.Select(x => $"0x{x:X2}, ").Dump();

			TestHelper.AreEqual(expected, actual);

			ValidateOscMessage(OscPacket.Parse(actual) as OscMessage, true);
		}

		[TestMethod]
		public void ToFromStringOscMessageWithAllTypes()
		{
			var message = GetOscMessage();
			var expected = "/Address,123,456u,\"Boom\",{ Blob: 0x010203 },321L,654U,16136018769012064256U,{ Time: 2019-01-20T08:50:12.0000000Z },[True,123,\"fox\",null],\'A\',True,False,123.45f,Infinity,-Infinity,null,54.321d,Infinityd,-Infinityd,12345.67890123m,-79228162514264337593543950335m,79228162514264337593543950335m,Test,{ Color: 1,2,3,4 },{ Midi: 80,76,42,24 },{ TimeSpan: 13.10:56:44.2340000 }";
			var actual = message.ToString();

			actual.Escape().Dump();
			Assert.AreEqual(expected, actual);

			ValidateOscMessage(OscPacket.Parse(actual) as OscMessage, false);
		}

		[TestMethod]
		public void ToStringWithStringContainingSpecialCharacters()
		{
			var itemsToTest = new Dictionary<string, string>
			{
				{ "This is \"a quote\". -John", "/update,\"This is \\\"a quote\\\". -John\"" },
				{ "John's House Of Cards", "/update,\"John\\'s House Of Cards\"" },
				{ "\0", "/update,\"\\0\"" },
				{ "\a", "/update,\"\\a\"" },
				{ "\b", "/update,\"\\b\"" },
				{ "\f", "/update,\"\\f\"" },
				{ "\n", "/update,\"\\n\"" },
				{ "\r", "/update,\"\\r\"" },
				{ "\t", "/update,\"\\t\"" },
				{ "\v", "/update,\"\\v\"" },
				{ "\'", "/update,\"\\\'\"" },
				{ "\\", "/update,\"\\\\\"" }
			};

			foreach (var item in itemsToTest)
			{
				var message = new OscMessage("/update", item.Key);
				var actual = message.ToString();
				Assert.AreEqual(item.Value, actual);

				var actualMessage = OscPacket.Parse(item.Value) as OscMessage;
				Assert.IsNotNull(actualMessage);
				Assert.AreEqual(1, actualMessage.Arguments.Count);
				Assert.AreEqual(item.Key, actualMessage.Arguments[0]);
			}
		}

		private OscMessage GetOscMessage()
		{
			var message = new OscMessage(TimeService.UtcNow, "/Address");
			message.Arguments.Add(123);
			message.Arguments.Add((uint) 456);
			message.Arguments.Add("Boom");
			message.Arguments.Add(new byte[] { 1, 2, 3 });
			message.Arguments.Add((long) 321);
			message.Arguments.Add((ulong) 654);
			message.Arguments.Add(new OscTimeTag(new DateTime(2019, 1, 20, 07, 53, 56, DateTimeKind.Utc)).Value);
			message.Arguments.Add(new OscTimeTag(new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc)));
			message.Arguments.Add(new object[] { true, 123, "fox", null });
			message.Arguments.Add('A');
			message.Arguments.Add(true);
			message.Arguments.Add(false);
			message.Arguments.Add(123.45f);
			message.Arguments.Add(float.PositiveInfinity);
			message.Arguments.Add(float.NegativeInfinity);
			message.Arguments.Add(null);
			message.Arguments.Add(54.321d);
			message.Arguments.Add(double.PositiveInfinity);
			message.Arguments.Add(double.NegativeInfinity);
			message.Arguments.Add(12345.67890123m);
			message.Arguments.Add(decimal.MinValue);
			message.Arguments.Add(decimal.MaxValue);
			message.Arguments.Add(new OscSymbol("Test"));
			message.Arguments.Add(new OscRgba(1, 2, 3, 4));
			message.Arguments.Add(new OscMidi(80, 76, 42, 24));
			message.Arguments.Add(new TimeSpan(12, 34, 56, 43, 1234));
			return message;
		}

		private void ValidateOscMessage(OscMessage actual, bool allInfinityTheSame)
		{
			var index = 0;
			Assert.IsNotNull(actual);
			Assert.AreEqual("/Address", actual.Address);
			Assert.AreEqual(26, actual.Arguments.Count);
			Assert.AreEqual(123, actual.Arguments[index++]);
			Assert.AreEqual((uint) 456, actual.Arguments[index++]);
			Assert.AreEqual("Boom", actual.Arguments[index++]);
			TestHelper.AreEqual(new byte[] { 1, 2, 3 }, actual.Arguments[index++]);
			Assert.AreEqual((long) 321, actual.Arguments[index++]);
			Assert.AreEqual((ulong) 654, actual.Arguments[index++]);
			Assert.AreEqual(new OscTimeTag(new DateTime(2019, 1, 20, 07, 53, 56, DateTimeKind.Utc)).Value, actual.Arguments[index]);
			Assert.AreEqual(16136018769012064256, (ulong) actual.Arguments[index++]);
			Assert.AreEqual(new OscTimeTag(new DateTime(2019, 1, 20, 08, 50, 12, DateTimeKind.Utc)), actual.Arguments[index]);
			Assert.AreEqual(16136033268821655552, ((OscTimeTag) actual.Arguments[index++]).Value);
			TestHelper.AreEqual(new object[] { true, 123, "fox", null }, actual.Arguments[index++]);
			Assert.AreEqual('A', actual.Arguments[index++]);
			Assert.AreEqual(true, actual.Arguments[index++]);
			Assert.AreEqual(false, actual.Arguments[index++]);
			Assert.AreEqual(123.45f, actual.Arguments[index++]);

			if (allInfinityTheSame)
			{
				Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
				Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			}
			else
			{
				Assert.AreEqual(float.PositiveInfinity, actual.Arguments[index++]);
				Assert.AreEqual(float.NegativeInfinity, actual.Arguments[index++]);
			}

			Assert.AreEqual(null, actual.Arguments[index++]);
			Assert.AreEqual(54.321d, actual.Arguments[index++]);
			Assert.AreEqual(double.PositiveInfinity, actual.Arguments[index++]);
			Assert.AreEqual(allInfinityTheSame ? double.PositiveInfinity : double.NegativeInfinity, actual.Arguments[index++]);
			Assert.AreEqual(12345.67890123m, actual.Arguments[index++]);
			Assert.AreEqual(decimal.MinValue, actual.Arguments[index++]);
			Assert.AreEqual(decimal.MaxValue, actual.Arguments[index++]);
			Assert.AreEqual(new OscSymbol("Test"), actual.Arguments[index++]);
			Assert.AreEqual(new OscRgba(1, 2, 3, 4), actual.Arguments[index++]);
			Assert.AreEqual(new OscMidi(80, 76, 42, 24), actual.Arguments[index++]);
			Assert.AreEqual(new TimeSpan(12, 34, 56, 43, 1234), actual.Arguments[index]);
			Assert.AreEqual(new TimeSpan(12, 34, 56, 43, 1234).Ticks, ((TimeSpan) actual.Arguments[index]).Ticks);
		}

		#endregion
	}
}