#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Client;
using Speedy.Net;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SyncClientTests
	{
		#region Methods

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

			request.Reset();
			changes = client.GetChanges(sessionId, request);
			Assert.AreEqual(2, changes.Collection.Count);

			options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.State == "SC"));

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
		public void GetCorrectionShouldBeProcessWithConverter()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false))
			{
				OutgoingConverter = null,
				IncomingConverter = new SyncClientIncomingConverter(new SyncObjectIncomingConverter<ClientAddress, long, AddressEntity, long>())
			};

			AddressEntity address;

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				address = EntityFactory.GetAddress(line1: "Home", state: "SC");
				database.Addresses.Add(address);
				database.SaveChanges();
			}

			var issue = new SyncIssue { Id = address.SyncId, IssueType = SyncIssueType.RelationshipConstraint, Message = "Errors", TypeName = typeof(ClientAddress).ToAssemblyName() };
			var request = new ServiceRequest<SyncIssue>(issue);
			var options = new SyncOptions();
			var sessionId = Guid.NewGuid();
			client.BeginSync(sessionId, options);

			var corrections = client.GetCorrections(sessionId, request);
			Assert.AreEqual(1, corrections.Collection.Count);
			Assert.AreEqual(typeof(AddressEntity).ToAssemblyName(), corrections.Collection[0].TypeName);
			Assert.AreEqual(SyncObjectStatus.Added, corrections.Collection[0].Status);
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

		#endregion
	}
}