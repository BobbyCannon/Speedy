﻿#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class TrackerPathTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void CompleteShouldWork()
		{
			SetTime(new DateTime(2020, 12, 15, 01, 02, 03, 999, DateTimeKind.Utc));

			var path = new TrackerPath();
			Assert.AreEqual(false, path.IsCompleted);
			Assert.AreEqual(0, path.ElapsedTime.Ticks);

			IncrementTime(TimeSpan.FromMilliseconds(1));

			path.Complete();
			Assert.AreEqual(true, path.IsCompleted);
			Assert.AreEqual(1, path.ElapsedTime.TotalMilliseconds);
		}

		[TestMethod]
		public void DisposeShouldComplete()
		{
			var repository = new MemoryTrackerPathRepository();
			var provider = new KeyValueMemoryRepositoryProvider<TrackerPath>();
			Assert.AreEqual(0, repository.Paths.Count);

			var tracker = Tracker.Start(repository, provider);
			var path = tracker.StartNewPath("Test");
			path.Dispose();
			tracker.Dispose();

			Assert.AreEqual(2, repository.Paths.Count);
			Assert.AreEqual("Session", repository.Paths[0].Name);
			Assert.AreEqual("Test", repository.Paths[1].Name);
		}

		#endregion
	}
}