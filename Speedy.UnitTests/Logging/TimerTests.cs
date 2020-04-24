#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;
using System;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class TimerTests : BaseTests
	{
		[TestMethod]
		public void TimerShouldTrackUsingTimeService()
		{
			var dateTime = new DateTime(2020, 04, 23, 07, 56, 12);
			var timer = new Timer();
			
			Assert.IsFalse(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime;

			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime.AddTicks(1);
			
			timer.Stop();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(1, timer.Elapsed.Ticks);
		}
	}
}
