#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.Website.Samples;

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
				Assert.AreEqual("Speedy.Website.Samples.Entities.AddressEntity,Speedy.Website.Samples", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.SettingEntity,Speedy.Website.Samples", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.AccountEntity,Speedy.Website.Samples", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.LogEventEntity,Speedy.Website.Samples", repositories[3].TypeName);
			}

			using (var database = provider.GetDatabase())
			{
				var order = database.Options.SyncOrder.ToList();
				order.RemoveAt(2);
				database.Options.SyncOrder = order.ToArray();
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				Assert.AreEqual(3, repositories.Count);
				Assert.AreEqual("Speedy.Website.Samples.Entities.AddressEntity,Speedy.Website.Samples", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.SettingEntity,Speedy.Website.Samples", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.LogEventEntity,Speedy.Website.Samples", repositories[2].TypeName);
			}
			
			using (var database = provider.GetDatabase())
			{
				database.Options.SyncOrder = new string[0];
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				repositories.ForEach(x => x.TypeName.Dump());
				Assert.AreEqual(6, repositories.Count);
				Assert.AreEqual("Speedy.Website.Samples.Entities.AccountEntity,Speedy.Website.Samples", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.AddressEntity,Speedy.Website.Samples", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.LogEventEntity,Speedy.Website.Samples", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.SettingEntity,Speedy.Website.Samples", repositories[3].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.TrackerPathConfigurationEntity,Speedy.Website.Samples", repositories[4].TypeName);
				Assert.AreEqual("Speedy.Website.Samples.Entities.TrackerPathEntity,Speedy.Website.Samples", repositories[5].TypeName);
			}
		}

		private static IDatabaseProvider<ContosoDatabase> GetDatabase(DatabaseOptions options = null)
		{
			return TestHelper.GetSqlProvider(options);
		}

		#endregion
	}
}