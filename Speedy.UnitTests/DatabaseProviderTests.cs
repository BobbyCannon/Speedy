#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.WebApi;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using System.Linq;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class DatabaseProviderTests : BaseTests
	{
		[TestMethod]
		public void BulkProcess()
		{
			var memoryDatabase = new ContosoMemoryDatabase();
			var provider = new DatabaseProvider<ContosoMemoryDatabase>(x => 
			{
				memoryDatabase.Options.UpdateWith(x);
				return memoryDatabase;
			}, ContosoDatabase.GetDefaultOptions());

			provider.BulkProcess(100, 10, (i, database) => {
				database.LogEvents.Add(new LogEventEntity { Level = LogLevel.Debug, Message = i.ToString() });
			});

			Assert.AreEqual(100, memoryDatabase.LogEvents.Count());
			
			provider.BulkProcess(37, 5, (i, database) => {
				database.LogEvents.Add(new LogEventEntity { Level = LogLevel.Debug, Message = i.ToString() });
			});

			Assert.AreEqual(137, memoryDatabase.LogEvents.Count());
		}
	}
}