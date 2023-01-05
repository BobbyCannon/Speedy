#region References

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

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
			actual.Add(data);
		}

		using var service = new ThrottleService<int>(TimeSpan.FromSeconds(1), Work);
		service.QueueTriggers = true;

		service.Trigger(1);
		service.Trigger(2);
		service.Trigger(3);

		while (service.IsTriggered)
		{
			IncrementTime(seconds: 1);
			Wait(() => !service.IsTriggered, 50, 10);
		}

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

		Wait(() => !service.IsTriggered, 50, 10);

		AreEqual(1, actual.Count);
	}

	#endregion
}