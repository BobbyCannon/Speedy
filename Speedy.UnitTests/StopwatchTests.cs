#region References

using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class StopwatchTests
	{
		#region Methods

		[TestMethod]
		public void ConstructorFromTime()
		{
			var previous = TimeService.UtcNow.AddSeconds(-10);
			var watch = new Stopwatch(previous);
			var actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsTrue(watch.IsHighResolution);
			Assert.IsFalse(watch.IsRunning);
			Assert.IsTrue(actual.TotalSeconds >= 10, actual.ToString());
			Assert.IsTrue(actual.TotalSeconds < 11, actual.ToString());

			previous = TimeService.UtcNow.AddMilliseconds(-1234);
			watch = new Stopwatch(previous);
			actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsTrue(watch.IsHighResolution);
			Assert.IsFalse(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 1234, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 1235, actual.ToString());
		}

		[TestMethod]
		public void DoubleStartShouldBeIgnored()
		{
			var watch = new Stopwatch();
			watch.Start();
			Thread.Sleep(120);
			watch.Start();
			Thread.Sleep(200);
			var actual = watch.Elapsed;
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void DoubleStopShouldBeIgnored()
		{
			var watch = new Stopwatch();
			watch.Start();
			Thread.Sleep(320);
			watch.Stop();
			Thread.Sleep(10);
			watch.Stop();
			var actual = watch.Elapsed;
			Assert.IsFalse(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void Elapsed()
		{
			var watch = Stopwatch.StartNew();
			Thread.Sleep(1234);
			watch.Stop();
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds >= 1234.0, watch.Elapsed.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1244.0, watch.Elapsed.ToString());
			// This delay should not be counted.
			Thread.Sleep(300);
			// Timer should continue from previous elapsed
			watch.Start();
			Thread.Sleep(300);
			watch.Stop();
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds >= 1534.0, watch.Elapsed.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1544.0, watch.Elapsed.ToString());
		}

		[TestMethod]
		public void IsHighResolution()
		{
			Assert.IsTrue(Stopwatch.IsHighResolutionAvailable);
			Assert.AreEqual(10000000, Stopwatch.Frequency);
			Assert.AreEqual(1, Stopwatch.TickFrequency);
			Assert.AreEqual(10000000, Stopwatch.TicksPerSecond);
			Assert.AreEqual(10000, Stopwatch.TicksPerMillisecond);
		}

		[TestMethod]
		public void Reset()
		{
			var watch = Stopwatch.StartNew();
			Thread.Sleep(200);
			watch.Restart();
			Thread.Sleep(320);
			var actual = watch.Elapsed;
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
			watch.Reset();
			actual = watch.Elapsed;
			Assert.IsFalse(watch.IsRunning);
			Assert.IsTrue(Math.Abs(actual.TotalMilliseconds) < double.Epsilon, actual.ToString());
		}

		[TestMethod]
		public void Restart()
		{
			var watch = Stopwatch.StartNew();
			Thread.Sleep(200);
			watch.Restart();
			Thread.Sleep(320);
			var actual = watch.Elapsed;
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 320.0, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 324.0, actual.ToString());
		}

		[TestMethod]
		public void RestartFromTime()
		{
			var previous = TimeService.UtcNow.AddSeconds(-10);
			var watch = new Stopwatch();

			// Timers from DateTime cannot be high resolution
			Assert.IsTrue(watch.IsHighResolution);
			Assert.IsFalse(watch.IsRunning);

			// Restart watch to a specific time
			watch.Restart(previous);
			var actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsFalse(watch.IsHighResolution);
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalSeconds >= 10, actual.ToString());
			Assert.IsTrue(actual.TotalSeconds < 11, actual.ToString());

			previous = TimeService.UtcNow.AddMilliseconds(-1234);
			watch.Restart(previous);
			actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsFalse(watch.IsHighResolution);
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 1234, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 1235, actual.ToString());
		}

		[TestMethod]
		public void StartNewFromTime()
		{
			var previous = TimeService.UtcNow.AddSeconds(-10);
			var watch = Stopwatch.StartNew(previous);
			var actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsTrue(watch.IsHighResolution);
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalSeconds >= 10, actual.ToString());
			Assert.IsTrue(actual.TotalSeconds < 11, actual.ToString());

			previous = TimeService.UtcNow.AddMilliseconds(-1234);
			watch = Stopwatch.StartNew(previous);
			actual = watch.Elapsed;

			// Timers from DateTime cannot be high resolution
			Assert.IsTrue(watch.IsHighResolution);
			Assert.IsTrue(watch.IsRunning);
			Assert.IsTrue(actual.TotalMilliseconds >= 1234, actual.ToString());
			Assert.IsTrue(actual.TotalMilliseconds < 1235, actual.ToString());
		}

		#endregion
	}
}