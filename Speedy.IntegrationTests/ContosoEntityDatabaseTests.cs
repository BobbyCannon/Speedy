#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class ContosoEntityDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void DetectSyncableRepository()
		{
			var provider = GetDatabase();
			using var database = provider.GetDatabase();
			var repositories = database.GetSyncableRepositories(new SyncOptions()).ToList();
			Assert.AreEqual(4, repositories.Count);
			Assert.AreEqual("Speedy.Website.Samples.Entities.AddressEntity,Speedy.Website.Samples", repositories[0].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.SettingEntity,Speedy.Website.Samples", repositories[1].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.AccountEntity,Speedy.Website.Samples", repositories[2].TypeName);
			Assert.AreEqual("Speedy.Website.Samples.Entities.LogEventEntity,Speedy.Website.Samples", repositories[3].TypeName);
		}

		private AddressEntity GetAddress()
		{
			return new AddressEntity
			{
				City = "City"
			};
		}

		private static IDatabaseProvider<ContosoDatabase> GetDatabase(DatabaseOptions options = null)
		{
			return TestHelper.GetSqlProvider(options);
		}

		private AccountEntity GetPerson()
		{
			return new AccountEntity
			{
				Name = "John"
			};
		}

		#endregion
	}
}