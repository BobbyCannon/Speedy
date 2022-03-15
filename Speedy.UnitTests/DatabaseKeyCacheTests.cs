#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class DatabaseKeyCacheTests
	{
		#region Methods

		[TestMethod]
		public void Cleanup()
		{
			var currentTime = new DateTime(2021, 02, 04, 08, 00, 01, DateTimeKind.Utc);

			TimeService.UtcNowProvider = () => currentTime;

			var cache = new DatabaseKeyCache(new TimeSpan(0, 0, 2));
			cache.AddEntityId(typeof(AddressEntity), Guid.Parse("4BC15FD1-B6F0-4BDF-A24C-BBC8A88ADDCB"), 1);

			// Should still be cached even after cleanup
			cache.Cleanup();
			Assert.AreEqual(1, cache.TotalCachedItems);
			Assert.AreEqual("\tSpeedy.Website.Data.Entities.AddressEntity\r\n\t\t4bc15fd1-b6f0-4bdf-a24c-bbc8a88addcb-1\r\n", cache.ToDetailedString());

			currentTime = currentTime.AddSeconds(1);
			cache.AddEntityId(typeof(AddressEntity), Guid.Parse("3D5859EE-5C0D-4CD5-8EDB-5D4B9127CADF"), 2);
			Assert.AreEqual(2, cache.TotalCachedItems);
			Assert.AreEqual("\tSpeedy.Website.Data.Entities.AddressEntity\r\n\t\t4bc15fd1-b6f0-4bdf-a24c-bbc8a88addcb-1\r\n\t\t3d5859ee-5c0d-4cd5-8edb-5d4b9127cadf-2\r\n", cache.ToDetailedString());

			// Cache should not have changed yet
			cache.Cleanup();
			Assert.AreEqual(2, cache.TotalCachedItems);
			Assert.AreEqual("\tSpeedy.Website.Data.Entities.AddressEntity\r\n\t\t4bc15fd1-b6f0-4bdf-a24c-bbc8a88addcb-1\r\n\t\t3d5859ee-5c0d-4cd5-8edb-5d4b9127cadf-2\r\n", cache.ToDetailedString());

			// Cache should be clear only 1 item after cleanup because enough time passed
			currentTime = currentTime.AddSeconds(1);
			cache.Cleanup();
			Assert.AreEqual(1, cache.TotalCachedItems);
			Assert.AreEqual("\tSpeedy.Website.Data.Entities.AddressEntity\r\n\t\t3d5859ee-5c0d-4cd5-8edb-5d4b9127cadf-2\r\n", cache.ToDetailedString());

			currentTime = currentTime.AddSeconds(1);

			// Cache should be clear the only remaining item after cleanup because enough time passed
			cache.Cleanup();
			Assert.AreEqual(0, cache.TotalCachedItems);
			Assert.AreEqual("", cache.ToDetailedString());
		}

		[TestMethod]
		public void HasKeysBeenLoadedIntoCache()
		{
			var memoryDatabase = new ContosoMemoryDatabase();
			var cache = new DatabaseKeyCache();
			cache.Initialize(typeof(AccountEntity), typeof(AddressEntity));

			Assert.IsFalse(cache.HasKeysBeenLoadedIntoCache(typeof(AccountEntity)));
			Assert.IsFalse(cache.HasKeysBeenLoadedIntoCache(typeof(AddressEntity)));

			cache.LoadKeysIntoCache(memoryDatabase, typeof(AccountEntity));

			Assert.IsTrue(cache.HasKeysBeenLoadedIntoCache(typeof(AccountEntity)));
			Assert.IsFalse(cache.HasKeysBeenLoadedIntoCache(typeof(AddressEntity)));

			var accountKeys = cache[typeof(AccountEntity)];
			Assert.AreEqual(0, accountKeys.Count);
		}

		[TestMethod]
		public void InitializeWithDatabase()
		{
			var memoryDatabase = new ContosoMemoryDatabase();
			var address = EntityFactory.GetAddress();
			memoryDatabase.Addresses.Add(address);
			memoryDatabase.Accounts.Add(EntityFactory.GetAccount(address: address));
			memoryDatabase.Accounts.Add(EntityFactory.GetAccount(address: address));
			memoryDatabase.SaveChanges();
			var cache = new DatabaseKeyCache();
			var syncableTypes = memoryDatabase.GetSyncableRepositories(new SyncOptions()).Select(x => x.RealType).ToArray();
			cache.InitializeAndLoad(memoryDatabase, syncableTypes);
			Assert.AreEqual(4, cache.Count);
			Assert.AreEqual(3, cache.TotalCachedItems);

			memoryDatabase = new ContosoMemoryDatabase();
			memoryDatabase.Addresses.Add(address);
			memoryDatabase.Accounts.Add(EntityFactory.GetAccount(address: address));
			memoryDatabase.Accounts.Add(EntityFactory.GetAccount(address: address));
			memoryDatabase.SaveChanges();
			var memoryProvider = new SyncableDatabaseProvider((o, c) => memoryDatabase, ContosoDatabase.GetDefaultOptions(), null, null);
			cache = new DatabaseKeyCache();
			cache.InitializeAndLoad(memoryProvider.GetSyncableDatabase(), syncableTypes);
			Assert.AreEqual(4, cache.Count);
			Assert.AreEqual(3, cache.TotalCachedItems);
		}

		[TestMethod]
		public void InitializeWithEmptyDatabase()
		{
			var memoryDatabase = new ContosoMemoryDatabase();
			var cache = new DatabaseKeyCache();
			cache.InitializeAndLoad(memoryDatabase);
			Assert.AreEqual(0, cache.TotalCachedItems);

			memoryDatabase = new ContosoMemoryDatabase();
			var memoryProvider = new SyncableDatabaseProvider((o, c) => memoryDatabase, ContosoDatabase.GetDefaultOptions(), null, null);
			cache = new DatabaseKeyCache();
			cache.InitializeAndLoad(memoryProvider);
			Assert.AreEqual(0, cache.TotalCachedItems);
		}

		#endregion
	}
}