#region References

using System;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests;

[TestClass]
public class RandomGeneratorTests : SpeedyUnitTest
{
	#region Constants

	public const byte LoopCount = 50;

	#endregion

	#region Methods

	[TestMethod]
	public void DefaultNextDecimal()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextDecimal().Dump();
		}
	}

	[TestMethod]
	public void DefaultNextInteger()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextInteger().Dump();
		}
	}

	[TestMethod]
	public void GetBytes()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.GetBytes(13);
			actual.Dump();
			Assert.AreEqual(13, actual.Length);
		}
	}

	[TestMethod]
	public void GetPassword()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.GetPassword(24);
			actual.ToUnsecureString().Dump();
			Assert.AreEqual(24, actual.Length);
		}
	}

	[TestMethod]
	public void GetPhoneNumber()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.GetPhoneNumber().Dump();
			RandomGenerator.GetPhoneNumber(true).Dump();
		}
	}

	[TestMethod]
	public void LoremIpsum()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.LoremIpsum().Dump();
		}
	}

	[TestMethod]
	public void NextBool()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextBool().Dump();
		}
	}

	[TestMethod]
	public void NextDateTime()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextDateTime(DateTime.MinValue, DateTime.MaxValue);
			var actual = RandomGenerator.NextDateTime(DateTime.MinValue, DateTime.MinValue);
			actual.Dump();
			Assert.AreEqual(DateTime.MinValue, actual);
			actual = RandomGenerator.NextDateTime(DateTime.MaxValue, DateTime.MaxValue);
			actual.Dump();
			Assert.AreEqual(DateTime.MaxValue, actual);

			actual = RandomGenerator.NextDateTime();
			actual.Dump();
		}
	}

	[TestMethod]
	public void NextDecimal()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextDecimal();
			actual.Dump();
			
			actual = RandomGenerator.NextDecimal(scale: 6);
			actual.Dump();
		}
	}
	
	[TestMethod]
	public void NextDecimalWithSameMinimumAndMaximum()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextDecimal(64, 64);
			actual.Dump();
			Assert.AreEqual(64, actual);
			actual = RandomGenerator.NextDecimal(-46, -46);
			actual.Dump();
			Assert.AreEqual(-46, actual);
		}
	}

	[TestMethod]
	public void NextDouble()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextDouble();
			actual.Dump();
			
			actual = RandomGenerator.NextDouble(scale: 6);
			actual.Dump();
		}
	}
	
	[TestMethod]
	public void NextDoubleWithSameMinimumMaximum()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextDouble(10.0, 10.0);
			actual.Dump();
			Assert.AreEqual(10.0, actual);
		}
	}

	[TestMethod]
	public void NextLong()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextLong().Dump();
		}
	}

	[TestMethod]
	public void NextLongWithSameMinimumMaximum()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextLong(48, 48);
			actual.Dump();
			Assert.AreEqual(48, actual);
		}
	}

	[TestMethod]
	public void NextString()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			RandomGenerator.NextString(12).Dump();
		}
	}

	[TestMethod]
	public void NextStringWithProvidedCharacters()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = RandomGenerator.NextString(4, "a");
			actual.Dump();
			Assert.AreEqual("aaaa", actual);
		}
	}

	[TestMethod]
	public void Populate()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = new char[12];
			RandomGenerator.Populate(ref actual);
			actual.Dump();
		}
	}

	[TestMethod]
	public void SetPassword()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var actual = new SecureString();
			Assert.AreEqual(0, actual.Length);
			RandomGenerator.SetPassword(actual);
			Assert.AreEqual(16, actual.Length);
			actual.ToUnsecureString().Dump();

			actual = new SecureString();
			RandomGenerator.SetPassword(actual, 14, false);
			Assert.AreEqual(14, actual.Length);
			actual.ToUnsecureString().Dump();
		}
	}

	[TestMethod]
	public void Shuffle()
	{
		for (var i = 0; i < LoopCount; i++)
		{
			var expected = new[] { 0, 1, 2, 3, 4, 5, 6 };
			var actual = new[] { 0, 1, 2, 3, 4, 5, 6 }.Shuffle();

			actual.Dump();

			TestHelper.AreNotEqual(expected, actual);
		}
	}

	#endregion
}