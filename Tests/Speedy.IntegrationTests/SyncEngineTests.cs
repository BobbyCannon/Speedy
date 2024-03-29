﻿#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Automation.Tests;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Net;
using Speedy.Sync;
using Speedy.UnitTests;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	/// <summary>
	/// Summary description for SyncEngineTests.
	/// </summary>
	[TestClass]
	public class SyncEngineTests : SpeedyUnitTest
	{
		#region Methods

		[TestInitialize]
		public override void TestInitialize()
		{
			base.TestInitialize();
			ResetCurrentTime();
		}
		
		[TestMethod]
		public void AddItemToClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Blah"));

				var options = GetSyncOptions();
				using var engine = SyncEngine.Run(client, server, options);
				var issues = engine.SyncIssues;
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var exclusions = GetEntityExclusions(addresses1[0]);
					Assert.AreEqual(2, addresses1.Count);
					Assert.AreEqual(2, addresses2.Count);
					AreEqual(addresses1[0].Unwrap(), addresses2[0].Unwrap(), exclusions);
					AreEqual(addresses1[1].Unwrap(), addresses2[1].Unwrap(), exclusions);
				}

				issues = CompleteTestSync(client, server, options);
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));
			});
		}

		[TestMethod]
		public void AddItemToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Foo"));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Bar"));

				var options = GetSyncOptions();
				using var engine = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var exclusions = GetEntityExclusions(addresses1[0]);

					Assert.AreEqual(3, addresses1.Count);
					Assert.AreEqual(3, addresses2.Count);

					AreEqual(addresses1[0].Unwrap(), addresses2[0].Unwrap(), exclusions);
					AreEqual(addresses1[1].Unwrap(), addresses2[2].Unwrap(), exclusions);
					AreEqual(addresses1[2].Unwrap(), addresses2[1].Unwrap(), exclusions);
				}

				CompleteTestSync(client, server, options);
			});
		}

		/// <summary>
		/// This test will force both sides to have a local address with ID of 1 then sync.
		/// This means the engine should sync over the client address as 2 and update the
		/// relationship to point to ID 2 instead of the old local relationship of 1.
		/// </summary>
		[TestMethod]
		public void AddItemToClientAndServerForceRelationshipUpdate()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var accountId = Guid.NewGuid();
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo Bar", accountId, GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Bar"));

				var options = GetSyncOptions();
				using var engine = SyncEngine.Run(client, server, options);
				var issues = engine.SyncIssues;

				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var clientAddresses = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var serverAddresses = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var exclusions = GetEntityExclusions(clientAddresses[0]);
					var actualAccount = serverDatabase.Accounts.Include(x => x.Address).First(x => x.SyncId == accountId);

					Assert.AreEqual(3, actualAccount.Address.Id);
					Assert.AreEqual("Foo", actualAccount.Address.Line1);
					AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					AreEqual(serverDatabase.Accounts.Count(), clientDatabase.Accounts.Count());

					AreEqual(clientAddresses[0].Unwrap(), serverAddresses[0].Unwrap(), exclusions);
					AreEqual(clientAddresses[1].Unwrap(), serverAddresses[2].Unwrap(), exclusions);
					AreEqual(clientAddresses[2].Unwrap(), serverAddresses[1].Unwrap(), exclusions);
				}

				CompleteTestSync(client, server, options);
			});
		}

		[TestMethod]
		public void AddItemToClientAndServerWithSyncEntityWithCustomLookupFilter()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(GetSetting("Foo", "Bar", Guid.Parse("3E89C239-9B29-4E47-B4CA-C0695450FC07")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(GetSetting("Foo", "Bar", Guid.Parse("5323C23E-6959-47EE-9EDD-6CC1185859DA")));

				var options = GetSyncOptions();
				options.AddSyncableFilter(new SyncRepositoryFilter<SettingEntity>(null, null,
					// This filter below replaces lookup for "SyncId"
					x => y => y.Name == x.Name)
				);
				using var engine = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var settings1 = clientDatabase.Settings.OrderBy(x => x.Id).ToList();
					var settings2 = serverDatabase.Settings.OrderBy(x => x.Id).ToList();

					Assert.AreEqual(1, settings1.Count);
					Assert.AreEqual(1, settings2.Count);

					var actual1 = (SettingEntity) settings1[0].Unwrap();
					var actual2 = (SettingEntity) settings2[0].Unwrap();
					var exclusions = GetEntityExclusions(actual1)
						.ToList()
						.AddRange(nameof(ISyncEntity.SyncId))
						.ToArray();

					AreEqual(actual1, actual2, exclusions);
					Assert.AreNotEqual(actual1.SyncId, actual2.SyncId);
				}

				CompleteTestSync(client, server, options, false);
			}, false);
		}

		[TestMethod]
		public void AddItemToClientWithLogger()
		{
			ResetCurrentTime();

			TestHelper.TestServerAndClients((server, client) =>
			{
				using var listener = LogListener.CreateSession(Guid.Empty, EventLevel.Verbose, x => x.OutputToConsole = true);
				Logger.Instance.Write(client.Name + " -> " + server.Name);
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Blah"));

				Thread.Sleep(1);

				var options = GetSyncOptions();
				using var engine = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();

					Assert.AreEqual(2, addresses1.Count);
					Assert.AreEqual(2, addresses2.Count);
					var exclusions = GetEntityExclusions(addresses1[0]);

					AreEqual(addresses1[0].Unwrap(), addresses2[0].Unwrap(), exclusions);
					AreEqual(addresses1[1].Unwrap(), addresses2[1].Unwrap(), exclusions);
				}

				using var engine2 = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, engine2.SyncIssues.Count, string.Join(",", engine2.SyncIssues.Select(x => x.Message)));
				Assert.IsFalse(client.Statistics.IsReset);
				Assert.IsFalse(client.Statistics.IsReset);

				using var engine3 = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, engine3.SyncIssues.Count, string.Join(",", engine3.SyncIssues.Select(x => x.Message)));
				Assert.IsTrue(client.Statistics.IsReset);
				Assert.IsTrue(client.Statistics.IsReset);
			});
		}

		[TestMethod]
		public void AddItemToServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Blah"));

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				using var clientDatabase = client.GetDatabase<IContosoDatabase>();
				using var serverDatabase = server.GetDatabase<IContosoDatabase>();
				var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
				var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
				var exclusions = GetEntityExclusions(addresses1[0]);

				Assert.AreEqual(2, addresses1.Count);
				Assert.AreEqual(2, addresses2.Count);

				AreEqual(addresses1[0].Unwrap(), addresses2[0].Unwrap(), exclusions);
				AreEqual(addresses1[1].Unwrap(), addresses2[1].Unwrap(), exclusions);
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo", GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Bar", GetAddress("Bar")));

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using var clientDatabase = client.GetDatabase<IContosoDatabase>();
				using var serverDatabase = server.GetDatabase<IContosoDatabase>();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientPeople = clientDatabase.Accounts.ToList();
				var serverAddresses = serverDatabase.Addresses.ToList();
				var serverPeople = serverDatabase.Accounts.ToList();

				Assert.AreEqual(3, clientAddresses.Count);
				Assert.AreEqual(3, clientPeople.Count);
				Assert.AreEqual(3, serverAddresses.Count);
				Assert.AreEqual(3, serverPeople.Count);

				var exclusions = GetEntityExclusions(clientAddresses[0]);
				AreEqual(clientAddresses[1].Unwrap(), serverAddresses[2].Unwrap(), exclusions);
				AreEqual(clientAddresses[2].Unwrap(), serverAddresses[1].Unwrap(), exclusions);

				exclusions = GetEntityExclusions(clientPeople[0]);
				AreEqual(clientPeople[1].Unwrap(), serverPeople[2].Unwrap(), exclusions);
				AreEqual(clientPeople[2].Unwrap(), serverPeople[1].Unwrap(), exclusions);

				Assert.AreEqual("Foo", clientAddresses[1].Line1);
				Assert.AreEqual("Foo", clientPeople[1].Name);
				Assert.AreEqual("Foo", clientPeople[1].Address.Line1);
				Assert.AreEqual(2, clientPeople[1].AddressId);
				Assert.AreEqual("Foo", serverAddresses[2].Line1);
				Assert.AreEqual("Foo", serverPeople[2].Name);
				Assert.AreEqual("Foo", serverPeople[2].Address.Line1);
				Assert.AreEqual(3, serverPeople[2].AddressId);
				Assert.AreEqual("Bar", clientAddresses[2].Line1);
				Assert.AreEqual("Bar", clientPeople[2].Name);
				Assert.AreEqual("Bar", clientPeople[2].Address.Line1);
				Assert.AreEqual(3, clientPeople[2].AddressId);
				Assert.AreEqual("Bar", serverAddresses[1].Line1);
				Assert.AreEqual("Bar", serverPeople[1].Name);
				Assert.AreEqual("Bar", serverPeople[1].Address.Line1);
				Assert.AreEqual(2, serverPeople[1].AddressId);
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServerWithLogger()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo", GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Bar", GetAddress("Bar")));

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				using var listener = MemoryLogListener.CreateSession(engine.SessionId, EventLevel.Verbose);
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				var expected = client.Name.Contains("WEB") ? 18 : server.Name.Contains("WEB") ? 10 : 14;
				Assert.AreEqual(expected, listener.Events.Count, string.Join("\r\n", listener.Events.Select(x => x.GetMessage())));

				using var clientDatabase = client.GetDatabase<IContosoDatabase>();
				using var serverDatabase = server.GetDatabase<IContosoDatabase>();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientPeople = clientDatabase.Accounts.ToList();
				var serverAddresses = serverDatabase.Addresses.ToList();
				var serverPeople = serverDatabase.Accounts.ToList();

				Assert.AreEqual(3, clientAddresses.Count);
				Assert.AreEqual(3, clientPeople.Count);
				Assert.AreEqual(3, serverAddresses.Count);
				Assert.AreEqual(3, serverPeople.Count);

				var exclusions = GetEntityExclusions(clientAddresses[0]);
				AreEqual(clientAddresses[1].Unwrap(), serverAddresses[2].Unwrap(), exclusions);
				AreEqual(clientAddresses[2].Unwrap(), serverAddresses[1].Unwrap(), exclusions);

				exclusions = GetEntityExclusions(clientPeople[0]);
				AreEqual(clientPeople[1].Unwrap(), serverPeople[2].Unwrap(), exclusions);
				AreEqual(clientPeople[2].Unwrap(), serverPeople[1].Unwrap(), exclusions);

				Assert.AreEqual("Foo", clientAddresses[1].Line1);
				Assert.AreEqual("Foo", clientPeople[1].Name);
				Assert.AreEqual("Foo", clientPeople[1].Address.Line1);
				Assert.AreEqual(2, clientPeople[1].AddressId);
				Assert.AreEqual("Foo", serverAddresses[2].Line1);
				Assert.AreEqual("Foo", serverPeople[2].Name);
				Assert.AreEqual("Foo", serverPeople[2].Address.Line1);
				Assert.AreEqual(3, serverPeople[2].AddressId);
				Assert.AreEqual("Bar", clientAddresses[2].Line1);
				Assert.AreEqual("Bar", clientPeople[2].Name);
				Assert.AreEqual("Bar", clientPeople[2].Address.Line1);
				Assert.AreEqual(3, clientPeople[2].AddressId);
				Assert.AreEqual("Bar", serverAddresses[1].Line1);
				Assert.AreEqual("Bar", serverPeople[1].Name);
				Assert.AreEqual("Bar", serverPeople[1].Address.Line1);
				Assert.AreEqual(2, serverPeople[1].AddressId);
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServerWithLoggerOfVerbose()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo", GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Bar", GetAddress("Bar")));

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				using var listener = MemoryLogListener.CreateSession(engine.SessionId, EventLevel.Verbose);
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				var expected = client.Name.Contains("WEB") ? 18 : server.Name.Contains("WEB") ? 10 : 14;
				Assert.AreEqual(expected, listener.Events.Count, string.Join("\r\n", listener.Events.Select(x => x.GetMessage())));

				using var clientDatabase = client.GetDatabase<IContosoDatabase>();
				using var serverDatabase = server.GetDatabase<IContosoDatabase>();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientPeople = clientDatabase.Accounts.ToList();
				var serverAddresses = serverDatabase.Addresses.ToList();
				var serverPeople = serverDatabase.Accounts.ToList();

				Assert.AreEqual(3, clientAddresses.Count);
				Assert.AreEqual(3, clientPeople.Count);
				Assert.AreEqual(3, serverAddresses.Count);
				Assert.AreEqual(3, serverPeople.Count);

				var exclusions = GetEntityExclusions(clientAddresses[0]);
				AreEqual(clientAddresses[1].Unwrap(), serverAddresses[2].Unwrap(), exclusions);
				AreEqual(clientAddresses[2].Unwrap(), serverAddresses[1].Unwrap(), exclusions);

				exclusions = GetEntityExclusions(clientPeople[0]);
				AreEqual(clientPeople[1].Unwrap(), serverPeople[2].Unwrap(), exclusions);
				AreEqual(clientPeople[2].Unwrap(), serverPeople[1].Unwrap(), exclusions);

				Assert.AreEqual("Foo", clientAddresses[1].Line1);
				Assert.AreEqual("Foo", clientPeople[1].Name);
				Assert.AreEqual("Foo", clientPeople[1].Address.Line1);
				Assert.AreEqual(2, clientPeople[1].AddressId);
				Assert.AreEqual("Foo", serverAddresses[2].Line1);
				Assert.AreEqual("Foo", serverPeople[2].Name);
				Assert.AreEqual("Foo", serverPeople[2].Address.Line1);
				Assert.AreEqual(3, serverPeople[2].AddressId);
				Assert.AreEqual("Bar", clientAddresses[2].Line1);
				Assert.AreEqual("Bar", clientPeople[2].Name);
				Assert.AreEqual("Bar", clientPeople[2].Address.Line1);
				Assert.AreEqual(3, clientPeople[2].AddressId);
				Assert.AreEqual("Bar", serverAddresses[1].Line1);
				Assert.AreEqual("Bar", serverPeople[1].Name);
				Assert.AreEqual("Bar", serverPeople[1].Address.Line1);
				Assert.AreEqual(2, serverPeople[1].AddressId);
			});
		}

		[TestMethod]
		public void AddItemWithServerAndClientWithSameItem()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var person = GetAccount("Foo Bar", GetAddress("Foo"));
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(person);

				var person2 = GetAccount("Foo Bar", person.SyncId, GetAddress("Foo"));
				person2.Address.SyncId = person.Address.SyncId;
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(person2);

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using var clientDatabase = client.GetDatabase<IContosoDatabase>();
				using var serverDatabase = server.GetDatabase<IContosoDatabase>();
				var clientAddresses = clientDatabase.Addresses.ToList();
				var clientPeople = clientDatabase.Accounts.ToList();
				var serverAddresses = serverDatabase.Addresses.ToList();
				var serverPeople = serverDatabase.Accounts.ToList();

				Assert.AreEqual(2, clientAddresses.Count);
				Assert.AreEqual(2, clientPeople.Count);
				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(2, serverPeople.Count);

				var exclusions = GetEntityExclusions(clientAddresses[0]);
				AreEqual(clientAddresses[0].Unwrap(), serverAddresses[0].Unwrap(), exclusions);
				AreEqual(clientAddresses[1].Unwrap(), serverAddresses[1].Unwrap(), exclusions);

				exclusions = GetEntityExclusions(clientPeople[0]);
				AreEqual(clientPeople[0].Unwrap(), serverPeople[0].Unwrap(), exclusions);
				AreEqual(clientPeople[1].Unwrap(), serverPeople[1].Unwrap(), exclusions);
			});
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			TestHelper.Initialize();
		}

		/// <summary>
		/// This test will test the proposed scenario, client time is behind than server time by 10s
		/// Server adds address     - 12:00:59s / 12:00:49c
		/// Manual Sync
		/// Client Starts Sync      - 12:01:00s / 12:00:50c
		/// Client Reads Server     - 12:01:01s / 12:00:51c
		/// Client Writes Server    - 12:01:02s / 12:00:52c
		/// Client adds address     - 12:01:03s / 12:00:53c
		/// Client End Sync         - 12:01:04s / 12:00:54c
		/// Full Sync               - 12:01:05s / 12:00:55c
		/// Should sync the new client address
		/// </summary>
		[TestMethod]
		public void ClientTimeFasterThanServerTimeSyncStartShouldStillSync()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(initialize: false)) { Options = { IsServerClient = true } };

			// Server add address, on server time
			SetTime(new DateTime(2019, 07, 10, 12, 00, 59));
			var address = GetAddress("123 Elm Street");
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			var clientOptions = GetSyncOptions();
			var sessionId = Guid.NewGuid();

			// Do first part of syncing client 1 (client1 <- server)
			// The should not have any updates as the server has not changed
			// Set the time as server time
			SetTime(new DateTime(2019, 07, 10, 12, 01, 00));
			var clientStart = TimeService.UtcNow;
			var serverSession = server.BeginSync(sessionId, clientOptions);

			// Begin sync on client time
			SetTime(new DateTime(2019, 07, 10, 12, 00, 50));
			var clientSession = client.BeginSync(sessionId, clientOptions);

			// Reset time back to server time
			SetTime(new DateTime(2019, 07, 10, 12, 01, 01));
			var clientRequest = new SyncRequest { Since = clientOptions.LastSyncedOnServer, Until = clientStart, Skip = 0 };
			var clientResults = server.GetChanges(sessionId, clientRequest);
			Assert.AreEqual(1, clientResults.TotalCount);
			Assert.AreEqual(1, clientResults.Collection.Count);
			clientRequest.Collection = clientResults.Collection;

			// Reset time back to client time, check for client writes to server, should be none
			SetTime(new DateTime(2019, 07, 10, 12, 00, 52));
			clientRequest = new SyncRequest { Since = clientOptions.LastSyncedOnClient, Until = clientStart, Skip = 0 };
			clientResults = client.GetChanges(sessionId, clientRequest);
			Assert.AreEqual(0, clientResults.TotalCount);
			Assert.AreEqual(0, clientResults.Collection.Count);
			clientRequest.Collection = clientResults.Collection;

			// Reset time back to client time, let's add a new address on the client mid sync but behind server time
			SetTime(new DateTime(2019, 07, 10, 12, 00, 53));
			var clientAddress = GetAddress("123 Main Street");
			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(clientAddress);

			using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
			{
				var addresses = clientDatabase.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 53), addresses[0].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 53), addresses[0].ModifiedOn);
				Assert.AreEqual("123 Main Street", addresses[0].Line1);
			}

			// Reset time back to client time, and end the sync
			SetTime(new DateTime(2019, 07, 10, 12, 00, 54));
			client.EndSync(clientSession.Id);
			server.EndSync(serverSession.Id);

			// Now do a full normal sync and ensure the client address gets synced, we will set the time to server time
			// Full Sync : Go ahead and sync the data to all locations
			SetTime(new DateTime(2019, 07, 10, 12, 01, 05));
			clientOptions.LastSyncedOnServer = serverSession.StartedOn;
			clientOptions.LastSyncedOnClient = clientSession.StartedOn;

			Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 50), clientOptions.LastSyncedOnClient);
			Assert.AreEqual(new DateTime(2019, 07, 10, 12, 01, 00), clientOptions.LastSyncedOnServer);

			// Server should only have 2 address
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				var addresses = serverDatabase.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 59), addresses[0].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 59), addresses[0].ModifiedOn);
				Assert.AreEqual("123 Elm Street", addresses[0].Line1);
			}

			SyncEngine.Run(client, server, clientOptions).Dispose();

			// Server should now have 3 addresses
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				var addresses = serverDatabase.Addresses.ToList();
				Assert.AreEqual(2, addresses.Count);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 59), addresses[0].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 59), addresses[0].ModifiedOn);
				Assert.AreEqual("123 Elm Street", addresses[0].Line1);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 53), addresses[1].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 01, 05), addresses[1].ModifiedOn);
				Assert.AreEqual("123 Main Street", addresses[1].Line1);
			}

			SetTime(new DateTime(2019, 07, 10, 12, 01, 06));
			clientOptions.LastSyncedOnServer = serverSession.StartedOn;
			clientOptions.LastSyncedOnClient = clientSession.StartedOn;

			SyncEngine.Run(client, server, clientOptions).Dispose();

			using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
			{
				var addresses = clientDatabase.Addresses.ToList();
				Assert.AreEqual(1, addresses.Count);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 53), addresses[0].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 12, 01, 05), addresses[0].ModifiedOn);
				Assert.AreEqual("123 Main Street", addresses[0].Line1);
			}
		}

		[TestMethod]
		public void DeleteItemOnClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var setting1 = GetSetting("Foo", "Bar");
				var setting2 = GetSetting("Hello", "World");

				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(setting1);
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(setting2);

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				Thread.Sleep(1);
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(2, serverDatabase.Settings.Count(x => !x.IsDeleted));

					clientDatabase.Settings.Remove(x => x.Id > 1);
					clientDatabase.SaveChanges();

					Assert.AreEqual(1, clientDatabase.Settings.Count(x => x.IsDeleted));
					Assert.AreEqual(0, serverDatabase.Settings.Count(x => x.IsDeleted));
				}

				Thread.Sleep(1);
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, clientDatabase.Settings.Count(x => x.IsDeleted));

					Assert.AreEqual(1, serverDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Settings.Count(x => x.IsDeleted));
				}
			});
		}

		[TestMethod]
		public void DeleteItemOnServer()
		{
			ResetCurrentTime(null);

			TestHelper.TestServerAndClients((server, client) =>
			{
				var setting1 = GetSetting("Foo", "Bar");
				var setting2 = GetSetting("Hello", "World");

				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(setting1);
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<SettingEntity, long>(setting2);

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(2, serverDatabase.Settings.Count(x => !x.IsDeleted));

					serverDatabase.Settings.Remove(x => x.Id > 1);
					serverDatabase.SaveChanges();

					Assert.AreEqual(0, clientDatabase.Settings.Count(x => x.IsDeleted));
					Assert.AreEqual(2, clientDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Settings.Count(x => x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Settings.Count(x => !x.IsDeleted));
				}

				Thread.Sleep(1);

				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, clientDatabase.Settings.Count(x => x.IsDeleted));

					Assert.AreEqual(1, serverDatabase.Settings.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Settings.Count(x => x.IsDeleted));
				}
			});
		}

		public string[] GetEntityExclusions(ISyncEntity entity)
		{
			return entity
				.GetSyncExclusions(true, false, true)
				.AddRange("Id", nameof(ICreatedEntity.CreatedOn), nameof(IModifiableEntity.ModifiedOn))
				.ToArray();
		}

		[TestMethod]
		public void LookupFilterExpressionShouldOverrideSyncId()
		{
			SetTime(new DateTime(2019, 07, 10, 11, 59, 00));

			var keyCache1 = new DatabaseKeyCache();
			var keyCache2 = new DatabaseKeyCache();
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(keyCache: keyCache1)) { Options = { EnablePrimaryKeyCache = true } };
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(keyCache: keyCache2)) { Options = { EnablePrimaryKeyCache = true, IsServerClient = true } };
			var settingName1 = "Setting1";
			var settingName2 = "Setting2";
			var clientSetting1 = GetSetting(settingName1, "foo", Guid.Parse("00000000-0000-0000-0000-000000000001"));
			var clientSetting2 = GetSetting(settingName2, "bar", Guid.Parse("00000000-0000-0000-0000-000000000002"));
			var serverSetting1 = GetSetting(settingName1, "hello", Guid.Parse("00000000-0000-0000-0000-000000000003"));
			var serverSetting2 = GetSetting(settingName2, "world", Guid.Parse("00000000-0000-0000-0000-000000000004"));

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				database.Settings.Add(clientSetting1);
				database.Settings.Add(clientSetting2);
				database.SaveChanges();
			}

			SetTime(new DateTime(2019, 07, 10, 11, 59, 01));

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				database.Settings.Add(serverSetting1);
				database.Settings.Add(serverSetting2);
				database.SaveChanges();
			}

			SetTime(new DateTime(2019, 07, 10, 11, 59, 02));

			var options = GetSyncOptions();
			using var engine = new SyncEngine(Guid.NewGuid(), client, server, options);
			options.AddSyncableFilter(new SyncRepositoryFilter<SettingEntity>(null, null, o => x => x.Name == o.Name));
			engine.Run();

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				var count = database.Settings.Count();
				Assert.AreEqual(2, count);

				var actual = database.Settings.First(x => x.Name == settingName1);
				Assert.AreEqual(settingName1, actual.Name);
				Assert.AreEqual("hello", actual.Value);
				Assert.AreNotEqual(serverSetting1.SyncId, actual.SyncId);

				actual = database.Settings.First(x => x.Name == settingName2);
				Assert.AreEqual(settingName2, actual.Name);
				Assert.AreEqual("world", actual.Value);
				Assert.AreNotEqual(serverSetting2.SyncId, actual.SyncId);
			}

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				var count = database.Settings.Count();
				Assert.AreEqual(2, count);

				var actual = database.Settings.First(x => x.Name == settingName1);
				Assert.AreEqual(settingName1, actual.Name);
				Assert.AreEqual("hello", actual.Value);
				Assert.AreNotEqual(clientSetting2.SyncId, actual.SyncId);

				actual = database.Settings.First(x => x.Name == settingName2);
				Assert.AreEqual(settingName2, actual.Name);
				Assert.AreEqual("world", actual.Value);
				Assert.AreNotEqual(clientSetting1.SyncId, actual.SyncId);
			}
		}

		/// <summary>
		/// This test will test the proposed scenario
		/// Server adds address     - 11:59:00
		/// Manual Sync
		/// Client Starts Sync      - 12:00:00
		/// Server updates address  - 12:00:01
		/// Client Reads Server     - 12:01:00
		/// </summary>
		[TestMethod]
		public void NewlyCreatedItemModifiedAfterSyncStartShouldStillSync()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider());
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider()) { Options = { IsServerClient = true } };

			SetTime(new DateTime(2019, 07, 10, 11, 59, 00));
			var address = GetAddress("123 Elm Street");
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			var client1Options = GetSyncOptions();
			var client1Id = Guid.NewGuid();

			// Do first part of syncing client 1 (client1 <- server)
			// The should not have any updates as the server has not changed
			SetTime(new DateTime(2019, 07, 10, 12, 00, 00));
			var clientStart = TimeService.UtcNow;
			client.BeginSync(client1Id, client1Options);
			server.BeginSync(client1Id, client1Options);

			SetTime(new DateTime(2019, 07, 10, 12, 00, 01));
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				address = serverDatabase.Addresses.First(x => x.Id == address.Id);
				address.City = "Test";
				serverDatabase.SaveChanges();
			}

			var clientRequest = new SyncRequest { Since = client1Options.LastSyncedOnServer, Until = clientStart, Skip = 0 };
			var clientResults = server.GetChanges(client1Id, clientRequest);
			Assert.AreEqual(1, clientResults.TotalCount);
			Assert.AreEqual(1, clientResults.Collection.Count);
			clientRequest.Collection = clientResults.Collection;
		}

		[TestMethod]
		public void ServerClientShouldNotAcceptFilteredCorrections()
		{
			var options = GetSyncOptions();
			options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.Id > 0));

			var server = new SyncClient("Server (MEM)", TestHelper.GetSyncableMemoryProvider());
			var address = GetAddress("Home");
			var person = GetAccount("John Doe", address);

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				database.Addresses.Add(address);
				database.Accounts.Add(person);
				database.SaveChanges();
			}

			var corrections = new List<SyncObject>();
			var client = new Mock<ISyncClient>();
			var statistics = new SyncStatistics();

			client.Setup(x => x.Statistics).Returns(() => statistics);

			client.Setup(x => x.BeginSync(It.IsAny<Guid>(), It.IsAny<SyncOptions>()))
				.Returns<Guid, SyncOptions>((i, o) => new SyncSession { Id = i, StartedOn = TimeService.UtcNow });

			client.Setup(x => x.GetChanges(It.IsAny<Guid>(), It.IsAny<SyncRequest>()))
				.Returns<Guid, SyncRequest>((id, x) => new ServiceResult<SyncObject>());

			client.Setup(x => x.ApplyChanges(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) => new ServiceResult<SyncIssue>(new SyncIssue { Id = person.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = typeof(AccountEntity).ToAssemblyName() })
				);

			client.Setup(x => x.ApplyCorrections(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) =>
				{
					corrections.AddRange(x.Collection);
					return new ServiceResult<SyncIssue>();
				});

			using var engine = new SyncEngine(Guid.NewGuid(), client.Object, server, options);
			engine.Run();

			// Server should just ignore the request, there should have been no corrections
			Assert.AreEqual(0, corrections.Count);
		}

		[TestMethod]
		public void ServerLimitRepositories()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				using (var database = server.GetDatabase<IContosoDatabase>())
				{
					database.Addresses.Add(GetAddress("Work"));
					database.SaveChanges();
				}

				using (var database = client.GetDatabase<IContosoDatabase>())
				{
					var address = GetAddress("Home");
					database.Addresses.Add(address);
					database.Accounts.Add(GetAccount("John Doe", address));
					database.SaveChanges();
				}

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Accounts.Count());

					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
				}

				var options = GetSyncOptions();
				options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, options);
				Thread.Sleep(10);
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count);

				// We should have synced only the address but not the people!

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Accounts.Count());

					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
				}
			}, false);
		}

		[TestMethod]
		public void ServerLimitRepository()
		{
			ResetCurrentTime();

			TestHelper.TestServerAndClients((server, client) =>
			{
				AccountEntity account;

				using (var database = server.GetDatabase<IContosoDatabase>())
				{
					var address = GetAddress("Work");
					database.Addresses.Add(address);
					account = GetAccount("Jane Doe", address);
					database.Accounts.Add(account);
					database.SaveChanges();
				}

				Thread.Sleep(10);

				using (var database = client.GetDatabase<IContosoDatabase>())
				{
					var address = GetAddress("Home");
					database.Addresses.Add(address);
					database.Accounts.Add(GetAccount("John Doe", address));
					database.SaveChanges();
				}

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());

					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
				}

				using (var listener = LogListener.CreateSession(Guid.NewGuid(), EventLevel.Informational))
				{
					listener.OutputToConsole = true;

					var options = GetSyncOptions();
					options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					options.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>(x => x.SyncId == account.SyncId));

					Thread.Sleep(10);

					using var engine = new SyncEngine(listener.SessionId, client, server, options);
					engine.Run();

					Assert.AreEqual(0, engine.SyncIssues.Count);
				}

				// We should have synced only the address but not the people!

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					// Client should have all
					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(3, clientDatabase.Accounts.Count());

					// Server should only have the address
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());
				}
			}, false);
		}

		[TestMethod]
		public void ServerShouldNotPushNonModifiedEntities()
		{
			SetTime(new DateTime(2019, 07, 10, 11, 59, 00));

			var server = new TestSyncClient("Server (Test)");
			var client = new SyncClient("Client (MEM)", TestHelper.GetSyncableMemoryProvider());

			var address = GetAddress("123 Elm Street");
			server.Changes.Add(new ServiceResult<SyncObject>(address.ToSyncObject()));

			SetTime(new DateTime(2019, 07, 10, 11, 59, 01));

			using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
			engine.Run();

			Assert.AreEqual(1, server.AppliedChanges.Count);
		}

		[TestMethod]
		public void SoftDeletedEntitiesShouldNotSync()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = GetAddress("Blah");
				address.IsDeleted = true;
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					var addresses2 = serverDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					Assert.AreEqual(0, addresses1.Count);
					Assert.AreEqual(1, addresses2.Count);
				}

				var options = GetSyncOptions();
				options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(skipDeletedItemsOnInitialSync: true));
				var issues = SyncEngine.Run(client, server, options).SyncIssues;
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					var addresses2 = serverDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					Assert.AreEqual(0, addresses1.Count);
					Assert.AreEqual(1, addresses2.Count);
				}

				issues = CompleteTestSync(client, server, options);
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));
			}, false);
		}

		[TestMethod]
		public void SoftDeletedEntitiesShouldSync()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = GetAddress("Blah");
				address.IsDeleted = true;
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					var addresses2 = serverDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					Assert.AreEqual(0, addresses1.Count);
					Assert.AreEqual(1, addresses2.Count);
				}

				var options = GetSyncOptions();
				options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(skipDeletedItemsOnInitialSync: false));
				var issues = SyncEngine.Run(client, server, options).SyncIssues;
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					var addresses2 = serverDatabase.Addresses.Where(x => x.Id == address.Id).ToList();
					Assert.AreEqual(1, addresses1.Count);
					Assert.AreEqual(1, addresses2.Count);
				}

				issues = CompleteTestSync(client, server, options);
				Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));
			}, false);
		}

		/// <summary>
		/// This test will test the proposed scenario
		/// Server adds address       - 11:59:00
		/// Sync Set 1
		/// Client 1 Full Syncs       - 11:59:01
		/// Client 2 Full Syncs       - 11:59:02
		/// Client 2 updates address  - 11:59:04
		/// Manual Sync
		/// Client 1 Starts Sync      - 12:00:00
		/// Client 1 Reads Server     - 12:00:00
		/// Client 2 Starts Sync      - 12:01:00
		/// Client 2 Reads Server     - 12:01:00
		/// Client 2 Writes Server    - 12:02:00
		/// Server now has new address and the ModifiedOn is set to server time of 12:02:00
		/// Client 2 Ends Sync        - 12:02:01 (Last Synced on: 12:01:00)
		/// Client 1 Writes Server    - 12:03:00
		/// Client 1 Ends Sync        - 12:03:01 (Last Synced on: 12:00:00)
		/// Sync Set 2
		/// Next sync should pull down the address again due to server maintaining modified on.
		/// Client 1 Full Syncs       - 12:04:00 (Since: 12:00:00 - Until: 12:04:00) (Last Synced On: 12:04:00)
		/// Client gets new address because server update ModifiedOn to it's time which is greater than sync start
		/// Client 2 Full Syncs       - 12:04:01 (Since: 12:01:00 - Until: 12:04:01) (Last Synced On: 12:04:01)
		/// Client gets new address because server update ModifiedOn to it's time which is greater than sync start
		/// Sync Set 3
		/// Final sync should not move any data
		/// Client 1 Full Syncs       - 12:05:00 (Since: 12:04:00 - Until: 12:05:00) (Last Synced On: 12:05:00)
		/// Client 2 Full Syncs       - 12:05:01 (Since: 12:04:01 - Until: 12:05:01) (Last Synced On: 12:05:01)
		/// </summary>
		[TestMethod]
		public void ThreeWaySyncShouldWork()
		{
			SetTime(new DateTime(2019, 07, 10, 11, 58, 00));

			using var listener = LogListener.CreateSession(Guid.Empty, EventLevel.Verbose, x => x.OutputToConsole = true);
			var serverMemoryProvider = TestHelper.GetSyncableMemoryProvider(initialize: false);
			var client1 = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var client2 = new SyncClient("Client 2", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server1 = new SyncClient("Server", serverMemoryProvider) { Options = { IsServerClient = true } };
			var server2 = new SyncClient("Server2", serverMemoryProvider) { Options = { IsServerClient = true } };

			SetTime(new DateTime(2019, 07, 10, 11, 59, 00));
			var address1 = GetAddress("123 Elm Street", null, Guid.Parse("00000000-0000-0000-0000-000000000001"));
			server1.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address1);

			// Make sure all data is only on the server
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				Assert.AreEqual(1, serverAddresses.Count);
				Assert.AreEqual("123 Elm Street", serverAddresses[0].Line1);
				Assert.AreEqual(new DateTime(2019, 07, 10, 11, 59, 00), serverAddresses[0].CreatedOn);
				Assert.AreEqual(new DateTime(2019, 07, 10, 11, 59, 00), serverAddresses[0].ModifiedOn);
				Assert.AreEqual(0, client1Database.Addresses.Count());
				Assert.AreEqual(0, client2Database.Addresses.Count());
			}

			var client1Options = GetSyncOptions();
			var client2Options = GetSyncOptions();

			// Sync Set 1: Go ahead and sync the data to all locations
			"\r\nSync Set 1".Dump();
			SetTime(new DateTime(2019, 07, 10, 11, 59, 01));
			SyncEngine.Run(client1, server1, client1Options).Dispose();
			SetTime(new DateTime(2019, 07, 10, 11, 59, 02));
			SyncEngine.Run(client2, server2, client2Options).Dispose();

			// Prepare a new address for client 2
			SetTime(new DateTime(2019, 07, 10, 11, 59, 03));
			var address2 = GetAddress("123 Main Street", null, Guid.Parse("00000000-0000-0000-0000-000000000002"));

			// Make sure all data is there
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(1, serverAddresses.Count);
				Assert.AreEqual(1, client1Addresses.Count);
				Assert.AreEqual(1, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true).ToArray();
				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);

				// Add another address to client 2
				SetTime(new DateTime(2019, 07, 10, 11, 59, 04));
				client2Database.Addresses.Add(address2);
				client2Database.SaveChanges();
			}

			var client1Id = Guid.NewGuid();
			var client2Id = Guid.NewGuid();

			// Manual Sync

			// Do first part of syncing client 1 (client1 <- server1)
			// The should not have any updates as the server has not changed
			SetTime(new DateTime(2019, 07, 10, 12, 00, 00));
			"\r\nClient1 Begin".Dump();
			var client1Start = TimeService.UtcNow;
			var serverSession1 = server1.BeginSync(client1Id, client1Options);
			client1.BeginSync(client1Id, client1Options);
			var client1Request = new SyncRequest { Since = client1Options.LastSyncedOnServer, Until = client1Start, Skip = 0 };
			var client1Results = server1.GetChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Results.TotalCount);
			Assert.AreEqual(0, client1Results.Collection.Count);
			client1Request.Collection = client1Results.Collection;
			SetTime(new DateTime(2019, 07, 10, 12, 00, 01));
			var client1Issues = client1.ApplyChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Issues.TotalCount);
			Assert.AreEqual(0, client1Issues.Collection.Count);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(1, serverAddresses.Count);
				Assert.AreEqual(1, client1Addresses.Count);
				Assert.AreEqual(2, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true).ToArray();
				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);
			}

			// Do first part of syncing client 2 (client2 <- server 2)
			// This should not have any updates as the server has not changed
			SetTime(new DateTime(2019, 07, 10, 12, 01, 00));
			"\r\nClient2 Begin".Dump();
			var serverStart = TimeService.UtcNow;
			var client2Start = serverStart;
			var serverSession2 = server2.BeginSync(client2Id, client2Options);
			client2.BeginSync(client2Id, client2Options);
			var client2Request = new SyncRequest { Since = client2Options.LastSyncedOnServer, Until = serverStart, Skip = 0 };
			var client2Results = server2.GetChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Results.TotalCount);
			Assert.AreEqual(0, client2Results.Collection.Count);
			client2Request.Collection = client2Results.Collection;
			SetTime(new DateTime(2019, 07, 10, 12, 01, 01));
			var client2Issues = client2.ApplyChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Issues.TotalCount);
			Assert.AreEqual(0, client2Issues.Collection.Count);

			// Data should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(1, serverAddresses.Count);
				Assert.AreEqual(1, client1Addresses.Count);
				Assert.AreEqual(2, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true).ToArray();
				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);
			}

			// Do second part of client 2 (client2 -> server2)
			SetTime(new DateTime(2019, 07, 10, 12, 02, 00));
			"\r\nClient2 Get Changes -> Server2 Apply".Dump();
			client2Request = new SyncRequest { Since = client2Options.LastSyncedOnClient, Until = client2Start, Skip = 0 };
			client2Results = client2.GetChanges(client2Id, client2Request);
			Assert.AreEqual(1, client2Results.TotalCount);
			Assert.AreEqual(1, client2Results.Collection.Count);
			Assert.AreEqual(address2.SyncId, client2Results.Collection[0].SyncId);
			client2Request.Collection = client2Results.Collection;
			SetTime(new DateTime(2019, 07, 10, 12, 02, 01));
			client2Issues = server2.ApplyChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Issues.TotalCount);
			Assert.AreEqual(0, client2Issues.Collection.Count);
			client2.EndSync(serverSession2.Id);
			server2.EndSync(serverSession2.Id);

			// Data still should now have synced to the server
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server2.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(1, client1Addresses.Count);
				Assert.AreEqual(2, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true)
					.AddRange(nameof(IModifiableEntity.ModifiedOn))
					.ToArray();

				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[1].Unwrap(), client2Addresses[1].Unwrap(), exclusions);
			}

			// Do second part of client 1 (client1 -> server1)
			SetTime(new DateTime(2019, 07, 10, 12, 03, 00));
			"\r\nClient1 Get Changes -> Server1 Apply".Dump();
			client1Request = new SyncRequest { Since = client1Options.LastSyncedOnClient, Until = client1Start, Skip = 0 };
			client1Results = client1.GetChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Results.TotalCount);
			Assert.AreEqual(0, client1Results.Collection.Count);
			client1Request.Collection = client1Results.Collection;
			SetTime(new DateTime(2019, 07, 10, 12, 03, 01));
			client1Issues = server1.ApplyChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Issues.TotalCount);
			Assert.AreEqual(0, client1Issues.Collection.Count);
			client1.EndSync(serverSession1.Id);
			server1.EndSync(serverSession1.Id);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(1, client1Addresses.Count);
				Assert.AreEqual(2, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true)
					.AddRange(nameof(IModifiableEntity.ModifiedOn))
					.ToArray();

				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[1].Unwrap(), client2Addresses[1].Unwrap(), exclusions);
			}

			// Sync Set 2: Go ahead and sync the data to all locations
			SetTime(new DateTime(2019, 07, 10, 12, 04, 00));
			"\r\nSync Set 2".Dump();
			client1Options.LastSyncedOnServer = serverStart;
			client1Options.LastSyncedOnClient = client1Start;
			SyncEngine.Run(client1, server1, client1Options).Dispose();
			Assert.IsFalse(client1.Statistics.IsReset);
			Assert.IsFalse(server1.Statistics.IsReset);
			SetTime(new DateTime(2019, 07, 10, 12, 04, 01));
			client2Options.LastSyncedOnServer = serverStart;
			client2Options.LastSyncedOnClient = client2Start;
			SyncEngine.Run(client2, server2, client2Options).Dispose();
			Assert.IsFalse(client2.Statistics.IsReset);
			Assert.IsFalse(server2.Statistics.IsReset);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server1.GetDatabase<IContosoDatabase>())
			{
				var serverAddresses = serverDatabase.Addresses.ToList();
				var client1Addresses = client1Database.Addresses.ToList();
				var client2Addresses = client2Database.Addresses.ToList();

				Assert.AreEqual(2, serverAddresses.Count);
				Assert.AreEqual(2, client1Addresses.Count);
				Assert.AreEqual(2, client2Addresses.Count);

				var exclusions = client1Addresses[0].GetSyncExclusions(true, false, true)
					.AddRange(nameof(IModifiableEntity.ModifiedOn))
					.ToArray();

				AreEqual(serverAddresses[0].Unwrap(), client1Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[0].Unwrap(), client2Addresses[0].Unwrap(), exclusions);
				AreEqual(serverAddresses[1].Unwrap(), client1Addresses[1].Unwrap(), exclusions);
				AreEqual(serverAddresses[1].Unwrap(), client2Addresses[1].Unwrap(), exclusions);
			}

			// Sync Set 3
			SetTime(new DateTime(2019, 07, 10, 12, 05, 00));
			"\r\nSync Set 3".Dump();
			client1Options.LastSyncedOnServer = client1Options.LastSyncedOnServer;
			client1Options.LastSyncedOnClient = client1Options.LastSyncedOnClient;
			SyncEngine.Run(client1, server1, client1Options).Dispose();
			Assert.IsTrue(client1.Statistics.IsReset);
			Assert.IsTrue(server1.Statistics.IsReset);
			SetTime(new DateTime(2019, 07, 10, 12, 05, 01));
			client2Options.LastSyncedOnServer = client2Options.LastSyncedOnServer;
			client2Options.LastSyncedOnClient = client2Options.LastSyncedOnClient;
			SyncEngine.Run(client2, server2, client2Options).Dispose();
			Assert.IsTrue(client2.Statistics.IsReset);
			Assert.IsTrue(server2.Statistics.IsReset);
		}

		[TestMethod]
		public void UpdateItemOnClientThenServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = GetAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				address = (AddressEntity) address.Unwrap();
				address.Id = 0;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());

					clientDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line1 = "Foo Client";
					clientDatabase.SaveChanges();

					serverDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line1 = "Foo Server";
					serverDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line2 = "Foo Server2";
					serverDatabase.SaveChanges();
				}

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual("Foo Server", clientDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line1);
					Assert.AreEqual("Foo Server2", clientDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line2);
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual("Foo Server", serverDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line1);
					Assert.AreEqual("Foo Server2", serverDatabase.Addresses.First(x => x.SyncId == address.SyncId).Line2);
				}
			});
		}

		[TestMethod]
		public void UpdateItemOnServerThenClient()
		{
			ResetCurrentTime(null);

			TestHelper.TestServerAndClients((server, client) =>
			{
				Assert.AreNotEqual(server, client);

				var serverAddress = GetAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(serverAddress);

				var clientAddress = (AddressEntity) serverAddress.Unwrap();
				clientAddress.Id = 0;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(clientAddress);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.IsFalse(ReferenceEquals(clientDatabase, serverDatabase));

					Assert.AreNotEqual(clientDatabase, serverDatabase);
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());

					serverDatabase.Addresses.First(x => x.Id == serverAddress.Id).Line1 = "123 Server Street";
					serverDatabase.Addresses.First(x => x.Id == serverAddress.Id).Line2 = "Server2";
					serverDatabase.SaveChanges();
					
					Thread.Sleep(1);

					clientDatabase.Addresses.First(x => x.Id == clientAddress.Id).Line1 = "123 Client Street";
					clientDatabase.SaveChanges();
				}

				Thread.Sleep(1);

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				Assert.IsFalse(engine.SyncIssues.Count > 0, string.Join(",", engine.SyncIssues.Select(x => x.Message)));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", clientDatabase.Addresses.First(x => x.Id == serverAddress.Id).Line1);
					Assert.AreEqual(serverAddress.Line2, clientDatabase.Addresses.First(x => x.Id == serverAddress.Id).Line2);
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", serverDatabase.Addresses.First(x => x.Id == clientAddress.Id).Line1);
					Assert.AreEqual(serverAddress.Line2, serverDatabase.Addresses.First(x => x.Id == clientAddress.Id).Line2);
				}
			});
		}

		[TestMethod]
		public void UseItemOnClientAndServerDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo Bar", GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Bar"));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Accounts.Count());
				}

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join("\r\n", engine.SyncIssues));

				AddressEntity removedAddress;
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var account = serverDatabase.Accounts.Including(x => x.Address).OrderBy(x => x.Id).Skip(1).First();
					Assert.AreEqual("Foo", account.Address.Line1);
					AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					AreEqual(serverDatabase.Accounts.Count(), clientDatabase.Accounts.Count());
					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());

					var person = clientDatabase.Accounts.First(x => x.SyncId == account.SyncId);
					person.Address = clientDatabase.Addresses.First(x => x.Line1 == "Bar");
					clientDatabase.SaveChanges();

					removedAddress = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.Addresses.Remove(removedAddress);
					serverDatabase.SaveChanges();
				}

				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count, string.Join("\r\n", engine.SyncIssues));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var serverPerson = serverDatabase
						.Accounts
						.Including(x => x.Address)
						.OrderBy(x => x.Id)
						.Skip(1)
						.First();

					Assert.AreEqual("Bar", serverPerson.Address.Line1);
					Assert.AreEqual(removedAddress.SyncId, serverPerson.AddressSyncId);
					Assert.AreEqual(removedAddress.SyncId, serverPerson.Address.SyncId);
					AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					AreEqual(serverDatabase.Accounts.Count(), clientDatabase.Accounts.Count());
					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());
				}
			});
		}

		/// <summary>
		/// This test will create the same entities on each side of the sync. However the server will switch relationships before the client
		/// is able to delete it. This should prevent the clients from deleting any entities.
		/// </summary>
		[TestMethod]
		public void UseItemOnServerAndClientDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AccountEntity, int>(GetAccount("Foo Bar", GetAddress("Foo")));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(GetAddress("Bar"));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Accounts.Count());
				}

				using var engine = new SyncEngine(Guid.NewGuid(), client, server, GetSyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual("Foo", serverDatabase.Accounts.Include(x => x.Address).OrderBy(x => x.Id).Skip(1).First().Address.Line1);
					AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					AreEqual(serverDatabase.Accounts.Count(), clientDatabase.Accounts.Count());
					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());

					serverDatabase.Accounts.First().Address = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.SaveChanges();
					var removedAddress = clientDatabase.Addresses.First(x => x.Line1 == "Bar");
					clientDatabase.Addresses.Remove(removedAddress);
					clientDatabase.SaveChanges();
				}

				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual("Foo", serverDatabase.Accounts.Include(x => x.Address).OrderBy(x => x.Id).Skip(1).First().Address.Line1);
					Assert.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					Assert.AreEqual(serverDatabase.Accounts.Count(), clientDatabase.Accounts.Count());
					Assert.AreEqual(3, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.Accounts.Count());
					Assert.AreEqual(3, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Accounts.Count());
				}
			});
		}

		private IReadOnlyList<SyncIssue> CompleteTestSync(ISyncClient client, ISyncClient server, SyncOptions options, bool doublePass = true)
		{
			IReadOnlyList<SyncIssue> issues1;

			if (doublePass)
			{
				Thread.Sleep(10);

				// Catch up sync
				using var engine1 = SyncEngine.Run(client, server, options);
				issues1 = engine1.SyncIssues;
				var results = string.Join("\r\n", issues1.Select(x => x.Message));
				Assert.IsFalse(server.Statistics.IsReset, results);
				Assert.IsFalse(client.Statistics.IsReset, results);
				Assert.AreEqual(0, server.Statistics.IndividualProcessCount);
				Assert.AreEqual(0, client.Statistics.IndividualProcessCount);
			}
			else
			{
				issues1 = new List<SyncIssue>();
			}

			Thread.Sleep(10);

			// Final sync
			using var engine2 = SyncEngine.Run(client, server, options);
			var issues2 = engine2.SyncIssues;
			Assert.IsTrue(server.Statistics.IsReset);
			Assert.IsTrue(client.Statistics.IsReset);

			return issues1.Concat(issues2).ToList();
		}

		private static AccountEntity GetAccount(string name, AddressEntity address)
		{
			return GetAccount(name, Guid.NewGuid(), address);
		}

		private static AccountEntity GetAccount(string name, Guid syncId, AddressEntity address)
		{
			var time = TimeService.UtcNow;
			return new AccountEntity
			{
				Address = address,
				CreatedOn = time,
				EmailAddress = Guid.NewGuid().ToString(),
				ModifiedOn = time,
				Name = name,
				SyncId = syncId
			};
		}

		private static AddressEntity GetAddress(string line1, string line2 = null, Guid? syncId = null)
		{
			var time = TimeService.UtcNow;
			return new AddressEntity
			{
				Line1 = line1,
				Line2 = line2 ?? Guid.NewGuid().ToString(),
				City = Guid.NewGuid().ToString(),
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString(),
				SyncId = syncId ?? Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		private static SettingEntity GetSetting(string name, string value, Guid? syncId = null)
		{
			var time = TimeService.UtcNow;
			return new SettingEntity
			{
				Name = name,
				Value = value,
				SyncId = syncId ?? Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		private SyncOptions GetSyncOptions()
		{
			return new SyncOptions
			{
				IncludeIssueDetails = true
			};
		}

		#endregion
	}
}