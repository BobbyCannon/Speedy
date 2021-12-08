#region References

using System.Threading;
using Speedy.Profiling;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.Sandbox
{
	internal class Program
	{
		#region Methods

		private static void Main(string[] args)
		{
			var repository = new MemoryTrackerPathRepository();
			using var tracker = new Tracker(repository, new KeyValueMemoryRepositoryProvider<TrackerPath>());
			tracker.Start();

			while (true)
			{
				using var path = tracker.StartNewPath("Test");
				Thread.Sleep(10);
			}
		}

		#endregion
	}
}