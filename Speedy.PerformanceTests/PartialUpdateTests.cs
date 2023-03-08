#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Profiling;
using Speedy.Serialization;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class PartialUpdateTests
	{
		#region Methods

		[TestMethod]
		public void ShouldBeFast()
		{
			var addressJson = EntityFactory.GetAddress().ToJson();
			addressJson.Dump();
			var (update, timer) = Timer.Create(() => PartialUpdate.FromJson<AddressEntity>(addressJson));
			timer.Elapsed.Dump();
			
			#if NET48
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 75.0, $"Total Milliseconds: {timer.Elapsed.TotalMilliseconds}");
			Assert.IsNotNull(update);
			#else
			Assert.IsTrue(timer.Elapsed.TotalMilliseconds < 50.0, $"Total Milliseconds: {timer.Elapsed.TotalMilliseconds}");
			Assert.IsNotNull(update);
			#endif
		}

		#endregion
	}
}