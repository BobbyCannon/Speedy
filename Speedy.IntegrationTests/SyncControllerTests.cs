#region References

using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Data;
using Speedy.Data;
using Speedy.Data.Client;
using Speedy.Logging;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples.Entities;
using Speedy.Website.WebApi;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class SyncControllerTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void ShouldSyncAllSyncItems()
		{
			var dispatcher = TestHelper.GetDispatcher();
			var clientProvider = TestHelper.GetClientProvider();
			var entityProvider = TestHelper.GetMemoryProvider();

			using (var database = clientProvider.GetDatabase())
			{
				PopulateAllClientData(database);
			}

			var credential = new NetworkCredential(TestHelper.AdministratorEmailAddress, TestHelper.AdministratorPassword);
			var server = new SyncController(entityProvider, TestHelper.GetAuthenticationService()) { User = TestHelper.GetIdentity(TestHelper.AdministratorId, "Administrator") };
			var syncClientProvider = new SyncClientProvider((n, c) => server);
			var syncManager = new ClientSyncManager(() => credential, clientProvider, syncClientProvider, dispatcher);
			using var logger = new LogListener(Guid.Empty, EventLevel.Verbose) { OutputToConsole = true };
			syncManager.Initialize();
			syncManager.Sync();

			Assert.AreEqual(true, syncManager.IsSyncSuccessful, string.Join(Environment.NewLine, syncManager.SyncIssues.Select(x => x.Message)));
			Assert.AreNotEqual(0, logger.Events.Count);

			using (var clientDatabase = clientProvider.GetDatabase())
			using (var entityDatabase = entityProvider.GetDatabase())
			{
				var clientAccounts = clientDatabase.Accounts.ToList();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var serverAccounts = entityDatabase.Accounts.ToList();
				var serverAddresses = entityDatabase.Addresses.ToList();

				Assert.AreEqual(2, clientAccounts.Count);
				Assert.AreEqual(2, clientAddresses.Count);
				Assert.AreEqual(2, serverAccounts.Count);
				Assert.AreEqual(2, serverAddresses.Count);

				Compare(clientAccounts[0], serverAccounts[1]);
				Compare(clientAccounts[1], serverAccounts[0]);
				Compare(clientAddresses[0], serverAddresses[1]);
				Compare(clientAddresses[1], serverAddresses[0]);
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

		private void PopulateAllClientData(ContosoClientDatabase database)
		{
			var address = ClientFactory.GetClientAddress();
			var account = ClientFactory.GetClientAccount("John", address);
			database.Accounts.Add(account);
			database.SaveChanges();

			Assert.AreNotEqual(0, database.Accounts.Count());
			Assert.AreNotEqual(0, database.Addresses.Count());
		}

		#endregion
	}
}