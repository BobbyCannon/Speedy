#region References

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class ExtensionTests
	{
		#region Methods

		[TestMethod]
		public void RetryShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			Extensions.Retry(() =>
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
				Extensions.Retry(() =>
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
			var result = Extensions.Retry(() =>
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
				result = Extensions.Retry(() =>
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
		public void WaitShouldCompleteWhenConditionIsSatified()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 3, 1000, 50);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 150, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 200, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatifiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 3, 500, 10);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 30, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 40, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatified()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 100, 1000, 50);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(20, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 1000, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1050, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatifiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 100, 500, 100);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 500, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 510, watch.Elapsed.TotalMilliseconds.ToString());
		}

		#endregion
	}
}