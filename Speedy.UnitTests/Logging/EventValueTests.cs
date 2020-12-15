#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class EventValueTests
	{
		#region Methods

		[TestMethod]
		public void ConstructorObjectShouldConvertToValue()
		{
			var dateTimeValue = new EventValue("Test", new DateTime(2020, 12, 15, 01, 02, 03, 999, DateTimeKind.Utc));
			Assert.AreEqual("12/15/2020 01:02:03 AM", dateTimeValue.Value);

			var timeValue = new EventValue("Test", new TimeSpan(123, 12, 59, 59, 999));
			Assert.AreEqual("123.12:59:59.9990000", timeValue.Value);
			
			var timeValue2 = new EventValue("Test", TimeSpan.FromTicks(1));
			Assert.AreEqual("00:00:00.0000001", timeValue2.Value);
		}

		[TestMethod]
		public void ConstructorShouldNotAllowEmptyOrWhitespaceName()
		{
			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentException>(() => new EventValue("", null), "The name is required.");

			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentException>(() => new EventValue(" ", null), "The name is required.");
		}

		[TestMethod]
		public void ConstructorShouldNotAllowNullName()
		{
			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentNullException>(() => new EventValue(null, null), "The name cannot be null.");
		}

		#endregion
	}
}