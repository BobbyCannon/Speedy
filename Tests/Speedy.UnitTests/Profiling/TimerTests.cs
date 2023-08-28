#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class TimerTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void AddAverageTimerShouldWork()
		{
			SetTime(new DateTime(2020, 04, 23, 07, 56, 12));

			var timer = new Timer();
			var count = 0;

			timer.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(Timer.Elapsed))
				{
					count++;
				}
			};

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			var averageTimer = new AverageTimer();
			Assert.AreEqual(0, averageTimer.Elapsed.TotalMilliseconds);
			averageTimer.Start();
			Assert.AreEqual(0, averageTimer.Elapsed.TotalMilliseconds);
			Assert.AreEqual(0, count);
			IncrementTime(TimeSpan.FromMilliseconds(123456));
			Assert.AreEqual(123456, averageTimer.Elapsed.TotalMilliseconds);
			Assert.AreEqual(0, count);
			averageTimer.Stop();

			Assert.AreEqual(123456, averageTimer.Elapsed.TotalMilliseconds);
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, count);

			timer.Add(averageTimer);

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(123456, timer.Elapsed.TotalMilliseconds);
			Assert.AreEqual("00:02:03.4560000", timer.Elapsed.ToString());
			Assert.AreEqual(1, count);
		}

		[TestMethod]
		public void AddTimeSpanShouldWork()
		{
			var timer = new Timer();
			var count = 0;

			timer.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(Timer.Elapsed))
				{
					count++;
				}
			};

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);
			Assert.AreEqual(0, count);

			timer.Add(TimeSpan.FromMilliseconds(123456));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(123456, timer.Elapsed.TotalMilliseconds);
			Assert.AreEqual("00:02:03.4560000", timer.Elapsed.ToString());
			Assert.AreEqual(1, count);
		}

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
			SetTime(new DateTime(2020, 04, 23, 07, 56, 12));

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
			IncrementTime(TimeSpan.FromMilliseconds(123456));

			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(123456, timer.Elapsed.TotalMilliseconds);
		}

		[TestMethod]
		public void ShouldTrackUsingTimeService()
		{
			SetTime(new DateTime(2020, 04, 23, 07, 56, 12));
			var timer = new Timer();

			Assert.IsFalse(timer.IsRunning);
			
			timer.Start();

			Assert.IsTrue(timer.IsRunning);
			IncrementTime(TimeSpan.FromTicks(1));

			timer.Stop();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(1, timer.Elapsed.Ticks);
		}

		[TestMethod]
		public void StartWithDateTimeShouldStartTimerInPast()
		{
			SetTime(new DateTime(2020, 04, 23, 07, 56, 12));

			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Start(CurrentTime.AddMilliseconds(-12345));
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(12345, timer.Elapsed.TotalMilliseconds);

			timer.Stop();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(12345, timer.Elapsed.TotalMilliseconds);
		}

		[TestMethod]
		public void StopWithDateTimeShouldStopTimerInPast()
		{
			SetTime(new DateTime(2020, 04, 23, 07, 56, 12));

			var timer = new Timer();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Ticks);

			timer.Start();
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.TotalMilliseconds);

			IncrementTime(TimeSpan.FromSeconds(12));

			timer.Stop(new DateTime(2020, 04, 23, 07, 56, 15));
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(3000, timer.Elapsed.TotalMilliseconds);
		}

		#endregion
	}
}