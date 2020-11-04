#region References

using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.Data.Client;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.Website.Samples;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class ContosoModelDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void DetectSyncableRepository()
		{
			var provider = GetDatabaseProvider();
			using var database = provider.GetDatabase();
			var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
			Assert.AreEqual(4, repositories.Count);
			Assert.AreEqual("Speedy.Website.Samples.Entities.AddressEntity,Speedy.Website.Samples", repositories[0].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.SettingEntity,Speedy.Website.Samples", repositories[1].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.AccountEntity,Speedy.Website.Samples", repositories[2].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.LogEventEntity,Speedy.Website.Samples", repositories[3].TypeName);
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
			return new ClientAddress
			{
				City = "City"
			};
		}

		private static ContosoClientMemoryDatabase GetDatabase(DatabaseOptions options = null)
		{
			return new ContosoClientMemoryDatabase(options);
		}

		private static IDatabaseProvider<IContosoDatabase> GetDatabaseProvider(DatabaseOptions options = null)
		{
			return TestHelper.GetMemoryProvider(options);
		}

		private ClientAccount GetPerson()
		{
			return new ClientAccount
			{
				Name = "John"
			};
		}

		#endregion
	}
}