#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests.Profiling
{
	[TestClass]
	public class TrackerPathTests
	{
		#region Methods

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