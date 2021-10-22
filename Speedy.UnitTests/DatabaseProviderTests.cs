#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.WebApi;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class DatabaseProviderTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void BulkProcess()
		{
			var memoryDatabase = new ContosoMemoryDatabase();
			var provider = new DatabaseProvider<ContosoMemoryDatabase>(x =>
			{
				memoryDatabase.Options.UpdateWith(x);
				return memoryDatabase;
			}, ContosoDatabase.GetDefaultOptions());

			provider.BulkProcess(100, 10, (i, database) => { database.LogEvents.Add(new LogEventEntity { Level = LogLevel.Debug, Message = i.ToString() }); });

			Assert.AreEqual(100, memoryDatabase.LogEvents.Count());

			provider.BulkProcess(37, 5, (i, database) => { database.LogEvents.Add(new LogEventEntity { Level = LogLevel.Debug, Message = i.ToString() }); });

			Assert.AreEqual(137, memoryDatabase.LogEvents.Count());
		}

		#endregion
	}
}