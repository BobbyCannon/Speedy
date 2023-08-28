#region References

using System;
using System.IO.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Converters;

#endregion

namespace Speedy.UnitTests.Converters;

[TestClass]
public class ObjectConverterTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ConvertToByte()
	{
		AreEqual((byte) 0, ObjectConverter.Convert<byte>(null));
		AreEqual((byte) 0, ObjectConverter.Convert(typeof(byte), null));
		AreEqual((byte) 123, ObjectConverter.Convert(typeof(byte), (byte) 123));
		AreEqual((byte) 123, ObjectConverter.Convert(typeof(byte), (sbyte) 123));
		AreEqual((byte) 123, ObjectConverter.Convert(typeof(byte), 123));
		AreEqual((byte) 123, ObjectConverter.Convert(typeof(byte), (uint) 123));
		AreEqual((byte) 255, ObjectConverter.Convert(typeof(byte), 255));
		AreEqual((byte) 5, ObjectConverter.Convert(typeof(byte), "5"));
		AreEqual((byte) 0, ObjectConverter.Convert(typeof(byte), false));
		AreEqual((byte) 1, ObjectConverter.Convert(typeof(byte), true));

		// Bad values should throw a format exception
		ExpectedException<FormatException>(() => ObjectConverter.Convert(typeof(byte), "aoeu"), "The value format is not supported.");
		ExpectedException<FormatException>(() => ObjectConverter.Convert(typeof(byte), "256"), "The value format is not supported.");
		ExpectedException<FormatException>(() => ObjectConverter.Convert(typeof(byte), "-1"), "The value format is not supported.");
		ExpectedException<FormatException>(() => ObjectConverter.Convert(typeof(byte), -1), "The value format is not supported.");
		ExpectedException<FormatException>(() => ObjectConverter.Convert(typeof(byte), 256), "The value format is not supported.");
	}
	
	[TestMethod]
	public void ConvertToChar()
	{
		AreEqual((char) 0, ObjectConverter.Convert<char>(null));
		AreEqual((char) 0, ObjectConverter.Convert(typeof(char), null));
		AreEqual((char) 123, ObjectConverter.Convert(typeof(char), 123));
		AreEqual('5', ObjectConverter.Convert(typeof(char), "5"));
	}
	
	[TestMethod]
	public void ConvertToDateTime()
	{
		AreEqual(DateTime.MinValue, ObjectConverter.Convert<DateTime>(null));
		AreEqual(DateTime.MinValue, ObjectConverter.Convert(typeof(DateTime), null));
		AreEqual(new DateTime(2023, 04, 16, 0, 0, 0, DateTimeKind.Utc), ObjectConverter.Convert<DateTime>("2023/04/16"));

		// todo: support int
		//AreEqual(DateTime.MinValue, ObjectConverter.Convert(typeof(DateTime), 123));
	}

	[TestMethod]
	public void ConvertToEnum()
	{
		AreEqual(Parity.None, ObjectConverter.Convert<Parity>(null));
		AreEqual(Parity.None, ObjectConverter.Convert(typeof(Parity), null));
		AreEqual(Parity.Even, ObjectConverter.Convert<Parity>("Even"));
		AreEqual(Parity.Even, ObjectConverter.Convert<Parity>("even"));
		AreEqual(Parity.Even, ObjectConverter.Convert<Parity>("EVEN"));
		AreEqual(Parity.Mark, ObjectConverter.Convert<Parity>(Parity.Mark));
	}

	[TestMethod]
	public void ConvertToString()
	{
		AreEqual(null, ObjectConverter.Convert<string>(null));
		AreEqual(null, ObjectConverter.Convert(typeof(string), null));
		AreEqual("1", ObjectConverter.Convert(typeof(string), 1));
		AreEqual("False", ObjectConverter.Convert(typeof(string), false));
		AreEqual("True", ObjectConverter.Convert(typeof(string), true));
		AreEqual("123.456", ObjectConverter.Convert(typeof(string), 123.456d));
		AreEqual("123.456", ObjectConverter.Convert(typeof(string), 123.456f));
		AreEqual("123.456", ObjectConverter.Convert(typeof(string), 123.456m));
	}

	#endregion
}