#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests.Logging
{
	[TestClass]
	public class EventTrackerTests
	{
		#region Methods

		[TestMethod]
		public void InitialSessionEventShouldBeAddedOnStart()
		{
			var repository = new MemoryEventRepository();
			var provider = new KeyValueMemoryRepositoryProvider<Event>();
			var tracker = EventTracker.Start(repository, provider);
			tracker.Dispose();

			Assert.AreEqual(1, repository.Events.Count);
			Assert.AreEqual("Session", repository.Events[0].Name);

			var expectedAssembly = typeof(EventTracker).Assembly.GetName();
			var expectedVersion = expectedAssembly.Version;
			var values = repository.Events[0].Values.ToList();

			Assert.AreEqual(4, values.Count);
			Assert.AreEqual(".NET Version", values[0].Name);
			Assert.AreEqual(Environment.Version.ToString(), values[0].Value);
			Assert.AreEqual("Application Bitness", values[1].Name);
			Assert.AreEqual(Environment.Is64BitProcess ? "64" : "32", values[1].Value);
			Assert.AreEqual("Application Name", values[2].Name);
			Assert.AreEqual(expectedAssembly.Name, values[2].Value);
			Assert.AreEqual("Application Version", values[3].Name);
			Assert.AreEqual(expectedVersion.ToString(), values[3].Value);
		}

		[TestMethod]
		public void VersionShouldBeCorrect()
		{
			var expected = typeof(EventTracker).Assembly.GetName().Version;
			Assert.AreEqual(expected, EventTracker.Version);
		}

		#endregion
	}
}