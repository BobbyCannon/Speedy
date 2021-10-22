#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Protocols.Osc;

#endregion

namespace Speedy.UnitTests.Protocols.Osc
{
	[TestClass]
	public class OscSymbolTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void Equals()
		{
			var expected = new OscSymbol { Value = "Foo Bar" };
			var actual = new OscSymbol("Foo Bar");
			Assert.IsTrue(expected == actual);
			Assert.IsTrue(expected.Equals(actual));
			// ReSharper disable once SuspiciousTypeConversion.Global
			Assert.IsTrue(expected.Equals("Foo Bar"));
			Assert.AreEqual(1716311859, expected.GetHashCode());
			Assert.AreEqual(1716311859, actual.GetHashCode());
		}

		[TestMethod]
		public void NotEquals()
		{
			var notExpected = new OscSymbol { Value = "Foo Bar" };
			var actual = new OscSymbol("foo bar");
			Assert.AreEqual(1715228275, actual.GetHashCode());
			Assert.IsTrue(notExpected != actual);
			// ReSharper disable once SuspiciousTypeConversion.Global
			Assert.IsFalse(actual.Equals(true));
		}

		#endregion
	}
}