#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling;

[TestClass]
public class ThrottleServiceTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ThrottleReset()
	{
		var actual = new List<int>();
		var delay = TimeSpan.FromMilliseconds(10);

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(delay, work, true))
		{
			service.QueueTriggers = true;
			service.Trigger(1);
			service.Trigger(2);
			service.Trigger(3);

			AreEqual(TimeSpan.FromMilliseconds(10), service.TimeToNextTrigger);
			AreEqual(true, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(0, actual.Count);

			IsTrue(service.IsTriggered);
			service.Reset();

			AreEqual(TimeSpan.Zero, service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(0, actual.Count);
		}
	}

	[TestMethod]
	public void ThrottleShouldClearQueueWhenPropertyChanges()
	{
		var actual = new List<int>();

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(TimeSpan.FromSeconds(1), work, true))
		{
			actual.Clear();

			service.QueueTriggers = true;
			service.Trigger(1);
			service.Trigger(2);
			service.Trigger(3);

			service.QueueTriggers = false;
			service.Trigger(4);

			IncrementTime(seconds: 1);
			this.WaitUntil(() => !service.IsTriggered && (actual.Count > 0), 1000, 10);

			IsFalse(service.IsTriggered);
			AreEqual(1, actual.Count);
			AreEqual(4, actual[0]);
		}
	}

	[TestMethod]
	public void ThrottleShouldDefaultToUseDateTime()
	{
		// Pause time forever
		ResetCurrentTime(new DateTime(2023, 07, 31, 01, 40, 12, DateTimeKind.Utc));

		foreach (var service in GetServices(TimeSpan.FromMilliseconds(10), _ => { }))
		{
			var watch = Stopwatch.StartNew();
			service.Trigger(1);

			IsTrue(service.IsTriggered, "Service was not triggered but should have been.");

			var result = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			watch.Stop();

			// Should never be be greater than 100 ms +- 10/15ms, if so it's locked with
			// TimeService and the wait expired.
			IsTrue(result, "The wait timed out... it should not have...");
			IsFalse(watch.Elapsed.TotalMilliseconds > 250);
			watch.Elapsed.Dump();
		}
	}

	[TestMethod]
	public void ThrottleShouldOnlyRunLastTriggerData()
	{
		var actual = new List<int>();

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(TimeSpan.FromSeconds(1), work, true))
		{
			actual.Clear();

			service.Trigger(1);
			service.Trigger(2);
			service.Trigger(3);

			IsTrue(service.IsTriggered);
			IncrementTime(seconds: 1);
			this.WaitUntil(() => !service.IsTriggered && (actual.Count > 0), 1000, 10);

			IsFalse(service.IsTriggered);
			AreEqual(1, actual.Count);
			AreEqual(3, actual[0]);
		}
	}

	[TestMethod]
	public void ThrottleShouldProcessAllTriggeredData()
	{
		var actual = new List<int>();
		var expected = new List<int>();
		var count = 2;

		void work(CancellationToken token, int data)
		{
			//Console.WriteLine(data);
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(TimeSpan.FromSeconds(1), work, true))
		{
			actual.Clear();
			expected.Clear();
			service.QueueTriggers = true;

			// {12/29/2022 8:00:00 AM}
			TimeService.UtcNow.Dump();

			for (var i = 1; i <= count; i++)
			{
				service.Trigger(i);
				expected.Add(i);
			}

			while (service.IsTriggered)
			{
				var previousActualCount = actual.Count;
				// increment to {12/29/2022 8:00:01 AM}
				IncrementTime(seconds: 1);
				this.WaitUntil(() => actual.Count > previousActualCount, 1000, 10);
			}

			// should still be {12/29/2022 8:00:03 AM}
			TimeService.UtcNow.Dump();

			IsFalse(service.IsTriggered);
			AreEqual(count, actual.Count);
			AreEqual(expected, actual);
		}
	}

	[TestMethod]
	public void ThrottleShouldProcessNoDelayOption()
	{
		foreach (var service in GetServices(TimeSpan.FromSeconds(1), _ => { }, true))
		{
			service.QueueTriggers = false;
			service.Trigger(1, true);

			AreEqual(TimeSpan.Zero, service.TimeToNextTrigger);

			// Cannot check this because the trigger will (could) have already been processed
			//IsTrue(service.IsTriggered);
			var b = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(b, "Should not have timed out... process not running...");
		}
	}

	[TestMethod]
	public void TriggerShouldAlwaysBeTrueUntilQueueProcessed()
	{
		var actual = new List<int>();
		var expected = new List<int>();
		var count = 25;

		void work(CancellationToken token, int data)
		{
			//Console.WriteLine(data);
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(TimeSpan.FromMilliseconds(1), work, false))
		{
			actual.Clear();
			expected.Clear();
			service.QueueTriggers = true;

			for (var i = 1; i <= count; i++)
			{
				service.Trigger(i);
				expected.Add(i);
			}

			var b = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(b, "Should not have timed out... process not running...");
			AreEqual(expected, actual);
		}
	}

	[TestMethod]
	public void TriggerThrottleWhenAlreadyProcessing()
	{
		var actual = new List<int>();
		var expected = new List<int> { 1, 2 };
		ThrottleServiceBase currentService = null;

		void work(CancellationToken token, int data)
		{
			if (data == 1)
			{
				currentService?.Trigger(2);
			}

			Console.WriteLine(data);
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(TimeSpan.FromMilliseconds(25), work, false))
		{
			// ReSharper disable once RedundantAssignment
			currentService = service;
			actual.Clear();
			service.Trigger(1);

			IsTrue(service.IsTriggered);
			var b = this.WaitUntil(() => !service.IsTriggered, 1000, 10);

			IsTrue(b, "Should not have timed out... process not running...");
			AreEqual(expected, actual);
		}
	}

	private IEnumerable<ThrottleServiceBase> GetServices(TimeSpan delay, Action<CancellationToken> action)
	{
		yield return new ThrottleService(delay, action);
		yield return new ThrottleServiceAsync(delay, c => Task.Run(() => action(c)));
	}

	private IEnumerable<ThrottleServiceBase> GetServices(TimeSpan delay, Action<CancellationToken> action, bool useTimeService)
	{
		yield return new ThrottleService(delay, action, useTimeService);
		yield return new ThrottleServiceAsync(delay, c => Task.Run(() => action(c)), useTimeService);
	}

	private IEnumerable<ThrottleServiceBase> GetServices<T>(TimeSpan delay, Action<CancellationToken, T> action, bool useTimeService)
	{
		yield return new ThrottleService<T>(delay, action, useTimeService).Dump();
		yield return new ThrottleServiceAsync<T>(delay, (c, t) => Task.Run(() => action(c, t)), useTimeService).Dump();
	}

	#endregion
}