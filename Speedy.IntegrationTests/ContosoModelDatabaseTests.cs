#region References

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.Data.Client;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.Website.Data;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class ContosoModelDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void DetectSyncableRepositories()
		{
			var provider = GetDatabaseProvider();
			using (var database = provider.GetDatabase())
			{
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				Assert.AreEqual(4, repositories.Count);
				Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[3].TypeName);
			}

			provider = GetDatabaseProvider();
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

			provider = GetDatabaseProvider();
			using (var database = provider.GetDatabase())
			{
				database.Options.SyncOrder = new string[0];
				var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
				repositories.ForEach(x => x.TypeName.Dump());
				Assert.AreEqual(4, repositories.Count);
				Assert.AreEqual("Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data", repositories[0].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[1].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[2].TypeName);
				Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[3].TypeName);
			}
		}

		[TestMethod]
		public void DetectSyncableRepository()
		{
			var provider = GetDatabaseProvider();
			using var database = provider.GetDatabase();
			var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
			Assert.AreEqual(4, repositories.Count);
			Assert.AreEqual("Speedy.Website.Data.Entities.AddressEntity,Speedy.Website.Data", repositories[0].TypeName);
			Assert.AreEqual("Speedy.Website.Data.Entities.SettingEntity,Speedy.Website.Data", repositories[1].TypeName);
			Assert.AreEqual("Speedy.Website.Data.Entities.AccountEntity,Speedy.Website.Data", repositories[2].TypeName);
			Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", repositories[3].TypeName);
		}

		[TestMethod]
		public void SortShouldNotBreakLocalRepository()
		{
			using var database = GetDatabase();
			database.Addresses.Add(GetAddress());
			Assert.AreEqual(0, database.Addresses.Count());
			database.SaveChanges();
			Assert.AreEqual(1, database.Addresses.Count());

			var methods = database.Addresses.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
			var sortMethod = methods.First(x => x.Name == "Sort");
			sortMethod.Invoke(database.Addresses, new object[0]);

			methods = database.Accounts.GetType().GetCachedMethods(BindingFlags.Public | BindingFlags.Instance);
			sortMethod = methods.First(x => x.Name == "Sort");
			sortMethod.Invoke(database.Accounts, new object[0]);

			var address = database.Addresses.First();
			address.Accounts.Add(GetPerson());
			Assert.AreEqual(0, database.Accounts.Count());
			database.SaveChanges();
			Assert.AreEqual(1, database.Accounts.Count());
		}

		private ClientAddress GetAddress()
		{
			return new()
			{
				City = "City"
			};
		}

		private static ContosoClientMemoryDatabase GetDatabase(DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
		{
			return new(options, keyCache);
		}

		private static IDatabaseProvider<IContosoDatabase> GetDatabaseProvider(DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
		{
			return TestHelper.GetMemoryProvider(options, keyCache);
		}

		private ClientAccount GetPerson()
		{
			return new()
			{
				Name = "John"
			};
		}

		#endregion
	}
}