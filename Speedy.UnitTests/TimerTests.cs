#region References

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class TimerTests
	{
		#region Methods

		[TestMethod]
		public void DoubleStartShouldBeIgnored()
		{
			var timer = new Timer();
			timer.Start();
			Thread.Sleep(120);
			timer.Start();
			Thread.Sleep(200);
			var actual = timer.Elapsed;
			Assert.IsTrue(timer.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void DoubleStopShouldBeIgnored()
		{
			var timer = new Timer();
			timer.Start();
			Thread.Sleep(320);
			timer.Stop();
			Thread.Sleep(10);
			timer.Stop();
			var actual = timer.Elapsed;
			Assert.IsFalse(timer.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void Elapsed()
		{
			var timer = Timer.StartNew();
			Thread.Sleep(1234);
			timer.Stop();
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds >= 1234.0, timer.Elapsed.ToString());
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 1244.0, timer.Elapsed.ToString());
			// This delay should not be counted.
			Thread.Sleep(300);
			// Timer should continue from previous elapsed
			timer.Start();
			Thread.Sleep(300);
			timer.Stop();
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds >= 1534.0, timer.Elapsed.ToString());
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 1544.0, timer.Elapsed.ToString());
		}

		[TestMethod]
		public void IsHighResolution()
		{
			Assert.IsTrue(Timer.IsHighResolutionAvailable);
			Assert.AreEqual(10000000, Timer.Frequency);
			Assert.AreEqual(1, Timer.TickFrequency);
			Assert.AreEqual(10000000, Timer.TicksPerSecond);
			Assert.AreEqual(10000, Timer.TicksPerMillisecond);
		}

		[TestMethod]
		public void Reset()
		{
			var timer = Timer.StartNew();
			Thread.Sleep(200);
			timer.Restart();
			Thread.Sleep(320);
			var actual = timer.Elapsed;
			Assert.IsTrue(timer.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
			timer.Reset();
			actual = timer.Elapsed;
			Assert.IsFalse(timer.IsRunning);
			Assert.IsTrue(Math.Abs(actual.TotalMilliseconds) < double.Epsilon, actual.ToString());
		}

		[TestMethod]
		public void Restart()
		{
			var timer = Timer.StartNew();
			Thread.Sleep(200);
			timer.Restart();
			Thread.Sleep(320);
			var actual = timer.Elapsed;
			Assert.IsTrue(timer.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void StartTimerFromTime()
		{
			var previous = TimeService.UtcNow.AddSeconds(-10);
			var timer = Timer.CreateNew(previous);

			// Timers from DateTime cannot be high resolution
			Assert.IsFalse(timer.IsHighResolution);
			Assert.IsFalse(timer.IsRunning);
			Assert.IsTrue(timer.Elapsed.TotalSeconds >= 10, timer.Elapsed.ToString());
			Assert.IsTrue(timer.Elapsed.TotalSeconds < 11, timer.Elapsed.ToString());

			previous = TimeService.UtcNow.AddMilliseconds(-1234);
			timer = Timer.CreateNew(previous);

			// Timers from DateTime cannot be high resolution
			Assert.IsFalse(timer.IsHighResolution);
			Assert.IsFalse(timer.IsRunning);
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds >= 1234, timer.Elapsed.ToString());
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 1235, timer.Elapsed.ToString());
		}

		#endregion
	}
}