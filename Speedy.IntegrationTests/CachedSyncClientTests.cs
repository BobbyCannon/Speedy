#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class CachedSyncClientTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void CacheOnlyCertainEntities()
		{
			var server = TestHelper.GetSyncClient("Server", DatabaseType.Sql, false, true, false, null, null);
			var keyCache = server.DatabaseProvider.KeyCache;
			keyCache.Initialize();

			AddressEntity address;
			AccountEntity account;

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				address = DataHelper.NewAddress("foo bar", null, Guid.Parse("5CC753E1-8852-42BE-80D3-6E7A6E54E14F"));
				account = DataHelper.NewAccount("john", address, Guid.Parse("DA51BF03-29C6-41CC-9CF8-6E9D6CF35AFC"));
				database.Accounts.Add(account);
				database.SaveChanges();
			}

			// Should have two cached entity IDs, one for the address and one for the account
			Assert.AreEqual(2, keyCache.TotalCachedItems);
			Assert.AreEqual(1L, keyCache.GetEntityId(address));
			Assert.AreEqual(1, keyCache.GetEntityId(account));

			server = TestHelper.GetSyncClient("Server", DatabaseType.Sql, false, true, false, null, null);
			keyCache = server.DatabaseProvider.KeyCache;
			keyCache.Initialize(typeof(AddressEntity));

			// Brand new key cache manager
			Assert.IsTrue(keyCache.SupportsType(typeof(AddressEntity)));
			Assert.IsFalse(keyCache.SupportsType(typeof(AccountEntity)));
			Assert.AreEqual(1, keyCache.SyncEntitiesToCache.Length);
			Assert.AreEqual(0, keyCache.TotalCachedItems);
			Assert.AreEqual(null, keyCache.GetEntityId(address));
			Assert.AreEqual(null, keyCache.GetEntityId(account));

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				address = DataHelper.NewAddress("foo bar", null, Guid.Parse("5CC753E1-8852-42BE-80D3-6E7A6E54E14F"));
				account = DataHelper.NewAccount("john", address, Guid.Parse("DA51BF03-29C6-41CC-9CF8-6E9D6CF35AFC"));
				database.Accounts.Add(account);
				database.SaveChanges();
			}

			// Should have only cached entity IDs, one for the address but none for the account
			Assert.AreEqual(1, keyCache.TotalCachedItems);
			Assert.AreEqual(1L, keyCache.GetEntityId(address));
			Assert.AreEqual(null, keyCache.GetEntityId(account));
		}

		[TestMethod]
		public void InitializeShouldResetCache()
		{
			var server = TestHelper.GetSyncClient("Server", DatabaseType.Sql, false, true, false, null, null);
			var keyCache = server.DatabaseProvider.KeyCache;

			// Initialize with no parameters
			keyCache.AddEntityId(typeof(AddressEntity), Guid.Parse("4B6E7980-3715-4C2C-9DC6-F9A5F2A40351"), 1);
			Assert.AreEqual(1, keyCache.Count);
			Assert.AreEqual(1, keyCache.TotalCachedItems);

			keyCache.Initialize();
			Assert.AreEqual(0, keyCache.Count);
			Assert.AreEqual(0, keyCache.TotalCachedItems);

			// Initialize with database provider
			keyCache.AddEntityId(typeof(AddressEntity), Guid.Parse("4B6E7980-3715-4C2C-9DC6-F9A5F2A40351"), 1);
			Assert.AreEqual(1, keyCache.Count);
			Assert.AreEqual(1, keyCache.TotalCachedItems);

			keyCache.InitializeAndLoad(server.DatabaseProvider, typeof(AddressEntity));
			Assert.AreEqual(1, keyCache.Count);
			Assert.AreEqual(0, keyCache.TotalCachedItems);

			// Initialize with database
			keyCache.AddEntityId(typeof(AddressEntity), Guid.Parse("4B6E7980-3715-4C2C-9DC6-F9A5F2A40351"), 1);
			Assert.AreEqual(1, keyCache.Count);
			Assert.AreEqual(1, keyCache.TotalCachedItems);

			using var database = server.DatabaseProvider.GetSyncableDatabase();
			keyCache.InitializeAndLoad(database, typeof(AddressEntity));
			Assert.AreEqual(1, keyCache.Count);
			Assert.AreEqual(0, keyCache.TotalCachedItems);
		}

		[TestMethod]
		public void InvalidCacheShouldNotPreventSync()
		{
			var server = TestHelper.GetSyncClient("Server", DatabaseType.Sql, false, true, false, null, null);
			var client = TestHelper.GetSyncClient("Client", DatabaseType.Sql, false, true, true, null, null);
			client.Options.EnablePrimaryKeyCache = true;

			// Creating an address like normal, cache should not contain the ID yet
			var address = DataHelper.NewAddress("Blah");
			var addressId = client.DatabaseProvider.KeyCache.GetEntityId(address);
			Assert.IsNull(addressId);

			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			// Cache should have been update automatically
			addressId = client.DatabaseProvider.KeyCache.GetEntityId(address);
			Assert.IsNotNull(addressId);
			Assert.AreEqual(address.Id, (long) addressId);

			// Now we will sync the address to the server
			using var engine = SyncEngine.Run(client, server, new SyncOptions());
			Assert.AreEqual(0, engine.SyncIssues.Count);

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				var addresses = database.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				TestHelper.AreEqual(address, addresses[0].Unwrap());

				// Change the server address while we have it available
				addresses[0].Line1 = "foo bar";
				database.SaveChanges();
			}

			// Now clear the cache on the client, which is really bad. This will force an inefficient sync
			// However we want to ensure the sync still succeed even if it's slower
			client.DatabaseProvider.KeyCache.Clear();
			Assert.AreEqual(0, client.DatabaseProvider.KeyCache.TotalCachedItems);

			// Now we will sync the address change from the server to the client
			using var engine2 = SyncEngine.Run(client, server, new SyncOptions());
			Assert.AreEqual(0, engine2.SyncIssues.Count);
			Assert.AreEqual(1, client.Statistics.IndividualProcessCount);
			Assert.AreEqual(1, client.DatabaseProvider.KeyCache.TotalCachedItems);
			Assert.AreEqual(1L, client.DatabaseProvider.KeyCache.GetEntityId(address));

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				var addresses = database.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				Assert.AreEqual("foo bar", addresses[0].Line1);
			}

			// Let's change the server again and see if the client uses it new cache entry
			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				var addresses = database.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				addresses[0].Line1 = "bar foo";
				database.SaveChanges();
			}

			// Now we will sync the second address change from the server to the client
			// The client should use the cache properly meaning no individual processing
			using var engine3 = SyncEngine.Run(client, server, new SyncOptions());
			Assert.AreEqual(0, engine3.SyncIssues.Count);
			Assert.AreEqual(0, client.Statistics.IndividualProcessCount);

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				var addresses = database.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				Assert.AreEqual("bar foo", addresses[0].Line1);
			}
		}

		#endregion
	}
}