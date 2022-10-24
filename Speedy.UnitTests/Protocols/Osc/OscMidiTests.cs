#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscMidiTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void Equals()
		{
			var expected = new OscMidi(1, 2, 3, 4);
			var actual = new OscMidi(new byte[] { 1, 2, 3, 4 });
			Assert.IsTrue(expected == actual);
			Assert.IsTrue(expected.Equals(actual));
			Assert.IsTrue(expected.Equals(new byte[] { 1, 2, 3, 4 }));
			Assert.AreEqual(62884804, expected.GetHashCode());
			Assert.AreEqual(62884804, actual.GetHashCode());
		}

		[TestMethod]
		public void GetHashCodeShouldSucceed()
		{
			var midi = new OscMidi();
			Assert.AreEqual(0, midi.GetHashCode());
			midi.Port = 1;
			Assert.AreEqual(62570773, midi.GetHashCode());
			midi.Port = 0;
			midi.Status = 1;
			Assert.AreEqual(157609, midi.GetHashCode());
			midi.Status = 0;
			midi.Data1 = 1;
			Assert.AreEqual(397, midi.GetHashCode());
			midi.Data1 = 0;
			midi.Data2 = 1;
			Assert.AreEqual(1, midi.GetHashCode());
			midi.Data2 = 0;
			Assert.AreEqual(0, midi.GetHashCode());
		}

		[TestMethod]
		public void NotEquals()
		{
			var notExpected = new OscMidi(1, 2, 3, 4);
			var actual = new OscMidi(new byte[] { 4, 3, 2, 1 });
			Assert.AreEqual(250755124, actual.GetHashCode());
			Assert.IsTrue(notExpected != actual);
			// ReSharper disable once SuspiciousTypeConversion.Global
			Assert.IsFalse(actual.Equals(true));
		}

		[TestMethod]
		public void ParseException()
		{
			TestHelper.ExpectedException<Exception>(() => OscMidi.Parse("0"), "Not a midi message '0'");
		}

		#endregion
	}
}