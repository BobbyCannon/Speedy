﻿#region References

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class UtilityExtensionsTests : SpeedyUnitTest
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

		ExpectedException<Exception>(() =>
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

		ExpectedException<Exception>(() =>
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
		var count = 0;

		SetTime(new DateTime(2021, 06, 23, 09, 37, 45, DateTimeKind.Utc));

		var watch = Stopwatch.StartNew();
		var actual = UtilityExtensions.WaitUntil(() =>
		{
			count++;
			return true;
		}, timeout, delay, minimum, true);
		watch.Stop();

		watch.Elapsed.Dump();

		Assert.AreEqual(1, count);
		Assert.AreEqual(true, actual);
	}

	[TestMethod]
	public void WaitShouldCompleteWhenConditionIsSatisfied()
	{
		var count = 0;
		var watch = Stopwatch.StartNew();
		var result = UtilityExtensions.WaitUntil(() => ++count > 4, 1000, 50);
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
		var result = UtilityExtensions.WaitUntil(() => ++count > 4, 500, 10);
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
		var count = 0;

		SetTime(new DateTime(2021, 06, 23, 09, 37, 00, DateTimeKind.Utc));

		var watch = Stopwatch.StartNew();
		var actual = UtilityExtensions.WaitUntil(() =>
		{
			if (count > 0)
			{
				IncrementTime(TimeSpan.FromSeconds(1));
			}
			count++;
			return false;
		}, timeout, delay, minimum, true);
		watch.Stop();

		watch.Elapsed.Dump();

		Assert.AreEqual(31, count);
		Assert.AreEqual(new DateTime(2021, 06, 23, 09, 37, 30, DateTimeKind.Utc), CurrentTime);
		Assert.AreEqual(false, actual);
	}

	[TestMethod]
	public void WaitWithValueLessThanMinimum()
	{
		var timeout = TimeSpan.FromSeconds(1);
		var delay = TimeSpan.FromMilliseconds(25);
		var minimum = TimeSpan.FromMinutes(1);
		var count = 0;
		var startedOn = new DateTime(2021, 06, 23, 09, 37, 45, DateTimeKind.Utc);

		SetTime(startedOn);

		var actual = UtilityExtensions.WaitUntil(() =>
			{
				IncrementTime(TimeSpan.FromSeconds(10));
				count++;
				return false;
			},
			timeout, delay, minimum, true
		);

		Assert.AreEqual(7, count);
		Assert.AreEqual(false, actual);
		// Time should be 1m (minimum) + one extra 10 seconds for the final loop
		Assert.AreEqual(startedOn.Add(minimum).AddSeconds(10), TimeService.UtcNow);
	}

	#endregion
}