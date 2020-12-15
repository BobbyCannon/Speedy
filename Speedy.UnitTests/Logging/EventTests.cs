#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class EventTests
	{
		#region Methods

		[TestMethod]
		public void CompleteShouldWork()
		{
			var currentTime = new DateTime(2020, 12, 15, 01, 02, 03, 999, DateTimeKind.Utc);

			// ReSharper disable once AccessToModifiedClosure
			TimeService.UtcNowProvider = () => currentTime;

			var e = new Event();
			Assert.AreEqual(false, e.IsCompleted);
			Assert.AreEqual(0, e.ElapsedTime.Ticks);

			currentTime = currentTime.AddMilliseconds(1);

			e.Complete();
			Assert.AreEqual(true, e.IsCompleted);
			Assert.AreEqual(1, e.ElapsedTime.TotalMilliseconds);
		}

		#endregion
	}
}