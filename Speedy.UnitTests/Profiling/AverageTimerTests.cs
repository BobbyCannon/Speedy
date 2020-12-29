#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class AverageTimerTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void AverageTimerWithMovingAverage()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 00);
			var timer = new AverageTimer();

			TimeService.UtcNowProvider = () => currentTime;

			timer.Time(() => currentTime = currentTime.AddTicks(1));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(1, timer.Elapsed.Ticks);
			Assert.AreEqual(1, timer.Average.Ticks);
			Assert.AreEqual(0, timer.Samples);
			Assert.AreEqual(1, timer.Count);

			timer.Time(() => currentTime = currentTime.AddTicks(2));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(2, timer.Elapsed.Ticks);
			Assert.AreEqual(1, timer.Average.Ticks);
			Assert.AreEqual(0, timer.Samples);
			Assert.AreEqual(2, timer.Count);

			timer.Time(() => currentTime = currentTime.AddTicks(3));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(3, timer.Elapsed.Ticks);
			Assert.AreEqual(1, timer.Average.Ticks);
			Assert.AreEqual(0, timer.Samples);
			Assert.AreEqual(3, timer.Count);

			timer.Time(() => currentTime = currentTime.AddTicks(4));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(4, timer.Elapsed.Ticks);
			Assert.AreEqual(2, timer.Average.Ticks);
			Assert.AreEqual(0, timer.Samples);
			Assert.AreEqual(4, timer.Count);

			timer.Time(() => currentTime = currentTime.AddTicks(5));

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(5, timer.Elapsed.Ticks);
			Assert.AreEqual(3, timer.Average.Ticks);
			Assert.AreEqual(0, timer.Samples);
			Assert.AreEqual(5, timer.Count);
		}

		[TestMethod]
		public void CancelShouldResetTimer()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 00);
			var timer = new AverageTimer(4);

			TimeService.UtcNowProvider = () => currentTime;

			timer.Start();
			currentTime = currentTime.AddMilliseconds(123);

			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Milliseconds);
			Assert.AreEqual(0, timer.Average.Ticks);

			// Cancel should reset state to empty
			timer.Cancel();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Milliseconds);
			Assert.AreEqual(0, timer.Average.Ticks);

			// Calling stop later should not change state
			timer.Stop();

			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Milliseconds);
			Assert.AreEqual(0, timer.Average.Ticks);
		}

		[TestMethod]
		public void CancelShouldResetTimerWithoutChangingHistory()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 00);
			var timer = new AverageTimer(4);

			TimeService.UtcNowProvider = () => currentTime;

			timer.Start();
			currentTime = currentTime.AddMilliseconds(123);
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(0, timer.Elapsed.Milliseconds);
			Assert.AreEqual(0, timer.Average.Ticks);

			timer.Stop();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(123, timer.Elapsed.Milliseconds);
			Assert.AreEqual(1230000, timer.Average.Ticks);
			Assert.AreEqual(1, timer.Samples);

			// Restart timer
			currentTime = currentTime.AddMilliseconds(12);
			timer.Start();
			currentTime = currentTime.AddMilliseconds(13);
			Assert.IsTrue(timer.IsRunning);
			Assert.AreEqual(123, timer.Elapsed.Milliseconds);
			Assert.AreEqual(1230000, timer.Average.Ticks);
			Assert.AreEqual(1, timer.Samples);

			// Cancel should reset state to empty
			timer.Cancel();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(123, timer.Elapsed.Milliseconds);
			Assert.AreEqual(1230000, timer.Average.Ticks);
			Assert.AreEqual(1, timer.Samples);

			// Calling stop later should not change state
			timer.Stop();
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(123, timer.Elapsed.Milliseconds);
			Assert.AreEqual(1230000, timer.Average.Ticks);
			Assert.AreEqual(1, timer.Samples);
		}

		[TestMethod]
		public void IsRunning()
		{
			var timer = new AverageTimer();
			Assert.IsFalse(timer.IsRunning);

			timer.Start();
			Assert.IsTrue(timer.IsRunning);

			timer.Stop();
			Assert.IsFalse(timer.IsRunning);

			timer.Time(() => { Assert.IsTrue(timer.IsRunning); });

			Assert.IsFalse(timer.IsRunning);

			var actual = timer.Time(() =>
			{
				Assert.IsTrue(timer.IsRunning);
				return true;
			});

			Assert.IsTrue(actual);
			Assert.IsFalse(timer.IsRunning);
		}

		[TestMethod]
		public void ShouldAverageOverTime()
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

		[TestMethod]
		public void ShouldAverageWithLimit()
		{
			var currentTime = new DateTime(2020, 04, 23, 07, 56, 00);
			var timer = new AverageTimer(4);
			var values = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

			TimeService.UtcNowProvider = () => currentTime;

			for (var i = 0; i < values.Length; i++)
			{
				var value = values[i];

				timer.Start();
				currentTime = currentTime.AddTicks(value);
				timer.Stop();

				// Just bump up to ensure average is not borked by time moving
				currentTime = currentTime.AddTicks(50 + i);
			}

			// 6 + 7 + 8 + 9 = 30 / 4 = 7
			Assert.IsFalse(timer.IsRunning);
			Assert.AreEqual(9, timer.Elapsed.Ticks);
			Assert.AreEqual(7, timer.Average.Ticks);
			Assert.AreEqual(4, timer.Samples);
		}

		#endregion
	}
}