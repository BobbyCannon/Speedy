#region References

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class UtilityExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void RetryShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			UtilityExtensions.Retry(() =>
			{
				if (++count < 3)
				{
					throw new Exception("Nope");
				}
			}, 1000, 50);
			watch.Stop();
			Assert.AreEqual(3, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryShouldTimeoutAndThrowLastException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();

			TestHelper.ExpectedException<Exception>(() =>
				UtilityExtensions.Retry(() =>
				{
					if (++count < 3)
					{
						throw new Exception("Nope..." + count);
					}
				}, 100, 50), "Nope...2");

			watch.Stop();
			Assert.AreEqual(2, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryTypedShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Retry(() =>
			{
				if (++count < 3)
				{
					throw new Exception("Nope");
				}

				return count;
			}, 1000, 50);
			watch.Stop();
			Assert.AreEqual(3, result);
			Assert.AreEqual(3, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryTypeShouldTimeoutAndThrowLastException()
		{
			var count = 0;
			var result = 0;
			var watch = Stopwatch.StartNew();

			TestHelper.ExpectedException<Exception>(() =>
				result = UtilityExtensions.Retry(() =>
				{
					if (++count < 3)
					{
						throw new Exception("Nope..." + count);
					}

					return count;
				}, 100, 50), "Nope...2");

			watch.Stop();
			Assert.AreEqual(0, result);
			Assert.AreEqual(2, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void UpdateIf()
		{
			var account = new AccountEntity();
			Assert.AreEqual(null, account.Name);

			account.IfThen(x => x.Name == null, x => x.Name = "John");
			Assert.AreEqual("John", account.Name);
		}

		[TestMethod]
		public void WaitShouldCancelImmediately()
		{
			var timeout = TimeSpan.FromMinutes(30);
			var delay = TimeSpan.FromMilliseconds(25);
			var minimum = TimeSpan.FromMinutes(1);
			var maximum = TimeSpan.FromMinutes(10);
			var count = 0;
			var currentTime = new DateTime(2021, 06, 23, 09, 37, 45, DateTimeKind.Utc);

			TimeService.UtcNowProvider = () => currentTime;

			var watch = Stopwatch.StartNew();
			var actual = UtilityExtensions.Wait(() =>
			{
				count++;
				return true;
			}, timeout, delay, minimum, maximum, true);
			watch.Stop();

			watch.Elapsed.Dump();

			Assert.AreEqual(1, count);
			Assert.AreEqual(false, actual);
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 4, 1000, 50);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 150, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 200, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 4, 500, 10);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 30, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 50, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldShouldRunAsFastAsPossiblyWithNoDelay()
		{
			var timeout = TimeSpan.FromSeconds(30);
			var delay = TimeSpan.Zero;
			var minimum = TimeSpan.FromSeconds(1);
			var maximum = TimeSpan.FromSeconds(60);
			var count = 0;
			var currentTime = new DateTime(2021, 06, 23, 09, 37, 00, DateTimeKind.Utc);

			TimeService.UtcNowProvider = () => currentTime;

			var watch = Stopwatch.StartNew();
			var actual = UtilityExtensions.Wait(() =>
			{
				if (count > 0)
				{
					currentTime += TimeSpan.FromSeconds(1);
				}
				count++;
				return false;
			}, timeout, delay, minimum, maximum, true);
			watch.Stop();

			watch.Elapsed.Dump();

			Assert.AreEqual(31, count);
			Assert.AreEqual(new DateTime(2021, 06, 23, 09, 37, 30, DateTimeKind.Utc), currentTime);
			Assert.AreEqual(true, actual);
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 100, 1000, 50);
			watch.Stop();
			count.Dump();
			Assert.IsFalse(result);
			Assert.IsTrue(count <= 22);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 1000, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1100, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 100, 500, 100, true);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(7, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 500, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 600, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitWithValueGreaterThanMaximum()
		{
			var timeout = TimeSpan.FromMinutes(30);
			var delay = TimeSpan.FromMilliseconds(25);
			var minimum = TimeSpan.FromMinutes(1);
			var maximum = TimeSpan.FromMinutes(10);
			var count = 0;
			var currentTime = new DateTime(2021, 06, 23, 09, 37, 45, DateTimeKind.Utc);

			TimeService.UtcNowProvider = () => currentTime;

			var actual = UtilityExtensions.Wait(() =>
			{
				currentTime += TimeSpan.FromSeconds(60);
				count++;
				return false;
			}, timeout, delay, minimum, maximum, true);

			Assert.AreEqual(11, count);
			Assert.AreEqual(true, actual);
		}

		[TestMethod]
		public void WaitWithValueLessThanMinimum()
		{
			var timeout = TimeSpan.FromSeconds(1);
			var delay = TimeSpan.FromMilliseconds(25);
			var minimum = TimeSpan.FromMinutes(1);
			var maximum = TimeSpan.FromMinutes(10);
			var count = 0;
			var currentTime = new DateTime(2021, 06, 23, 09, 37, 45, DateTimeKind.Utc);

			TimeService.UtcNowProvider = () => currentTime;

			var actual = UtilityExtensions.Wait(() =>
			{
				currentTime += TimeSpan.FromSeconds(10);
				count++;
				return false;
			}, timeout, delay, minimum, maximum, true);

			Assert.AreEqual(7, count);
			Assert.AreEqual(true, actual);
		}

		#endregion
	}
}