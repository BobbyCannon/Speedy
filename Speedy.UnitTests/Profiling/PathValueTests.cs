#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class PathValueTests
	{
		#region Methods

		[TestMethod]
		public void ConstructorObjectShouldConvertToValue()
		{
			var dateTimeValue = new TrackerPathValue("Test", new DateTime(2020, 12, 15, 01, 02, 03, 999, DateTimeKind.Utc));
			Assert.AreEqual("12/15/2020 1:02:03 AM", dateTimeValue.Value);

			var timeValue = new TrackerPathValue("Test", new TimeSpan(123, 12, 59, 59, 999));
			Assert.AreEqual("123.12:59:59.9990000", timeValue.Value);
			
			var timeValue2 = new TrackerPathValue("Test", TimeSpan.FromTicks(1));
			Assert.AreEqual("00:00:00.0000001", timeValue2.Value);
		}

		[TestMethod]
		public void ConstructorShouldNotAllowEmptyOrWhitespaceName()
		{
			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentException>(() => new TrackerPathValue("", null), "The name is required.");

			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentException>(() => new TrackerPathValue(" ", null), "The name is required.");
		}

		[TestMethod]
		public void ConstructorShouldNotAllowNullName()
		{
			// ReSharper disable once ObjectCreationAsStatement
			TestHelper.ExpectedException<ArgumentNullException>(() => new TrackerPathValue(null, null), "The name cannot be null.");
		}

		#endregion
	}
}