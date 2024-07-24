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
	public void DebounceCanCancel()
	{
		var triggered = false;
		using var service = new DebounceService(TimeSpan.FromSeconds(5), _ => triggered = true, this);
		service.Trigger();
		IncrementTime(seconds: 4);
		service.Cancel();
		IncrementTime(seconds: 1);
		IsFalse(triggered);
	}

	[TestMethod]
	public void DebounceShouldDefaultToUseDateTime()
	{
		// Pause time forever
		PauseTime();

		using var service = new DebounceService<int>(TimeSpan.FromMilliseconds(10), (_, _) => { });
		var watch = Stopwatch.StartNew();
		service.Trigger(1);

		IsTrue(service.IsTriggered, "Service was not triggered but should have been.");

		var result = this.WaitUntil(() => !service.IsActive, 1000, 10);
		watch.Stop();

		// Should never be greater than 100 ms +- 10/15ms, if so it's locked with
		// TimeService and the wait expired.
		IsTrue(result, "The wait timed out... it should not have...");
		IsFalse(watch.Elapsed.TotalMilliseconds > 250);
		watch.Elapsed.Dump();
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

		using var service = new DebounceService<int>(delay, work, this);
		actual.Clear();

		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		IncrementTime(delay);
		var r = this.WaitUntil(() => !service.IsActive, 1000, 10);
		IsTrue(r, "Should not have timed out... debounce never completed...");
		IsFalse(service.IsTriggered);

		AreEqual(1, actual.Count);
		AreEqual(3, actual[0]);
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

		using var service = new DebounceService<int>(delay, work, this);
		actual.Clear();

		service.Trigger(1);
		service.Trigger(2);
		IncrementTime(delay);
		var r = this.WaitUntil(() => !service.IsActive, 1000, 10);
		IsTrue(r, "Should not have timed out... debounce never completed...");
		IsFalse(service.IsTriggered);

		service.Trigger(3);
		service.Trigger(4);
		IncrementTime(delay);

		r = this.WaitUntil(() => !service.IsActive, 1000, 10);
		IsTrue(r, "Should not have timed out... debounce never completed...");
		IsFalse(service.IsTriggered);

		AreEqual(2, actual.Count);
		AreEqual(2, actual[0]);
		AreEqual(4, actual[1]);
	}

	[TestMethod]
	public void DebounceShouldOnlyFireWhenTimeChangesEnough()
	{
		var actual = new List<int>();
		var count = 1;

		void work(CancellationToken token, int value)
		{
			actual.Add(count++);
		}

		using var service = new DebounceService<int>(TimeSpan.FromSeconds(10), work, this);
		count = 1;
		actual.Clear();
		service.Trigger(1);

		IncrementTime(TimeSpan.FromSeconds(6));
		IsTrue(service.IsTriggered);
		AreEqual(TimeSpan.FromSeconds(4), service.TimeToNextTrigger);
		IncrementTime(TimeSpan.FromSeconds(4));

		var r = this.WaitUntil(() => !service.IsActive, 1000, 10);
		IsTrue(r, "Should not have timed out... debounce never completed...");
		IsFalse(service.IsTriggered);

		AreEqual(1, actual.Count);
		AreEqual(1, actual[0]);
	}

	[TestMethod]
	public void DebounceShouldTrigger()
	{
		PauseTime();

		var triggered = false;
		using var service = new DebounceService(TimeSpan.FromSeconds(5), _ => triggered = true, this);
		service.Trigger();
		IncrementTime(seconds: 5);
		this.WaitUntil(() => triggered, 1000, 10);
		IsTrue(triggered);
	}

	[TestMethod]
	public void TriggerDebounceWhenAlreadyProcessing()
	{
		var actual = new List<int>();
		var expected = new List<int> { 1 };
		DebounceService<int> currentService = null;

		void work(CancellationToken token, int data)
		{
			if (data == 1)
			{
				currentService?.Trigger(2);
			}

			Console.WriteLine(data);
			actual.Add(data);
		}

		using var service = new DebounceService<int>(TimeSpan.FromMilliseconds(25), work);
		// ReSharper disable once RedundantAssignment
		currentService = service;
		actual.Clear();
		service.Trigger(1);

		IsTrue(service.IsTriggered);
		var b = this.WaitUntil(() => !service.IsActive, 1000, 10);

		IsTrue(b, "Should not have timed out... process not running...");
		AreEqual(expected, actual);
	}

	private void PauseTime()
	{
		ResetCurrentTime(new DateTime(2023, 07, 31, 01, 40, 12, DateTimeKind.Utc));
	}

	#endregion
}