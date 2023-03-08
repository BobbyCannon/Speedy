#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Profiling;
using Speedy.UnitTests.Factories;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class SyncObjectTests
	{
		#region Methods

		[TestMethod]
		public void AddressEntityToSyncObject()
		{
			var entity = EntityFactory.GetAddress();
			// Cold start, warm up SyncObject base static class
			entity.ToSyncObject();
			var (syncObject, timer) = Timer.Create(() => entity.ToSyncObject());
			timer.Elapsed.Dump();
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 1.0, $"Total Milliseconds: {timer.Elapsed.TotalMilliseconds}");
			Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", syncObject.TypeName);
		}

		#endregion
	}
}