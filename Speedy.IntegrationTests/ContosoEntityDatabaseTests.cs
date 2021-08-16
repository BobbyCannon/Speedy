#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.Website.Data;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class ContosoEntityDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void DetectSyncableRepositories()
		{
			var provider = GetDatabase();
			using (var database = provider.GetDatabase())
			{
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				Assert.AreEqual(4, repositories.Count);
				Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[3].TypeName);
			}

			using (var database = provider.GetDatabase())
			{
				var order = database.Options.SyncOrder.ToList();
				order.RemoveAt(2);
				database.Options.SyncOrder = order.ToArray();
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				Assert.AreEqual(3, repositories.Count);
				Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[2].TypeName);
			}

			using (var database = provider.GetDatabase())
			{
				database.Options.SyncOrder = Array.Empty<string>();
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				repositories.ForEach(x => x.TypeName.Dump());
				Assert.AreEqual(6, repositories.Count);
				Assert.AreEqual("Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[3].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.TrackerPathConfigurationEntity,Speedy.Website.Data", repositories[4].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.TrackerPathEntity,Speedy.Website.Data", repositories[5].TypeName);
			}
		}

		private static IDatabaseProvider<ContosoDatabase> GetDatabase(DatabaseOptions options = null)
		{
			return TestHelper.GetSqlProvider(options);
		}

		#endregion
	}
}