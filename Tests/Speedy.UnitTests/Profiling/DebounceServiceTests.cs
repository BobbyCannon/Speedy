#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling;

[TestClass]
public class DebounceServiceTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void DebounceShouldDefaultToUseDateTime()
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
	public void DebounceShouldOnlyFireOnLastTriggeredData()
	{
		var actual = new List<int>();
		var delay = TimeSpan.FromMilliseconds(25);

		void work(CancellationToken token, int data)
		{
			Console.WriteLine(data);
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(delay, work, true))
		{
			actual.Clear();

			service.Trigger(1);
			service.Trigger(2);
			service.Trigger(3);

			IncrementTime(delay);
			var r = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(r, "Should not have timed out... debounce never completed...");
			IsFalse(service.IsTriggered);

			AreEqual(1, actual.Count);
			AreEqual(3, actual[0]);
		}
	}

	[TestMethod]
	public void DebounceShouldOnlyFireWhenTimeChanges()
	{
		var actual = new List<int>();
		var delay = TimeSpan.FromMilliseconds(25);

		void work(CancellationToken token, int data)
		{
			Console.WriteLine(data);
			actual.Add(data);
		}

		foreach (var service in GetServices<int>(delay, work, true))
		{
			actual.Clear();

			service.Trigger(1);
			service.Trigger(2);
			IncrementTime(delay);
			var r = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(r, "Should not have timed out... debounce never completed...");
			IsFalse(service.IsTriggered);

			service.Trigger(3);
			service.Trigger(4);
			IncrementTime(delay);

			r = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(r, "Should not have timed out... debounce never completed...");
			IsFalse(service.IsTriggered);

			AreEqual(2, actual.Count);
			AreEqual(2, actual[0]);
			AreEqual(4, actual[1]);
		}
	}

	[TestMethod]
	public void DebounceShouldOnlyFireWhenTimeChangesEnough()
	{
		var actual = new List<int>();
		var count = 1;

		void work(CancellationToken token)
		{
			actual.Add(count++);
		}

		foreach (var service in GetServices(TimeSpan.FromMilliseconds(25), work, true))
		{
			// ReSharper disable once RedundantAssignment
			count = 1;
			actual.Clear();
			service.Trigger(1);

			IncrementTime(TimeSpan.FromMilliseconds(24));
			IsTrue(service.IsTriggered);
			AreEqual(TimeSpan.FromMilliseconds(1), service.TimeToNextTrigger);
			IncrementTime(TimeSpan.FromMilliseconds(1));

			var r = this.WaitUntil(() => !service.IsTriggered, 1000, 10);
			IsTrue(r, "Should not have timed out... debounce never completed...");
			IsFalse(service.IsTriggered);

			AreEqual(1, actual.Count);
			AreEqual(1, actual[0]);
		}
	}

	[TestMethod]
	public void TriggerDebounceWhenAlreadyProcessing()
	{
		var actual = new List<int>();
		var expected = new List<int> { 1, 2 };
		DebounceServiceBase currentService = null;

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

	private IEnumerable<DebounceServiceBase> GetServices(TimeSpan delay, Action<CancellationToken> action)
	{
		yield return new DebounceService(delay, action);
		//yield return new DebounceServiceAsync(delay, c => Task.Run(() => action(c)));
	}

	private IEnumerable<DebounceServiceBase> GetServices(TimeSpan delay, Action<CancellationToken> action, bool useTimeService)
	{
		yield return new DebounceService(delay, action, useTimeService);
		//yield return new DebounceServiceAsync(delay, c => Task.Run(() => action(c)), useTimeService);
	}

	private IEnumerable<DebounceServiceBase> GetServices<T>(TimeSpan delay, Action<CancellationToken, T> action, bool useTimeService)
	{
		yield return new DebounceService<T>(delay, action, useTimeService).Dump();
		//yield return new DebounceServiceAsync<T>(delay, (c, t) => Task.Run(() => action(c, t)), useTimeService).Dump();
	}

	#endregion
}