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
		public void ShouldResetToProvidedElapsed()
		{
			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Reset(TimeSpan.FromMilliseconds(1234));
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(1234, timer.Elapsed.TotalMilliseconds);
			
			timer.Reset();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.TotalMilliseconds);
		}
		
		[TestMethod]
		public void ShouldRestartWithProvidedStartTime()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 12);

			TimeService.UtcNowProvider = () => currentTime;
			
			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Restart(new DateTime(2020, 04, 23, 07, 53, 46));

			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(146000, timer.Elapsed.TotalMilliseconds);

			timer.Restart();
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.TotalMilliseconds);
			
			timer.Restart();
			currentTime = currentTime.AddMilliseconds(123456);
			
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(123456, timer.Elapsed.TotalMilliseconds);
		}

		[TestMethod]
		public void ShouldTrackUsingTimeService()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 12);
			var timer = new Timer();
			
			Assert.IsFalse(timer.IsRunning);
			TimeService.UtcNowProvider = () => currentTime;

			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			currentTime = currentTime.AddTicks(1);
			
			timer.Stop();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(1, timer.Elapsed.Ticks);
		}

		[TestMethod]
		public void StartWithDateTimeShouldStartTimerInPast()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 12);

			TimeService.UtcNowProvider = () => currentTime;

			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Start(currentTime.AddMilliseconds(-12345));
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(12345, timer.Elapsed.TotalMilliseconds);

			timer.Stop();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(12345, timer.Elapsed.TotalMilliseconds);
		}
		
		[TestMethod]
		public void StopWithDateTimeShouldStopTimerInPast()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 12);

			TimeService.UtcNowProvider = () => currentTime;

			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Start();
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.TotalMilliseconds);

			currentTime = currentTime.AddSeconds(12);

			timer.Stop(new DateTime(2020, 04, 23, 07, 56, 15));
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(3000, timer.Elapsed.TotalMilliseconds);
		}
	}
}
