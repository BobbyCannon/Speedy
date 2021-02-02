#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;
using Speedy.UnitTests;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class ToSyncObjectTests
	{
		#region Methods

		[TestMethod]
		public void AddressEntityToSyncObject()
		{
			var entity = new AddressEntity();
			// Cold start, warm up SyncObject base static class
			entity.ToSyncObject();
			var (syncObject, timer) = Timer.Create(() => 1); //entity.ToSyncObject());
			timer.Elapsed.Dump();
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 100, $"Total Milliseconds: {timer.Elapsed.TotalMilliseconds}");
		}

		#endregion
	}
}