﻿#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Client.Samples.Models;
using Speedy.Logging;
using Speedy.Net;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Tests
{
	/// <summary>
	/// Summary description for SyncEngineTests.
	/// </summary>
	[TestClass]
	public class SyncEngineTests
	{
		#region Methods

		[TestMethod]
		public void AddItemToClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Blah"));

				var options = new SyncOptions();
				var issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					TestHelper.AreEqual(clientDatabase.Addresses.First().Unwrap(), serverDatabase.Addresses.First().Unwrap());
				}

				issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);
				Assert.IsTrue(client.Statistics.IsReset);
				Assert.IsTrue(client.Statistics.IsReset);
			});
		}

		[TestMethod]
		public void AddItemToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Foo"));
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Bar"));

				var options = new SyncOptions();
				var issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();

					Assert.AreEqual(2, addresses1.Count);
					Assert.AreEqual(2, addresses2.Count);

					TestHelper.AreEqual(addresses1[0].Unwrap(), addresses2[1].Unwrap(), nameof(AddressEntity.Id), nameof(ISyncEntity.ModifiedOn));
					TestHelper.AreEqual(addresses1[1].Unwrap(), addresses2[0].Unwrap(), nameof(AddressEntity.Id), nameof(ISyncEntity.ModifiedOn));
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
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Bar"));

				var options = new SyncOptions();
				var issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var clientAddresses = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
					var serverAddresses = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();

					Assert.AreEqual(2, serverDatabase.People.First().Address.Id);
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());

					TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), nameof(AddressEntity.Id), nameof(ISyncEntity.ModifiedOn));
					TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), nameof(AddressEntity.Id), nameof(ISyncEntity.ModifiedOn));
				}

				CompleteTestSync(client, server, options);
			});
		}

		[TestMethod]
		public void AddItemToClientWithLogger()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Blah"));

				var options = new SyncOptions();
				var issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					TestHelper.AreEqual(clientDatabase.Addresses.First().Unwrap(), serverDatabase.Addresses.First().Unwrap());
				}

				issues = SyncEngine.Run(client, server, options);

				Assert.AreEqual(0, issues.Count);
				Assert.IsTrue(client.Statistics.IsReset);
				Assert.IsTrue(client.Statistics.IsReset);
			});
		}

		[TestMethod]
		public void AddItemToServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var clientAddresses = clientDatabase.Addresses.ToList();
					var clientPeople = clientDatabase.People.ToList();
					var serverAddresses = serverDatabase.Addresses.ToList();
					var serverPeople = serverDatabase.People.ToList();

					Assert.AreEqual(2, clientAddresses.Count);
					Assert.AreEqual(2, clientPeople.Count);
					Assert.AreEqual(2, serverAddresses.Count);
					Assert.AreEqual(2, serverPeople.Count);

					TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
					TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
					TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));
					TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));

					Assert.AreEqual("Foo", clientAddresses[0].Line1);
					Assert.AreEqual("Foo", clientPeople[0].Name);
					Assert.AreEqual("Foo", clientPeople[0].Address.Line1);
					Assert.AreEqual(1, clientPeople[0].AddressId);
					Assert.AreEqual("Foo", serverAddresses[1].Line1);
					Assert.AreEqual("Foo", serverPeople[1].Name);
					Assert.AreEqual("Foo", serverPeople[1].Address.Line1);
					Assert.AreEqual(2, serverPeople[1].AddressId);
					Assert.AreEqual("Bar", clientAddresses[1].Line1);
					Assert.AreEqual("Bar", clientPeople[1].Name);
					Assert.AreEqual("Bar", clientPeople[1].Address.Line1);
					Assert.AreEqual(2, clientPeople[1].AddressId);
					Assert.AreEqual("Bar", serverAddresses[0].Line1);
					Assert.AreEqual("Bar", serverPeople[0].Name);
					Assert.AreEqual("Bar", serverPeople[0].Address.Line1);
					Assert.AreEqual(1, serverPeople[0].AddressId);
				}
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServerWithLogger()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				using (var listener = new LogListener(engine.SessionId))
				{
					engine.Run();

					Assert.AreEqual(8, listener.Events.Count, string.Join("\r\n", listener.Events.Select(x => x.ToPayloadString())));

					using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
					using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
					{
						var clientAddresses = clientDatabase.Addresses.ToList();
						var clientPeople = clientDatabase.People.ToList();
						var serverAddresses = serverDatabase.Addresses.ToList();
						var serverPeople = serverDatabase.People.ToList();

						Assert.AreEqual(2, clientAddresses.Count);
						Assert.AreEqual(2, clientPeople.Count);
						Assert.AreEqual(2, serverAddresses.Count);
						Assert.AreEqual(2, serverPeople.Count);

						TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));

						Assert.AreEqual("Foo", clientAddresses[0].Line1);
						Assert.AreEqual("Foo", clientPeople[0].Name);
						Assert.AreEqual("Foo", clientPeople[0].Address.Line1);
						Assert.AreEqual(1, clientPeople[0].AddressId);
						Assert.AreEqual("Foo", serverAddresses[1].Line1);
						Assert.AreEqual("Foo", serverPeople[1].Name);
						Assert.AreEqual("Foo", serverPeople[1].Address.Line1);
						Assert.AreEqual(2, serverPeople[1].AddressId);
						Assert.AreEqual("Bar", clientAddresses[1].Line1);
						Assert.AreEqual("Bar", clientPeople[1].Name);
						Assert.AreEqual("Bar", clientPeople[1].Address.Line1);
						Assert.AreEqual(2, clientPeople[1].AddressId);
						Assert.AreEqual("Bar", serverAddresses[0].Line1);
						Assert.AreEqual("Bar", serverPeople[0].Name);
						Assert.AreEqual("Bar", serverPeople[0].Address.Line1);
						Assert.AreEqual(1, serverPeople[0].AddressId);
					}
				}
			});
		}

		[TestMethod]
		public void AddItemWithRelationshipToClientAndServerWithLoggerOfVerbose()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				using (var listener = new LogListener(engine.SessionId, EventLevel.Verbose))
				{
					engine.Run();

					var expected = client.Name.Contains("WEB") ? 12 : server.Name.Contains("WEB") ? 10 : 12;
					Assert.AreEqual(expected, listener.Events.Count, string.Join("\r\n", listener.Events.Select(x => x.ToPayloadString())));

					using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
					using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
					{
						var clientAddresses = clientDatabase.Addresses.ToList();
						var clientPeople = clientDatabase.People.ToList();
						var serverAddresses = serverDatabase.Addresses.ToList();
						var serverPeople = serverDatabase.People.ToList();

						Assert.AreEqual(2, clientAddresses.Count);
						Assert.AreEqual(2, clientPeople.Count);
						Assert.AreEqual(2, serverAddresses.Count);
						Assert.AreEqual(2, serverPeople.Count);

						TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id", nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));
						TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(PersonEntity.AddressId), nameof(IModifiableEntity.ModifiedOn));

						Assert.AreEqual("Foo", clientAddresses[0].Line1);
						Assert.AreEqual("Foo", clientPeople[0].Name);
						Assert.AreEqual("Foo", clientPeople[0].Address.Line1);
						Assert.AreEqual(1, clientPeople[0].AddressId);
						Assert.AreEqual("Foo", serverAddresses[1].Line1);
						Assert.AreEqual("Foo", serverPeople[1].Name);
						Assert.AreEqual("Foo", serverPeople[1].Address.Line1);
						Assert.AreEqual(2, serverPeople[1].AddressId);
						Assert.AreEqual("Bar", clientAddresses[1].Line1);
						Assert.AreEqual("Bar", clientPeople[1].Name);
						Assert.AreEqual("Bar", clientPeople[1].Address.Line1);
						Assert.AreEqual(2, clientPeople[1].AddressId);
						Assert.AreEqual("Bar", serverAddresses[0].Line1);
						Assert.AreEqual("Bar", serverPeople[0].Name);
						Assert.AreEqual("Bar", serverPeople[0].Address.Line1);
						Assert.AreEqual(1, serverPeople[0].AddressId);
					}
				}
			});
		}

		[TestMethod]
		public void AddItemWithServerAndClientWithSameItem()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var person = new PersonEntity { Address = NewAddress("Foo"), Name = "Foo Bar" };
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(person);

				var person2 = new PersonEntity { Address = NewAddress("Foo"), Name = "Foo Bar", SyncId = person.SyncId };
				person2.Address.SyncId = person.Address.SyncId;
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(person2);

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());

					TestHelper.AreEqual(clientDatabase.Addresses.First().Unwrap(), serverDatabase.Addresses.First().Unwrap(), "Id");
					TestHelper.AreEqual(clientDatabase.People.First().Unwrap(), serverDatabase.People.First().Unwrap(), "Id");
				}
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
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider());
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider()) { Options = { MaintainModifiedOn = true } };

			// Server add address, on server time
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 59);
			var address = NewAddress("123 Elm Street");
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			var clientOptions = new SyncOptions();
			var sessionId = Guid.NewGuid();

			// Do first part of syncing client 1 (client1 <- server)
			// The should not have any updates as the server has not changed
			// Set the time as server time
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 01, 00);
			var clientStart = TimeService.UtcNow;
			var serverSession = server.BeginSync(sessionId, clientOptions);

			// Begin sync on client time
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 50);
			var clientSession = client.BeginSync(sessionId, clientOptions);

			// Reset time back to server time
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 01, 01);
			var clientRequest = new SyncRequest { Since = clientOptions.LastSyncedOnServer, Until = clientStart, Skip = 0 };
			var clientResults = server.GetChanges(sessionId, clientRequest);
			Assert.AreEqual(1, clientResults.TotalCount);
			Assert.AreEqual(1, clientResults.Collection.Count);
			clientRequest.Collection = clientResults.Collection;

			// Reset time back to client time, check for client writes to server, should be none
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 52);
			clientRequest = new SyncRequest { Since = clientOptions.LastSyncedOnClient, Until = clientStart, Skip = 0 };
			clientResults = client.GetChanges(sessionId, clientRequest);
			Assert.AreEqual(0, clientResults.TotalCount);
			Assert.AreEqual(0, clientResults.Collection.Count);
			clientRequest.Collection = clientResults.Collection;

			// Reset time back to client time, let's add a new address on the client mid sync but behind server time
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 53);
			var clientAddress = NewAddress("123 Main Street");
			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(clientAddress);

			// Reset time back to client time, and end the sync
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 54);
			client.EndSync(clientSession);
			server.EndSync(serverSession);

			// Now do a full normal sync and ensure the client address gets synced, we will set the time to server time
			// Full Sync : Go ahead and sync the data to all locations
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 01, 05);
			clientOptions.LastSyncedOnServer = serverSession.StartedOn;
			clientOptions.LastSyncedOnClient = clientSession.StartedOn;

			Assert.AreEqual(new DateTime(2019, 07, 10, 12, 00, 50), clientOptions.LastSyncedOnClient);
			Assert.AreEqual(new DateTime(2019, 07, 10, 12, 01, 00), clientOptions.LastSyncedOnServer);

			// Server should only have 1 address
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, serverDatabase.Addresses.Count());
			}

			SyncEngine.Run(client, server, clientOptions);

			// Server should now have 2 addresses
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(2, serverDatabase.Addresses.Count());
			}
		}

		[TestMethod]
		public void DeleteItemOnClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				var address2 = NewAddress("123 Elm Street");
				address2.SyncId = address.SyncId;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address2);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());

					clientDatabase.Addresses.Remove(x => x.SyncId == address.SyncId);
					clientDatabase.SaveChanges();

					Assert.AreEqual(0, clientDatabase.Addresses.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, clientDatabase.Addresses.Count(x => x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Addresses.Count(x => !x.IsDeleted));
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count(x => x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Addresses.Count(x => x.IsDeleted));
				}
			});
		}

		[TestMethod]
		public void DeleteItemOnServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				var address2 = NewAddress("123 Elm Street");
				address2.SyncId = address.SyncId;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address2);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Addresses.Count(x => !x.IsDeleted));

					serverDatabase.Addresses.Remove(x => x.Id > 0);
					serverDatabase.SaveChanges();

					Assert.AreEqual(0, clientDatabase.Addresses.Count(x => x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Addresses.Count(x => x.IsDeleted));
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(0, clientDatabase.Addresses.Count(x => !x.IsDeleted));
					Assert.AreEqual(0, serverDatabase.Addresses.Count(x => !x.IsDeleted));
					Assert.AreEqual(1, clientDatabase.Addresses.Count(x => x.IsDeleted));
					Assert.AreEqual(1, serverDatabase.Addresses.Count(x => x.IsDeleted));
				}
			});
		}

		[TestCleanup]
		public void Initialize()
		{
			TimeService.Reset();
		}

		[TestMethod]
		public void LookupFilterExpressionShouldOverrideSyncId()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider());
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider()) { Options = { MaintainModifiedOn = true } };
			var options = new SyncOptions();
			var engine = new SyncEngine(client, server, options);
			var settingName1 = "Setting1";
			var settingName2 = "Setting2";
			var clientSetting1 = NewSetting(settingName1, "foo");
			var clientSetting2 = NewSetting(settingName2, "bar");
			var serverSetting1 = NewSetting(settingName1, "hello");
			var serverSetting2 = NewSetting(settingName2, "world");

			using (var database = client.GetDatabase<IContosoDatabase>())
			{
				database.Settings.Add(clientSetting1);
				database.Settings.Add(clientSetting2);
				database.SaveChanges();
			}

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				database.Settings.Add(serverSetting1);
				database.Settings.Add(serverSetting2);
				database.SaveChanges();
			}

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
		/// Server adds address       - 11:59:00
		/// Manual Sync
		/// Client Starts Sync      - 12:00:00
		/// Server updates address    - 12:00:01
		/// Client Reads Server     - 12:01:00
		/// </summary>
		[TestMethod]
		public void NewlyCreatedItemModifiedAfterSyncStartShouldStillSync()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider());
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider()) { Options = { MaintainModifiedOn = true } };

			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 00);
			var address = NewAddress("123 Elm Street");
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			var client1Options = new SyncOptions();
			var client1Id = Guid.NewGuid();

			// Do first part of syncing client 1 (client1 <- server)
			// The should not have any updates as the server has not changed
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 00);
			var clientStart = TimeService.UtcNow;
			client.BeginSync(client1Id, client1Options);
			server.BeginSync(client1Id, client1Options);

			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 01);
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
			var options = new SyncOptions();
			options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.Id > 0));

			var server = new SyncClient("Server (MEM)", TestHelper.GetSyncableMemoryProvider());
			var address = NewAddress("Home");
			var person = NewPerson("John Doe", address);

			using (var database = server.GetDatabase<IContosoDatabase>())
			{
				database.Addresses.Add(address);
				database.People.Add(person);
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
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) => new ServiceResult<SyncIssue>(new SyncIssue { Id = person.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = typeof(PersonEntity).ToAssemblyName() })
				);

			client.Setup(x => x.ApplyCorrections(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) =>
				{
					corrections.AddRange(x.Collection);
					return new ServiceResult<SyncIssue>();
				});

			var engine = new SyncEngine(client.Object, server, options);
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
					database.Addresses.Add(NewAddress("Work"));
					database.SaveChanges();
				}

				using (var database = client.GetDatabase<IContosoDatabase>())
				{
					var address = NewAddress("Home");
					database.Addresses.Add(address);
					database.People.Add(NewPerson("John Doe", address));
					database.SaveChanges();
				}

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());

					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
				}

				var options = new SyncOptions();
				options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());

				var engine = new SyncEngine(client, server, options);
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count);

				// We should have synced only the address but not the people!

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());

					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
				}
			}, false);
		}

		[TestMethod]
		public void ServerLimitRepository()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				PersonEntity person;

				using (var database = server.GetDatabase<IContosoDatabase>())
				{
					var address = NewAddress("Work");
					database.Addresses.Add(address);
					person = NewPerson("Jane Doe", address);
					database.People.Add(person);
					database.SaveChanges();
				}

				using (var database = client.GetDatabase<IContosoDatabase>())
				{
					var address = NewAddress("Home");
					database.Addresses.Add(address);
					database.People.Add(NewPerson("John Doe", address));
					database.SaveChanges();
				}

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());

					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
				}

				using (var listener = new LogListener(Guid.NewGuid()))
				{
					listener.OutputToConsole = true;

					var options = new SyncOptions();
					options.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					options.AddSyncableFilter(new SyncRepositoryFilter<PersonEntity>(x => x.SyncId == person.SyncId));

					var engine = new SyncEngine(listener.SessionId, client, server, options);
					engine.Run();

					Assert.AreEqual(0, engine.SyncIssues.Count);
				}

				// We should have synced only the address but not the people!

				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				{
					// Client should have all
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.People.Count());

					// Server should only have the address
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
				}
			}, false);
		}

		[TestMethod]
		public void ServerShouldNotPushNonModifiedEntities()
		{
			var server = new TestSyncClient("Server (Test)");
			var client = new SyncClient("Client (MEM)", TestHelper.GetSyncableMemoryProvider());

			var address = NewAddress("123 Elm Street");
			server.Changes.Add(new ServiceResult<SyncObject>(address.ToSyncObject()));

			var engine = new SyncEngine(client, server, new SyncOptions());
			engine.Run();

			Assert.AreEqual(0, server.AppliedChanges.Count);
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
			var client1 = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider());
			var client2 = new SyncClient("Client 2", TestHelper.GetSyncableMemoryProvider());
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider());
			server.Options.MaintainModifiedOn = true;

			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 00);
			var address = NewAddress("123 Elm Street");
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

			// Make sure all data is only on the server
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, serverDatabase.Addresses.Count());
				Assert.AreEqual("123 Elm Street", serverDatabase.Addresses.First().Line1);
				Assert.AreEqual(0, client1Database.Addresses.Count());
				Assert.AreEqual(0, client2Database.Addresses.Count());
			}

			var client1Options = new SyncOptions();
			var client2Options = new SyncOptions();

			// Sync Set 1: Go ahead and sync the data to all locations
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 01);
			SyncEngine.Run(client1, server, client1Options);
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 02);
			SyncEngine.Run(client2, server, client2Options);

			// Prepare a new address for client 2
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 03);
			var address2 = NewAddress("123 Main Street");

			// Make sure all data is there
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, client1Database.Addresses.Count());
				Assert.AreEqual(1, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));

				// Add another address to client 2
				TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 11, 59, 04);
				client2Database.Addresses.Add(address2);
				client2Database.SaveChanges();
			}

			var client1Id = Guid.NewGuid();
			var client2Id = Guid.NewGuid();

			// Manual Sync

			// Do first part of syncing client 1 (client1 <- server)
			// The should not have any updates as the server has not changed
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 00);
			var client1Start = TimeService.UtcNow;
			var serverSession = server.BeginSync(client1Id, client1Options);
			client1.BeginSync(client1Id, client1Options);
			var client1Request = new SyncRequest { Since = client1Options.LastSyncedOnServer, Until = client1Start, Skip = 0 };
			var client1Results = server.GetChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Results.TotalCount);
			Assert.AreEqual(0, client1Results.Collection.Count);
			client1Request.Collection = client1Results.Collection;
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 00, 01);
			var client1Issues = client1.ApplyChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Issues.TotalCount);
			Assert.AreEqual(0, client1Issues.Collection.Count);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, client1Database.Addresses.Count());
				Assert.AreEqual(2, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));
			}

			// Do first part of syncing client 2 (client2 <- server)
			// This should not have any updates as the server has not changed
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 01, 00);
			var serverStart = TimeService.UtcNow;
			var client2Start = serverStart;
			serverSession = server.BeginSync(client2Id, client2Options);
			client2.BeginSync(client2Id, client2Options);
			var client2Request = new SyncRequest { Since = client2Options.LastSyncedOnServer, Until = serverStart, Skip = 0 };
			var client2Results = server.GetChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Results.TotalCount);
			Assert.AreEqual(0, client2Results.Collection.Count);
			client2Request.Collection = client2Results.Collection;
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 01, 01);
			var client2Issues = client2.ApplyChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Issues.TotalCount);
			Assert.AreEqual(0, client2Issues.Collection.Count);

			// Data should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(1, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, client1Database.Addresses.Count());
				Assert.AreEqual(2, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));
			}

			// Do second part of client 2 (client2 -> server)
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 02, 00);
			client2Request = new SyncRequest { Since = client2Options.LastSyncedOnClient, Until = client2Start, Skip = 0 };
			client2Results = client2.GetChanges(client2Id, client2Request);
			Assert.AreEqual(1, client2Results.TotalCount);
			Assert.AreEqual(1, client2Results.Collection.Count);
			Assert.AreEqual(address2.SyncId, client2Results.Collection[0].SyncId);
			client2Request.Collection = client2Results.Collection;
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 02, 01);
			client2Issues = server.ApplyChanges(client2Id, client2Request);
			Assert.AreEqual(0, client2Issues.TotalCount);
			Assert.AreEqual(0, client2Issues.Collection.Count);
			client2.EndSync(serverSession);
			server.EndSync(serverSession);

			// Data still should now have synced to the server
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(2, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, client1Database.Addresses.Count());
				Assert.AreEqual(2, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));
			}

			// Do second part of client 1 (client1 -> server)
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 03, 00);
			client1Request = new SyncRequest { Since = client1Options.LastSyncedOnClient, Until = client1Start, Skip = 0 };
			client1Results = client1.GetChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Results.TotalCount);
			Assert.AreEqual(0, client1Results.Collection.Count);
			client1Request.Collection = client1Results.Collection;
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 03, 01);
			client1Issues = server.ApplyChanges(client1Id, client1Request);
			Assert.AreEqual(0, client1Issues.TotalCount);
			Assert.AreEqual(0, client1Issues.Collection.Count);
			client1.EndSync(serverSession);
			server.EndSync(serverSession);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(2, serverDatabase.Addresses.Count());
				Assert.AreEqual(1, client1Database.Addresses.Count());
				Assert.AreEqual(2, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));
			}

			// Sync Set 2: Go ahead and sync the data to all locations
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 04, 00);
			client1Options.LastSyncedOnServer = serverStart;
			client1Options.LastSyncedOnClient = client1Start;
			SyncEngine.Run(client1, server, client1Options);
			Assert.IsFalse(client1.Statistics.IsReset);
			Assert.IsFalse(server.Statistics.IsReset);
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 04, 01);
			client2Options.LastSyncedOnServer = serverStart;
			client2Options.LastSyncedOnClient = client2Start;
			SyncEngine.Run(client2, server, client2Options);
			Assert.IsFalse(client2.Statistics.IsReset);
			Assert.IsFalse(server.Statistics.IsReset);

			// Data still should not have changed yet
			using (var client1Database = client1.GetDatabase<IContosoDatabase>())
			using (var client2Database = client2.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				Assert.AreEqual(2, serverDatabase.Addresses.Count());
				Assert.AreEqual(2, client1Database.Addresses.Count());
				Assert.AreEqual(2, client2Database.Addresses.Count());

				var serverAddress = serverDatabase.Addresses.First();
				var client1Address = client1Database.Addresses.First();
				var client2Address = client2Database.Addresses.First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));

				serverAddress = serverDatabase.Addresses.Skip(1).First();
				client1Address = client1Database.Addresses.Skip(1).First();
				client2Address = client2Database.Addresses.Skip(1).First();
				TestHelper.AreEqual(serverAddress.Unwrap(), client1Address.Unwrap(), nameof(Address.ModifiedOn));
				TestHelper.AreEqual(serverAddress.Unwrap(), client2Address.Unwrap(), nameof(Address.ModifiedOn));
			}

			// Sync Set 3
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 05, 00);
			client1Options.LastSyncedOnServer = client1Options.LastSyncedOnServer;
			client1Options.LastSyncedOnClient = client1Options.LastSyncedOnClient;
			SyncEngine.Run(client1, server, client1Options);
			Assert.IsTrue(client1.Statistics.IsReset);
			Assert.IsTrue(server.Statistics.IsReset);
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 10, 12, 05, 01);
			client2Options.LastSyncedOnServer = client2Options.LastSyncedOnServer;
			client2Options.LastSyncedOnClient = client2Options.LastSyncedOnClient;
			SyncEngine.Run(client2, server, client2Options);
			Assert.IsTrue(client2.Statistics.IsReset);
			Assert.IsTrue(server.Statistics.IsReset);
		}

		[TestMethod]
		public void UpdateItemOnClientThenServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				address = address.Unwrap<AddressEntity>();
				address.Id = 0;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());

					clientDatabase.Addresses.First().Line1 = "Foo Client";
					clientDatabase.SaveChanges();

					serverDatabase.Addresses.First().Line1 = "Foo Server";
					serverDatabase.Addresses.First().Line2 = "Foo Server2";
					serverDatabase.SaveChanges();
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual("Foo Server", clientDatabase.Addresses.First().Line1);
					Assert.AreEqual("Foo Server2", clientDatabase.Addresses.First().Line2);
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual("Foo Server", serverDatabase.Addresses.First().Line1);
					Assert.AreEqual("Foo Server2", serverDatabase.Addresses.First().Line2);
				}
			});
		}

		[TestMethod]
		public void UpdateItemOnServerThenClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				Assert.AreNotEqual(server, client);

				var address1 = NewAddress("123 Elm Street");
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address1);

				var address2 = (AddressEntity) address1.Unwrap();
				address2.Id = 0;
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(address2);

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.IsFalse(ReferenceEquals(clientDatabase, serverDatabase));

					Assert.AreNotEqual(clientDatabase, serverDatabase);
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());

					serverDatabase.Addresses.First().Line1 = "123 Server Street";
					serverDatabase.Addresses.First().Line2 = "Server2";
					serverDatabase.SaveChanges();

					clientDatabase.Addresses.First().Line1 = "123 Client Street";
					clientDatabase.SaveChanges();
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", clientDatabase.Addresses.First().Line1);
					Assert.AreEqual(address1.Line2, clientDatabase.Addresses.First().Line2);
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", serverDatabase.Addresses.First().Line1);
					Assert.AreEqual(address1.Line2, serverDatabase.Addresses.First().Line2);
				}
			});
		}

		[TestMethod]
		public void UseItemOnClientAndServerDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Bar"));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				AddressEntity removedAddress;
				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual("Foo", serverDatabase.People.Including(x => x.Address).First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());

					var person = clientDatabase.People.First();
					person.Address = clientDatabase.Addresses.First(x => x.Line1 == "Bar");
					clientDatabase.SaveChanges();

					removedAddress = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.Addresses.Remove(removedAddress);
					serverDatabase.SaveChanges();
				}

				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					var serverPerson = serverDatabase.People.Including(x => x.Address).First();
					Assert.AreEqual("Bar", serverPerson.Address.Line1);
					Assert.AreEqual(removedAddress.SyncId, serverPerson.AddressSyncId);
					Assert.AreEqual(removedAddress.SyncId, serverPerson.Address.SyncId);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
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
				client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<PersonEntity, int>(new PersonEntity { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(NewAddress("Bar"));

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual("Foo", serverDatabase.People.Include(x => x.Address).First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());

					serverDatabase.People.First().Address = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.SaveChanges();
					var removedAddress = clientDatabase.Addresses.First(x => x.Line1 == "Bar");
					clientDatabase.Addresses.Remove(removedAddress);
					clientDatabase.SaveChanges();
				}

				engine.Run();

				using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
				using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
				{
					Assert.AreEqual("Bar", serverDatabase.People.Include(x => x.Address).First().Address.Line1);
					Assert.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					Assert.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
				}
			});
		}

		private void CompleteTestSync(ISyncClient client, ISyncClient server, SyncOptions options)
		{
			// Catch up sync
			SyncEngine.Run(client, server, options);
			Assert.IsFalse(server.Statistics.IsReset);
			Assert.IsFalse(client.Statistics.IsReset);

			// Final sync
			SyncEngine.Run(client, server, options);
			Assert.IsTrue(server.Statistics.IsReset);
			Assert.IsTrue(client.Statistics.IsReset);
		}

		private static AddressEntity NewAddress(string line1, string line2 = null)
		{
			var time = TimeService.UtcNow;
			return new AddressEntity
			{
				Line1 = line1,
				Line2 = line2 ?? Guid.NewGuid().ToString(),
				City = Guid.NewGuid().ToString(),
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString(),
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		private static PersonEntity NewPerson(string name, AddressEntity address)
		{
			var time = TimeService.UtcNow;
			return new PersonEntity
			{
				Address = address,
				Name = name,
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		private static SettingEntity NewSetting(string name, string value)
		{
			var time = TimeService.UtcNow;
			return new SettingEntity
			{
				Name = name,
				Value = value,
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		#endregion
	}
}