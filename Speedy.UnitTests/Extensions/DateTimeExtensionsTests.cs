#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class DateTimeExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void ToUtcString()
		{
			var test1 = new DateTime(2022, 08, 03, 03, 20, 12, DateTimeKind.Utc);
			var test2 = new DateTime(2022, 08, 03, 03, 20, 12, DateTimeKind.Unspecified);
			var test3 = new DateTime(2022, 08, 03, 03, 20, 12, DateTimeKind.Local);

			Assert.AreEqual("2022-08-03T03:20:12.0000000Z", test1.ToUtcString());
			Assert.AreEqual("2022-08-03T03:20:12.0000000Z", test2.ToUtcString());
			Assert.AreEqual("2022-08-03T07:20:12.0000000Z", test3.ToUtcString());
		}

		#endregion
	}
}