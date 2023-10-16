#region References

using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Data.Client;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests;

[TestClass]
public class SyncClientTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void CacheShouldBeUpdatedWhenUpdatingDatabase()
	{
		ForEachDatabaseType(x =>
		{
			// Removing from cache only works if we are actually removing data from the database
			var client = TestHelper.GetSyncClient("Client", x, false, true, false, null, null);
			client.DatabaseProvider.Options.PermanentSyncEntityDeletions = true;

			// Creating an address like normal, cache should not contain the ID yet
			var address = DataHelper.NewAddress("Blah");
			var addressId = client.DatabaseProvider.KeyCache.GetEntityId(address);
			Assert.IsNull(addressId);

			// Cache should be updated automatically
			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);
			addressId = client.DatabaseProvider.KeyCache.GetEntityId(address);
			Assert.IsNotNull(addressId);
			Assert.AreEqual(address.Id, (long) addressId);

			// Cache should be updated automatically
			client.GetDatabase<IContosoDatabase>().RemoveSaveAndCleanup<AddressEntity, long>(address);
			addressId = client.DatabaseProvider.KeyCache.GetEntityId(address);
			Assert.IsNull(addressId);
		});
	}

	/// <summary>
	/// Ensure that objects are reduced via the optional filter clause.
	/// </summary>
	[TestMethod]
	public void EntitiesShouldBeFiltered()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);

		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Work", state: "GA"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(2, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();
		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(2, changes.Collection.Count);

		options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.State == "SC"));

		Thread.Sleep(1);
		request.Reset();
		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var actual = changes.Collection[0].ToSyncEntity() as AddressEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("Home", actual.Line1);
		Assert.AreEqual("SC", actual.State);
	}

	/// <summary>
	/// Ensure that objects are returns with an "AND" filtered via an extended existing filter.
	/// </summary>
	[TestMethod]
	public void EntitiesShouldBeFilteredMoreByAndAlso()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);

		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC", postal: "12345"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home2", state: "SC", postal: "54321"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Work", state: "GA"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(3, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();

		// Simple filter to filter out a small subset
		var filter = new SyncRepositoryFilter<AddressEntity>(x => x.State == "SC");
		options.AddSyncableFilter(filter);

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(2, changes.Collection.Count);

		// Now add another clause via an "AND" statement to reduce the change list
		options.ResetFilters();
		options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(filter.OutgoingFilter.AndAlso(y => y.Postal == "12345")));

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);
		var actual = changes.Collection[0].ToSyncEntity() as AddressEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("Home", actual.Line1);
		Assert.AreEqual("12345", actual.Postal);
		Assert.AreEqual("SC", actual.State);
	}

	/// <summary>
	/// Ensure that objects are returns with an "OR" filtered via an extended existing filter.
	/// </summary>
	[TestMethod]
	public void EntitiesShouldBeFilteredMoreByOr()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);

		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home2", state: "NC"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Work", state: "GA"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(3, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();

		// Simple filter to filter out a small subset
		var filter = new SyncRepositoryFilter<AddressEntity>(x => x.State == "SC");
		options.AddSyncableFilter(filter);

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		// Now add another clause via an "OR" statement to extend the change list
		options.ResetFilters();
		options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(filter.OutgoingFilter.Or(x => x.State == "GA")));

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(2, changes.Collection.Count);
		var actual = changes.Collection[0].ToSyncEntity() as AddressEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("Home", actual.Line1);
		Assert.AreEqual("SC", actual.State);

		actual = changes.Collection[1].ToSyncEntity() as AddressEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("Work", actual.Line1);
		Assert.AreEqual("GA", actual.State);
	}

	/// <summary>
	/// Ensure that optional child relationships are not returned due to the filter.
	/// </summary>
	[TestMethod]
	public void EntityOptionalRelationshipsShouldBeFiltered()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);

		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			var address = EntityFactory.GetAddress(line1: "Home", state: "SC");
			database.Addresses.Add(address);
			address.Accounts.Add(EntityFactory.GetAccount(x => x.Name = "John Doe"));
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Work", state: "GA"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(2, database.Addresses.Count());
			Assert.AreEqual(1, database.Accounts.Count());
		}

		Thread.Sleep(1);
		request.Reset();

		// Would be 3 without filter, but with filter there is only one change
		options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.State == "SC"));

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var actual = changes.Collection[0].ToSyncEntity() as AddressEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("Home", actual.Line1);
		Assert.AreEqual("SC", actual.State);
	}

	/// <summary>
	/// Ensures that required parent relationships will be filters out of GetChanges. Sure this would cause the
	/// sync to fail but that is not point. The point is to ensure the other entities are not returned.
	/// </summary>
	[TestMethod]
	public void EntityRequiredRelationshipsShouldBeFiltered()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);

		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			var address = EntityFactory.GetAddress(x => x.Line1 = "Home");
			var account = EntityFactory.GetAccount(name: "John Doe", address: address);
			database.Accounts.Add(account);
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(1, database.Addresses.Count());
			Assert.AreEqual(1, database.Accounts.Count());
		}

		Thread.Sleep(1);
		request.Reset();

		// Would be 2 without filter, but with filter there is only one change
		options.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>());

		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var actual = changes.Collection[0].ToSyncEntity() as AccountEntity;
		Assert.IsNotNull(actual);
		Assert.AreEqual("John Doe", actual.Name);
	}

	[TestMethod]
	public void IncomingConverterShouldNotProcessNonDefinedObjects()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false))
		{
			OutgoingConverter = null,
			IncomingConverter = new SyncClientIncomingConverter(new SyncObjectIncomingConverter<ClientAddress, long, AddressEntity, long>())
		};

		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(1, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();
		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var address = EntityFactory.GetAddress(line1: "Work", state: "GA");
		var updates = new ServiceRequest<SyncObject>(address.ToSyncObject());
		var result = client.ApplyChanges(sessionId, updates);

		Assert.AreEqual(0, result.Skipped);
		Assert.AreEqual(0, result.TotalCount);
		Assert.AreEqual(0, result.Collection.Count);

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(1, database.Addresses.Count());
		}
	}

	[TestMethod]
	public void IncomingConverterShouldProcessNonDefinedObjects()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false))
		{
			OutgoingConverter = null,
			IncomingConverter = new SyncClientIncomingConverter(new SyncObjectIncomingConverter<ClientAddress, long, AddressEntity, long>())
		};

		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(1, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();
		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var address = ClientFactory.GetClientAddress();
		var updates = new ServiceRequest<SyncObject>(address.ToSyncObject());
		var result = client.ApplyChanges(sessionId, updates);

		Assert.AreEqual(0, result.Skipped);
		Assert.AreEqual(0, result.TotalCount);
		Assert.AreEqual(0, result.Collection.Count);

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(2, database.Addresses.Count());
		}
	}

	[TestMethod]
	public void NullConvertersShouldNotBreakClient()
	{
		var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false))
		{
			OutgoingConverter = null,
			IncomingConverter = null
		};

		var request = new SyncRequest();
		var options = new SyncOptions();
		var sessionId = Guid.NewGuid();
		client.BeginSync(sessionId, options);

		var changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(0, changes.Collection.Count);

		using (var database = client.GetDatabase<IContosoDatabase>())
		{
			database.Addresses.Add(EntityFactory.GetAddress(line1: "Home", state: "SC"));
			database.SaveChanges();
		}

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(1, database.Addresses.Count());
		}

		Thread.Sleep(1);
		request.Reset();
		changes = client.GetChanges(sessionId, request);
		Assert.AreEqual(1, changes.Collection.Count);

		var address = EntityFactory.GetAddress(line1: "Work", state: "GA");
		var updates = new ServiceRequest<SyncObject>(address.ToSyncObject());
		var result = client.ApplyChanges(sessionId, updates);

		Assert.AreEqual(0, result.Skipped);
		Assert.AreEqual(0, result.TotalCount);
		Assert.AreEqual(0, result.Collection.Count);

		using (var database = (IContosoDatabase) client.GetDatabase())
		{
			Assert.AreEqual(2, database.Addresses.Count());
		}
	}

	[TestInitialize]
	public override void TestInitialize()
	{
		base.TestInitialize();
		TimeService.Reset();
	}

	private void ForEachDatabaseType(Action<DatabaseType> action)
	{
		Enum.GetValues(typeof(DatabaseType))
			.Cast<DatabaseType>()
			.Where(x => x != DatabaseType.Unknown)
			.ForEach(x =>
			{
				x.Dump();
				action(x);
			});
	}

	#endregion
}