#region References

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests.Profiling;

[TestClass]
public class ThrottleServiceTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void ThrottleShouldAllowTimeServiceGoBackInTime()
	{
		var actual = new List<int>();

		void Work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		AreEqual(TimeSpan.FromSeconds(1), service.TimeToNextTrigger);

		IsTrue(service.IsTriggered);
		IncrementTime(seconds: -1);

		Wait(() => service.TimeToNextTrigger.TotalSeconds == 1.0);

		AreEqual(TimeSpan.FromSeconds(1), service.TimeToNextTrigger);
		AreEqual(0, actual.Count);
	}

	[TestMethod]
	public void ThrottleShouldClearQueueWhenPropertyChanges()
	{
		var actual = new List<int>();

		void Work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.QueueTriggers = true;
		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		service.QueueTriggers = false;
		service.Trigger(4);

		IncrementTime(seconds: 1);
		Wait(() => !service.IsTriggered && (actual.Count > 0), 1000, 10);

		IsFalse(service.IsTriggered);
		AreEqual(1, actual.Count);
		AreEqual(4, actual[0]);
	}

	[TestMethod]
	public void ThrottleShouldOnlyRunLastTriggerData()
	{
		var actual = new List<int>();

		void Work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		IsTrue(service.IsTriggered);
		IncrementTime(seconds: 1);
		Wait(() => !service.IsTriggered && (actual.Count > 0), 1000, 10);

		IsFalse(service.IsTriggered);
		AreEqual(1, actual.Count);
		AreEqual(3, actual[0]);
	}

	[TestMethod]
	public void ThrottleShouldProcessAllTriggeredData()
	{
		var actual = new List<int>();

		void Work(CancellationToken token, int data)
		{
			Console.WriteLine(data);
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.QueueTriggers = true;
		
		// {12/29/2022 8:00:00 AM}
		TimeService.UtcNow.Dump();

		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		var actualCount = actual.Count;
		Wait(() => service.IsTriggered);
		// increment to {12/29/2022 8:00:01 AM}
		IncrementTime(seconds: 1);
		Wait(() => actual.Count > actualCount);
		
		actualCount = actual.Count;
		Wait(() => service.IsTriggered);
		// increment to {12/29/2022 8:00:02 AM}
		IncrementTime(seconds: 1);
		Wait(() => actual.Count > actualCount);
		
		actualCount = actual.Count;
		Wait(() => service.IsTriggered);
		// increment to {12/29/2022 8:00:03 AM}
		IncrementTime(seconds: 1);
		Wait(() => actual.Count >= 3);

		// should still be {12/29/2022 8:00:03 AM}
		TimeService.UtcNow.Dump();

		IsFalse(service.IsTriggered);
		AreEqual(3, actual.Count);
		AreEqual(1, actual[0]);
		AreEqual(2, actual[1]);
		AreEqual(3, actual[2]);
	}

	[TestMethod]
	public void ThrottleShouldProcessForceTrigger()
	{
		var actual = new List<int>();

		void Work(CancellationToken token, int data)
		{
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.QueueTriggers = false;
		service.Trigger(1, true);

		IsTrue(service.IsTriggered);
		Wait(() => !service.IsTriggered);

		AreEqual(1, actual.Count);
	}

	#endregion
}