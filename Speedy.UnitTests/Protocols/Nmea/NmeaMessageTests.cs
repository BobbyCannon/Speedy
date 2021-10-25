#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Nmea.Messages;

#endregion

namespace Speedy.UnitTests.Protocols.Nmea
{
	[TestClass]
	public class NmeaMessageTests
	{
		#region Methods

		[TestMethod]
		public void TestMethodExtractChecksum()
		{
			var n = new RmcMessage();
			var m = "$GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A*65";
			var c = n.ExtractChecksum(m);
			Assert.AreEqual("65", c);
		}

		[TestMethod]
		public void TestMethodExtractChecksumInvalidSize()
		{
			var n = new RmcMessage();
			Assert.AreEqual(string.Empty, n.ExtractChecksum(null));
			Assert.AreEqual(string.Empty, n.ExtractChecksum(""));
			Assert.AreEqual(string.Empty, n.ExtractChecksum(" "));
			Assert.AreEqual(string.Empty, n.ExtractChecksum("  "));
			Assert.AreEqual(string.Empty, n.ExtractChecksum("   "));
			Assert.AreEqual(string.Empty, n.ExtractChecksum("$*"));
			Assert.AreEqual(string.Empty, n.ExtractChecksum("$GNRMC,*"));
		}

		[TestMethod]
		public void TestMethodExtractChecksumNoStar()
		{
			var n = new RmcMessage();
			var m = "$GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A";
			var c = n.ExtractChecksum(m);
			Assert.AreEqual(string.Empty, c);
		}

		[TestMethod]
		public void TestMethodParseChecksum()
		{
			var n = new RmcMessage();
			Assert.AreEqual("00", n.ParseChecksum(""));
			Assert.AreEqual("00", n.ParseChecksum("X"));
			Assert.AreEqual("00", n.ParseChecksum("$"));
			Assert.AreEqual("58", n.ParseChecksum("$X"));
			Assert.AreEqual("58", n.ParseChecksum("$X*"));
			Assert.AreEqual("59", n.ParseChecksum("$Y"));
			Assert.AreEqual("59", n.ParseChecksum("$Y*"));
			Assert.AreEqual("65", n.ParseChecksum("$GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A"));
			Assert.AreEqual("65", n.ParseChecksum("$GNRMC,143718.00,A,4513.13793,N,01859.19704,E,0.050,,290719,,,A*65"));
		}

		#endregion
	}
}