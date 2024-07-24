#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Profiling;
using Speedy.Runtime;

#endregion

namespace Speedy.UnitTests.Profiling;

[TestClass]
public class ThrottleServiceTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ThrottleShouldClearQueueWhenPropertyChanges()
	{
		var actual = new List<int>();

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		ForEachService<int>(TimeSpan.FromSeconds(10), work, this, service =>
		{
			actual.Clear();

			service.SetMemberValue(x => x.TriggeredOn, StartDateTime.AddSeconds(-1));
			service.QueueTriggers = true;
			service.Trigger(1);
			service.Trigger(2);
			service.Trigger(3);
			AreEqual(3, service.QueueCount);

			service.QueueTriggers = false;
			service.Trigger(4);

			AreEqual(1, service.QueueCount);
			IncrementTime(seconds: 10);

			service.WaitUntil(x => !x.IsTriggered && (actual.Count > 0), 1000, 10);

			IsFalse(service.IsTriggered);
			AreEqual(1, actual.Count);
			AreEqual(4, actual[0]);
		});
	}

	[TestMethod]
	public void ThrottleShouldDefaultToUseDateTime()
	{
		// Pause time forever
		ResetCurrentTime(new DateTime(2023, 07, 31, 01, 40, 12, DateTimeKind.Utc));

		ForEachService<int>(TimeSpan.FromSeconds(10), (_, _) => { }, null, service =>
		{
			var watch = Stopwatch.StartNew();
			service.Trigger(1);

			IsTrue(service.IsTriggered, "Service was not triggered but should have been.");

			var result = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			watch.Stop();

			// Should never be greater than 100 ms +- 10/15ms, if so it's locked with
			// TimeService and the wait expired.
			IsTrue(result, "The wait timed out... it should not have...");
			IsFalse(watch.Elapsed.TotalMilliseconds > 250);
			watch.Elapsed.Dump();
		});
	}

	[TestMethod]
	public void ThrottleShouldOnlyRunLastTriggerData()
	{
		var actual = new List<int>();

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		ForEachService<int>(TimeSpan.FromSeconds(10), work, this, service =>
		{
			actual.Clear();

			IsFalse(service.IsTriggered);
			AreEqual(0, actual.Count);

			for (var i = 1; i < 10; i++)
			{
				service.Trigger(i);
				AreEqual(0, actual.Count);
				IsTrue(service.IsTriggered);
			}

			IncrementTime(seconds: 10);
			IsTrue(this.WaitUntil(() => !service.IsTriggered && !service.IsProcessing, 1000, 10));
			IsFalse(service.IsTriggered);
			AreEqual(1, actual.Count);
			AreEqual(9, actual[0]);
		});
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

		ForEachService<int>(TimeSpan.FromSeconds(1), work, this, service =>
		{
			actual.Clear();
			expected.Clear();
			service.QueueTriggers = true;

			// {12/29/2022 8:00:00 AM}
			CurrentTime.Dump();

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
			CurrentTime.Dump();

			IsFalse(service.IsTriggered);
			AreEqual(count, actual.Count);
			AreEqual(expected, actual);
		});
	}

	[TestMethod]
	public void ThrottleWithForceShouldProcessNoDelayOption()
	{
		var count = 0;

		ForEachService<int>(TimeSpan.FromSeconds(10), (_, _) => count++, this, service =>
		{
			service.QueueTriggers = false;
			service.Trigger(1, true);

			IsTrue(service.WaitUntil(x => (x.QueueCount <= 0) && (count == 1), 1000, 10));
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);

			// Cannot check this because the trigger will (could) have already been processed
			//IsTrue(service.IsTriggered);
			var b = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(b, "Should not have timed out... process not running...");
			service.Dispose();
		});
	}

	[TestMethod]
	public void Trigger()
	{
		var actual = new List<int>();
		var interval = TimeSpan.FromSeconds(10);

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		ForEachService<int>(interval, work, this, service =>
		{
			service.Trigger(1);

			IsTrue(actual.WaitUntil(x => x.Count == 1, 1000, 10), $"Count should be 1 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(1, actual.Count);

			// Only increment half interval
			IncrementTime(seconds: 5);
			service.Trigger(2);

			AreEqual(TimeSpan.FromSeconds(5), service.TimeToNextTrigger);
			AreEqual(true, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(1, actual.Count);

			// Now finish the interval
			IncrementTime(seconds: 5);

			IsTrue(actual.WaitUntil(x => x.Count == 2, 1000, 10), $"Count should be 2 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(2, actual.Count);

			// Now go past the interval
			IncrementTime(seconds: 11);

			// Now next trigger should be immediate
			service.Trigger(1);

			IsTrue(actual.WaitUntil(x => x.Count == 3, 1000, 10), $"Count should be 3 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(3, actual.Count);
		});
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

		var watch = Stopwatch.StartNew();

		ForEachService<int>(TimeSpan.FromMilliseconds(10), work, null, service =>
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
			AreEqual(expected, actual, $"Elapsed: {watch.Elapsed} : {string.Join(",", actual)}");
		});
	}

	[TestMethod]
	public void TriggerThrottleWhenAlreadyProcessing()
	{
		var actual = new List<int>();
		var expected = new List<int> { 1 };
		ThrottleService<int> currentService = null;

		void work(CancellationToken token, int data)
		{
			if (data == 1)
			{
				// This should never trigger
				currentService?.Trigger(2);
			}

			Console.WriteLine(data);
			actual.Add(data);
		}

		ForEachService<int>(TimeSpan.FromMilliseconds(25), work, null, service =>
		{
			// ReSharper disable once RedundantAssignment
			currentService = service;
			actual.Clear();
			service.Trigger(1);

			IsTrue(service.IsTriggered);
			var b = service.WaitUntil(x => !x.IsTriggered && !x.IsProcessing, 1000, 10);
			IsTrue(b, "Should not have timed out... process not running...");
			AreEqual(expected, actual);
		});
	}

	[TestMethod]
	public void TriggerWithQueue()
	{
		var actual = new List<int>();
		var interval = TimeSpan.FromSeconds(10);

		void work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		ForEachService<int>(interval, work, this, service =>
		{
			service.QueueTriggers = true;
			service.Trigger(1);
			service.Trigger(2);

			IsTrue(actual.WaitUntil(x => x.Count == 1, 1000, 10), $"Count should be 1 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(true, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(1, actual.Count);

			// Only increment half interval
			IncrementTime(seconds: 5);

			AreEqual(1, service.QueueCount);
			AreEqual(TimeSpan.FromSeconds(5), service.TimeToNextTrigger);
			AreEqual(true, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(1, actual.Count);

			// Now finish the interval
			IncrementTime(seconds: 5);

			IsTrue(actual.WaitUntil(x => x.Count == 2, 1000, 10), $"Count should be 2 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(2, actual.Count);

			// Now go past the interval
			IncrementTime(seconds: 11);

			// Now next trigger should be immediate
			service.Trigger(1);

			IsTrue(actual.WaitUntil(x => x.Count == 3, 1000, 10), $"Count should be 3 but was {actual.Count}");
			AreEqual(TimeSpan.FromSeconds(10), service.TimeToNextTrigger);
			AreEqual(false, service.IsTriggered);
			AreEqual(false, service.IsTriggeredAndReadyToProcess);
			AreEqual(false, service.IsReadyToProcess);
			AreEqual(3, actual.Count);
		});
	}

	private void ForEachService<T>(TimeSpan interval, Action<CancellationToken, T> work, IDateTimeProvider timeProvider, Action<ThrottleService<T>> action)
	{
		foreach (var service in GetServices(interval, work, timeProvider))
		{
			try
			{
				action(service);
			}
			finally
			{
				service.Dispose();
			}
		}
	}

	private IEnumerable<ThrottleService> GetServices(TimeSpan interval, Action<CancellationToken> action, IDateTimeProvider timeService)
	{
		yield return new ThrottleService(interval, action, timeService);
		//yield return new ThrottleServiceAsync(interval, c => Task.Run(() => action(c)), useTimeService);
	}

	private IEnumerable<ThrottleService<T>> GetServices<T>(TimeSpan interval, Action<CancellationToken, T> action, IDateTimeProvider timeService)
	{
		yield return new ThrottleService<T>(interval, action, timeService);
		//yield return new ThrottleServiceAsync<T>(interval, (c, t) => Task.Run(() => action(c, t)), useTimeService).Dump();
	}

	#endregion
}