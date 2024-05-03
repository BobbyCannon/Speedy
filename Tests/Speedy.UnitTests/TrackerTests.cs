#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class TrackerTests
	{
		#region Methods

		[TestMethod]
		public void InitializeWithAdditionalValues()
		{
			var repository = new MemoryTrackerPathRepository();
			var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Initialize(new TrackerPathValue("Foo", "Bar"));
			tracker.Dispose();

			Assert.AreEqual(1, repository.Paths.Count);
			Assert.AreEqual(5, repository.Paths[0].Values.Count);

			var application = Tracker.AssemblyName;
			Assert.IsNotNull(application);
			Assert.AreEqual(new TrackerPathValue(".NET Version", Environment.Version), repository.Paths[0].Values[0]);
			Assert.AreEqual(new TrackerPathValue("Application Bitness", Environment.Is64BitProcess ? "64" : "32"), repository.Paths[0].Values[1]);
			Assert.AreEqual(new TrackerPathValue("Application Name", application.Name), repository.Paths[0].Values[2]);
			Assert.AreEqual(new TrackerPathValue("Application Version", application.Version?.ToString()), repository.Paths[0].Values[3]);
			Assert.AreEqual(new TrackerPathValue("Foo", "Bar"), repository.Paths[0].Values[4]);
		}

		[TestMethod]
		public void InitialValues()
		{
			var repository = new MemoryTrackerPathRepository();
			var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Initialize();
			tracker.Dispose();

			Assert.AreEqual(1, repository.Paths.Count);
			Assert.AreEqual(4, repository.Paths[0].Values.Count);

			var application = Tracker.AssemblyName;
			Assert.IsNotNull(application);
			Assert.AreEqual(new TrackerPathValue(".NET Version", Environment.Version), repository.Paths[0].Values[0]);
			Assert.AreEqual(new TrackerPathValue("Application Bitness", Environment.Is64BitProcess ? "64" : "32"), repository.Paths[0].Values[1]);
			Assert.AreEqual(new TrackerPathValue("Application Name", application.Name), repository.Paths[0].Values[2]);
			Assert.AreEqual(new TrackerPathValue("Application Version", application.Version?.ToString()), repository.Paths[0].Values[3]);
		}

		[TestMethod]
		public void StartNewPathAbort()
		{
			var repository = new MemoryTrackerPathRepository();
			var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Initialize();
			Assert.AreEqual(0, repository.Paths.Count);

			var value = tracker.StartNewPath("Test", new TrackerPathValue("Foo", "Bar"));
			value.Abort();
			value.Dispose();
			tracker.Dispose();

			Assert.AreEqual(1, repository.Paths.Count);
			Assert.AreEqual("Session", repository.Paths[0].Name);
		}
		
		[TestMethod]
		public void StartNewPathComplete()
		{
			var repository = new MemoryTrackerPathRepository();
			var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Initialize();
			Assert.AreEqual(0, repository.Paths.Count);

			var value = tracker.StartNewPath("Test", new TrackerPathValue("Foo", "Bar"));
			value.Complete();
			value.Dispose();
			tracker.Dispose();

			Assert.AreEqual(2, repository.Paths.Count);
			Assert.AreEqual("Session", repository.Paths[0].Name);
			Assert.AreEqual("Test", repository.Paths[1].Name);
		}
		
		[TestMethod]
		public void StartNewPathDisposeOnly()
		{
			var repository = new MemoryTrackerPathRepository();
			var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Initialize();
			Assert.AreEqual(0, repository.Paths.Count);

			var value = tracker.StartNewPath("Test", new TrackerPathValue("Foo", "Bar"));
			value.Dispose();
			tracker.Dispose();

			Assert.AreEqual(2, repository.Paths.Count);
			Assert.AreEqual("Session", repository.Paths[0].Name);
			Assert.AreEqual("Test", repository.Paths[1].Name);
		}

		#endregion
	}
}