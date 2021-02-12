#region References

using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.Data;
using Speedy.Data.Client;
using Speedy.Data.WebApi;
using Speedy.Logging;
using Speedy.Net;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Sync;
using Speedy.Website.WebApi;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SyncControllerTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void LogEventsShouldNotBeUpdatable()
		{
			var entityProvider = TestHelper.GetSyncableMemoryProvider();

			AccountEntity account;
			LogEventEntity logEntity;

			using (var database = entityProvider.GetDatabase())
			{
				logEntity = EntityFactory.GetLogEvent("Hello World", LogLevel.Debug);
				database.LogEvents.AddOrUpdate(logEntity);
				database.SaveChanges();
				account = database.Accounts.FirstOrDefault(x => x.Id == TestHelper.AdministratorId);
			}

			var controller = new SyncController(entityProvider) { ControllerContext = TestHelper.GetControllerContext(account) };
			var syncId = Guid.NewGuid();
			var syncOption = new SyncOptions();
			var session = controller.BeginSync(syncId, syncOption);

			// Change the log entry to see if we can sync it.
			var update = new LogEvent();
			update.UpdateWith(logEntity);
			update.Message = "Foo Bar";
			update.Level = LogLevel.Critical;
			update.ModifiedOn = TimeService.UtcNow;

			var issues = controller.ApplyChanges(session.Id, new ServiceRequest<SyncObject>(update.ToSyncObject()));
			controller.EndSync(session.Id);

			Assert.AreEqual(1, issues.TotalCount);
			Assert.AreEqual(logEntity.SyncId, issues.Collection[0].Id);
			Assert.AreEqual(SyncIssueType.UpdateException, issues.Collection[0].IssueType);
			Assert.AreEqual("You cannot modify or delete log entries.", issues.Collection[0].Message);
			Assert.AreEqual("Speedy.Website.Data.Entities.LogEventEntity,Speedy.Website.Data", issues.Collection[0].TypeName);

			using (var database = entityProvider.GetDatabase())
			{
				var actual = database.LogEvents.FirstOrDefault(x => x.Id == logEntity.Id);
				Assert.IsNotNull(actual);
				Assert.AreEqual("Hello World", actual.Message);
				Assert.AreEqual(LogLevel.Debug, actual.Level);
			}
		}

		[TestMethod]
		public void ShouldSyncAllSyncItems()
		{
			var dispatcher = TestHelper.GetDispatcher();
			var clientProvider = TestHelper.GetClientProvider();
			var entityProvider = TestHelper.GetSyncableMemoryProvider();
			var keyCache = new DatabaseKeyCache();

			using (var database = clientProvider.GetDatabase())
			{
				PopulateAllClientData(database, keyCache);
			}

			var credential = new NetworkCredential(TestHelper.AdministratorEmailAddress, TestHelper.AdministratorPassword);
			var server = new ServerSyncClient(new AccountEntity(), new SyncableDatabaseProvider<IContosoDatabase>(entityProvider.GetSyncableDatabase, ContosoDatabase.GetDefaultOptions(), keyCache));
			var syncClientProvider = new SyncClientProvider((n, c) => server);
			var syncManager = new ClientSyncManager(() => credential, clientProvider, syncClientProvider, dispatcher);
			using var logger = LogListener.CreateSession(Guid.Empty, EventLevel.Verbose, x => x.OutputToConsole = true);
			syncManager.Sync();

			Assert.AreEqual(true, syncManager.IsSyncSuccessful, string.Join(Environment.NewLine, syncManager.SyncIssues.Select(x => x.Message)));
			Assert.AreNotEqual(0, logger.Events.Count);

			using (var clientDatabase = clientProvider.GetDatabase())
			using (var entityDatabase = entityProvider.GetDatabase())
			{
				var clientAccounts = clientDatabase.Accounts.ToList();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientLogEvents = clientDatabase.LogEvents.ToList();
				var serverAccounts = entityDatabase.Accounts.ToList();
				var serverAddresses = entityDatabase.Addresses.ToList();
				var serverLogEvents = entityDatabase.LogEvents.ToList();

				Assert.AreEqual(2, clientAccounts.Count);
				Assert.AreEqual(2, clientAddresses.Count);
				Assert.AreEqual(6, clientLogEvents.Count);
				Assert.AreEqual(2, serverAccounts.Count);
				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(6, serverLogEvents.Count);

				Compare(clientAccounts[0], serverAccounts[1]);
				Compare(clientAccounts[1], serverAccounts[0]);
				Compare(clientAddresses[0], serverAddresses[1]);
				Compare(clientAddresses[1], serverAddresses[0]);

				for (var i = 0; i < clientLogEvents.Count; i++)
				{
					Compare(clientLogEvents[i], serverLogEvents[i]);
				}
			}
		}

		[TestMethod]
		public void ShouldSyncOnlyAddresses()
		{
			var dispatcher = TestHelper.GetDispatcher();
			var clientProvider = TestHelper.GetClientProvider();
			var entityProvider = TestHelper.GetSyncableMemoryProvider();
			var keyCache = new DatabaseKeyCache();

			using (var database = clientProvider.GetDatabase())
			{
				PopulateAllClientData(database, keyCache);
			}

			var credential = new NetworkCredential(TestHelper.AdministratorEmailAddress, TestHelper.AdministratorPassword);
			var server = new ServerSyncClient(new AccountEntity(), new SyncableDatabaseProvider<IContosoDatabase>(entityProvider.GetSyncableDatabase, ContosoDatabase.GetDefaultOptions(), keyCache));
			var syncClientProvider = new SyncClientProvider((n, c) => server);
			var syncManager = new ClientSyncManager(() => credential, clientProvider, syncClientProvider, dispatcher);
			using var logger = LogListener.CreateSession(Guid.Empty, EventLevel.Verbose, x => x.OutputToConsole = true);
			syncManager.SyncAddresses();

			Assert.AreEqual(true, syncManager.IsSyncSuccessful, string.Join(Environment.NewLine, syncManager.SyncIssues.Select(x => x.Message)));
			Assert.AreNotEqual(0, logger.Events.Count);

			using (var clientDatabase = clientProvider.GetDatabase())
			using (var entityDatabase = entityProvider.GetDatabase())
			{
				var clientAccounts = clientDatabase.Accounts.ToList();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientLogEvents = clientDatabase.LogEvents.ToList();
				var serverAccounts = entityDatabase.Accounts.ToList();
				var serverAddresses = entityDatabase.Addresses.ToList();
				var serverLogEvents = entityDatabase.LogEvents.ToList();

				Assert.AreEqual(1, clientAccounts.Count);
				Assert.AreEqual(2, clientAddresses.Count);
				Assert.AreEqual(6, clientLogEvents.Count);
				Assert.AreEqual(1, serverAccounts.Count);
				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(0, serverLogEvents.Count);

				Compare(clientAddresses[0], serverAddresses[1]);
				Compare(clientAddresses[1], serverAddresses[0]);
			}
		}

		[TestMethod]
		public void ShouldSyncOnlyLogEvents()
		{
			var dispatcher = TestHelper.GetDispatcher();
			var clientProvider = TestHelper.GetClientProvider();
			var entityProvider = TestHelper.GetSyncableMemoryProvider();
			var keyCache = new DatabaseKeyCache();

			using (var database = clientProvider.GetDatabase())
			{
				PopulateAllClientData(database, keyCache);
			}

			var credential = new NetworkCredential(TestHelper.AdministratorEmailAddress, TestHelper.AdministratorPassword);
			var server = new ServerSyncClient(new AccountEntity(), new SyncableDatabaseProvider<IContosoDatabase>(entityProvider.GetSyncableDatabase, ContosoDatabase.GetDefaultOptions(), keyCache));
			var syncClientProvider = new SyncClientProvider((n, c) => server);
			var syncManager = new ClientSyncManager(() => credential, clientProvider, syncClientProvider, dispatcher);
			using var logger = LogListener.CreateSession(Guid.Empty, EventLevel.Verbose, x => x.OutputToConsole = true);
			syncManager.SyncLogEvents();

			Assert.AreEqual(true, syncManager.IsSyncSuccessful, string.Join(Environment.NewLine, syncManager.SyncIssues.Select(x => x.Message)));
			Assert.AreNotEqual(0, logger.Events.Count);

			using (var clientDatabase = clientProvider.GetDatabase())
			using (var entityDatabase = entityProvider.GetDatabase())
			{
				var clientAccounts = clientDatabase.Accounts.ToList();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientLogEvents = clientDatabase.LogEvents.ToList();
				var serverAccounts = entityDatabase.Accounts.ToList();
				var serverAddresses = entityDatabase.Addresses.ToList();
				var serverLogEvents = entityDatabase.LogEvents.ToList();

				Assert.AreEqual(1, clientAccounts.Count);
				Assert.AreEqual(1, clientAddresses.Count);
				Assert.AreEqual(6, clientLogEvents.Count);
				Assert.AreEqual(1, serverAccounts.Count);
				Assert.AreEqual(1, serverAddresses.Count);
				Assert.AreEqual(6, serverLogEvents.Count);

				for (var i = 0; i < clientLogEvents.Count; i++)
				{
					Compare(clientLogEvents[i], serverLogEvents[i]);
				}
			}
		}

		private void Compare(ClientAccount client, AccountEntity entity)
		{
			Assert.AreEqual(client.AddressSyncId, entity.AddressSyncId);
			Assert.AreEqual(client.EmailAddress, entity.EmailAddress);
			Assert.AreEqual(client.Name, entity.Name);
		}

		private void Compare(ClientAddress client, AddressEntity entity)
		{
			Assert.AreEqual(client.City, entity.City);
			Assert.AreEqual(client.Line1, entity.Line1);
			Assert.AreEqual(client.Line2, entity.Line2);
			Assert.AreEqual(client.Postal, entity.Postal);
			Assert.AreEqual(client.State, entity.State);
		}

		private void Compare(ClientLogEvent client, LogEventEntity entity)
		{
			Assert.AreEqual(client.Level, entity.Level);
			Assert.AreEqual(client.Message, entity.Message);
		}

		private void PopulateAllClientData(ContosoClientMemoryDatabase memoryDatabase, DatabaseKeyCache keyCache)
		{
			var address = ClientFactory.GetClientAddress();
			var account = ClientFactory.GetClientAccount("John", address);
			memoryDatabase.Accounts.Add(account);

			var logEvent1 = ClientFactory.GetLogEvent("Critical", LogLevel.Critical);
			var logEvent2 = ClientFactory.GetLogEvent("Error", LogLevel.Error);
			var logEvent3 = ClientFactory.GetLogEvent("Warning", LogLevel.Warning);
			var logEvent4 = ClientFactory.GetLogEvent("Information", LogLevel.Information);
			var logEvent5 = ClientFactory.GetLogEvent("Debug", LogLevel.Debug);
			var logEvent6 = ClientFactory.GetLogEvent("Verbose", LogLevel.Verbose);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent1);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent2);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent3);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent4);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent5);
			memoryDatabase.LogEvents.AddOrUpdate(logEvent6);

			memoryDatabase.SaveChanges();
			keyCache?.Initialize(memoryDatabase);

			Assert.AreNotEqual(0, memoryDatabase.Accounts.Count());
			Assert.AreNotEqual(0, memoryDatabase.Addresses.Count());
		}

		#endregion
	}
}