#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscRgbaTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void Equals()
		{
			var expected = new OscRgba { A = 0x01, B = 0x02, G = 0x03, R = 0x04 };
			var actual = new OscRgba { A = 0x01, B = 0x02, G = 0x03, R = 0x04 };
			Assert.IsTrue(expected == actual);
			Assert.IsTrue(expected.Equals(actual));
			Assert.IsTrue(expected.Equals(new byte[] { 0x04, 0x03, 0x02, 0x01 }));
			Assert.AreEqual(250755124, expected.GetHashCode());
			Assert.AreEqual(250755124, actual.GetHashCode());
		}

		[TestMethod]
		public void GetHashCodeShouldSucceed()
		{
			var rgba = new OscRgba();
			Assert.AreEqual(0, rgba.GetHashCode());
			rgba.R = 1;
			Assert.AreEqual(62570773, rgba.GetHashCode());
			rgba.R = 0;
			rgba.G = 1;
			Assert.AreEqual(157609, rgba.GetHashCode());
			rgba.G = 0;
			rgba.B = 1;
			Assert.AreEqual(397, rgba.GetHashCode());
			rgba.B = 0;
			rgba.A = 1;
			Assert.AreEqual(1, rgba.GetHashCode());
			rgba.A = 0;
			Assert.AreEqual(0, rgba.GetHashCode());
		}

		[TestMethod]
		public void NotEquals()
		{
			var notExpected = new OscRgba { A = 0x04, B = 0x03, G = 0x02, R = 0x01 };
			var actual = new OscRgba { A = 0x01, B = 0x02, G = 0x03, R = 0x04 };
			Assert.AreEqual(250755124, actual.GetHashCode());
			Assert.IsTrue(notExpected != actual);
			// ReSharper disable once SuspiciousTypeConversion.Global
			Assert.IsFalse(actual.Equals(true));
		}

		[TestMethod]
		public void ParseException()
		{
			TestHelper.ExpectedException<Exception>(() => OscRgba.Parse("FooBar"), "Invalid color 'FooBar'");
		}

		#endregion
	}
}