#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;
using System;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class AverageTimerTests : BaseTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			var dateTime = new DateTime(2020, 04, 23, 07, 56, 12);
			var timer = new AverageTimer(10);
			
			Assert.IsFalse(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime;

			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime.AddTicks(10);
			
			timer.Stop();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(10, timer.Elapsed.Ticks);
			Assert.AreEqual(10, timer.Average.Ticks);
			Assert.AreEqual(1, timer.Samples);

			// Just bump up to ensure average is borked by time moving
			TimeService.UtcNowProvider = () => dateTime.AddTicks(100);

			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime.AddTicks(120);
			
			timer.Stop();

			// 10 + 20 = 30 / 2 = 15
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(20, timer.Elapsed.Ticks);
			Assert.AreEqual(15, timer.Average.Ticks);
			Assert.AreEqual(2, timer.Samples);

			// Just bump up to ensure average is borked by time moving
			TimeService.UtcNowProvider = () => dateTime.AddTicks(131);
			
			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			TimeService.UtcNowProvider = () => dateTime.AddTicks(140);
			
			timer.Stop();

			// 10 + 20 + 9 = 39 / 3 = 13
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(9, timer.Elapsed.Ticks);
			Assert.AreEqual(13, timer.Average.Ticks);
			Assert.AreEqual(3, timer.Samples);
		}
	}
}