#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class StringExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void FromHexString()
	{
		Assert.AreEqual("ABab", "41426162".FromHexString());
		Assert.AreEqual("\b\t\r\n", "08090D0A".FromHexString());
	}

	[TestMethod]
	public void Reverse()
	{
		Assert.AreEqual("CBA", "ABC".ReverseString());
		Assert.AreEqual("321", "123".ReverseString());
	}

	[TestMethod]
	public void ToByteArray()
	{
		var scenarios = new Dictionary<string, byte[]>
		{
			{ "aoeu\xFF\xBC", new byte[] { 0x61, 0x6F, 0x65, 0x75, 0xFF, 0xBC } }
		};

		foreach (var scenario in scenarios)
		{
			AreEqual(scenario.Value, scenario.Key.ToByteArray());
		}

		// All values should pass
		for (var i = 0; i <= 255; i++)
		{
			var array = new[] { (byte) i };
			var codeString = array.ToCodeString();
			var actual = codeString.Unescape().ToByteArray();

			$"{codeString} == {actual.ToHexString(delimiter: ", ", prefix: "0x")}".Dump();
			AreEqual(array, actual);
		}
	}

	[TestMethod]
	public void ToByteArrayForEscapeCharacter()
	{
		var array = new[] { (byte) 0x5C };
		var codeString = array.ToCodeString();
		var actual = codeString.Unescape().ToByteArray();

		$"{codeString} == {actual.ToHexString(delimiter: ", ", prefix: "0x")}".Dump();
		AreEqual(array, actual);
	}

	[TestMethod]
	public void ToCodeString()
	{
		var bytes = new byte[] { 0x61, 0x6F, 0x65, 0x75, 0xFF };
		var codeString = bytes.ToCodeString().Dump();

		// Code string formats are escaped so remove escapes before comparing
		var actual = codeString.Unescape();

		AreEqual("aoeu\xFF", actual);

		var actualBytes = actual.ToByteArray();
		actualBytes.ToHexString(delimiter: ", ", prefix: "0x").Dump();

		AreEqual(bytes, actualBytes);
	}

	[TestMethod]
	public void ToCodeStringWithTrailingAlphabetCharactersAfterHexValue()
	{
		var bytes = new byte[] { 0xFF, 0x45 };
		var codeString = bytes.ToCodeString().Dump();
		var expected = "\\xFF\\x45"; // "\xFFE"
		AreEqual(expected, codeString);
	}

	[TestMethod]
	public void ToHexString()
	{
		Assert.AreEqual("41426162", "ABab".ToHexString());
		Assert.AreEqual("08090D0A", "\b\t\r\n".ToHexString());
	}

	#endregion
}