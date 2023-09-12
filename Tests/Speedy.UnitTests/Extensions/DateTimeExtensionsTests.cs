#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class DateTimeExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void Since()
	{
		SetTime(new DateTime(2023, 08, 25, 08, 12, 45, DateTimeKind.Utc));

		var expected = TimeSpan.Parse("2.11:14:44");
		var value = "08-22-23 08:58:01 PM".ToUtcDateTime();
		var actual = value.Since(useTimeService: true);
		actual.ToString().Dump();

		AreEqual(expected, actual);
	}

	[TestMethod]
	public void ToUtcString()
	{
		var test1 = new DateTime(2022, 08, 03, 03, 20, 12, DateTimeKind.Utc);
		var test2 = new DateTime(2022, 08, 03, 03, 20, 12, DateTimeKind.Unspecified);
		var test3 = test1.ToLocalTime();

		Assert.AreEqual("2022-08-03T03:20:12.0000000Z", test1.ToUtcString());
		Assert.AreEqual("2022-08-03T03:20:12.0000000Z", test2.ToUtcString());
		Assert.AreEqual("2022-08-03T03:20:12.0000000Z", test3.ToUtcString());
	}

	#endregion
}