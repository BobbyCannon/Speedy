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
		public void WaitShouldCompleteWhenConditionIsSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 3, 1000, 50);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 150, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 200, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 3, 500, 10);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 30, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 50, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 100, 1000, 50);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.IsTrue(count <= 20);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 1000, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1100, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 100, 500, 100);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 500, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 600, watch.Elapsed.TotalMilliseconds.ToString());
		}

		#endregion
	}
}