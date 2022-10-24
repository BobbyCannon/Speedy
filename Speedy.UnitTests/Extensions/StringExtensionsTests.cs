#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
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
		public void ToByteString()
		{
			Assert.AreEqual("41426162", "ABab".ToHexString());
			Assert.AreEqual("08090D0A", "\b\t\r\n".ToHexString());
		}

		#endregion
	}
}