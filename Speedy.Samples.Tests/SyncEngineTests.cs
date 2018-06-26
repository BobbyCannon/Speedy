#region References

using System;
using System.Diagnostics.Tracing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Logging;
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

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			TestHelper.Initialize();
		}

		[TestMethod]
		public void SyncEngineAddItemToClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					TestHelper.AreEqual(clientDatabase.Addresses.First().Unwrap(), serverDatabase.Addresses.First().Unwrap());
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(NewAddress("Foo"));
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Bar"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					var addresses1 = clientDatabase.Addresses.ToList();
					var addresses2 = serverDatabase.Addresses.ToList();

					Assert.AreEqual(2, addresses1.Count);
					Assert.AreEqual(2, addresses2.Count);

					TestHelper.AreEqual(addresses1[0].Unwrap(), addresses2[1].Unwrap(), "Id");
					TestHelper.AreEqual(addresses1[1].Unwrap(), addresses2[0].Unwrap(), "Id");
				}
			});
		}

		/// <summary>
		/// This test will force both sides to have a local address with ID of 1 then sync.
		/// This means the engine should sync over the client address as 2 and update the
		/// relationship to point to ID 2 instead of the old local relationship of 1.
		/// </summary>
		[TestMethod]
		public void SyncEngineAddItemToClientAndServerForceRelationshipUpdate()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Bar"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					var clientAddresses = clientDatabase.Addresses.ToList();
					var serverAddresses = serverDatabase.Addresses.ToList();

					Assert.AreEqual(2, serverDatabase.People.First().Address.Id);
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());

					TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), nameof(Address.Id));
					TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), nameof(Address.Id));
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToClientWithLogger()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					TestHelper.AreEqual(clientDatabase.Addresses.First().Unwrap(), serverDatabase.Addresses.First().Unwrap());
				}

				//Assert.AreEqual(9, logger.Messages.Count);
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemWithRelationshipToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					var clientAddresses = clientDatabase.Addresses.ToList();
					var clientPeople = clientDatabase.People.ToList();
					var serverAddresses = serverDatabase.Addresses.ToList();
					var serverPeople = serverDatabase.People.ToList();

					Assert.AreEqual(2, clientAddresses.Count);
					Assert.AreEqual(2, clientPeople.Count);
					Assert.AreEqual(2, serverAddresses.Count);
					Assert.AreEqual(2, serverPeople.Count);

					TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id");
					TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id");
					TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(Person.AddressId));
					TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(Person.AddressId));

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
		public void SyncEngineAddItemWithRelationshipToClientAndServerWithLogger()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				using (var listener = new LogListener(engine.SessionId))
				{
					engine.Run();

					Assert.AreEqual(9, listener.Events.Count);

					using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
					using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
					{
						var clientAddresses = clientDatabase.Addresses.ToList();
						var clientPeople = clientDatabase.People.ToList();
						var serverAddresses = serverDatabase.Addresses.ToList();
						var serverPeople = serverDatabase.People.ToList();

						Assert.AreEqual(2, clientAddresses.Count);
						Assert.AreEqual(2, clientPeople.Count);
						Assert.AreEqual(2, serverAddresses.Count);
						Assert.AreEqual(2, serverPeople.Count);

						TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id");
						TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id");
						TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(Person.AddressId));
						TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(Person.AddressId));

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
		public void SyncEngineAddItemWithRelationshipToClientAndServerWithLoggerOfVerbose()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Bar"), Name = "Bar" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				using (var listener = new LogListener(engine.SessionId, EventLevel.Verbose))
				{
					engine.Run();

					var expected = client.Name.Contains("WEB") ? 13 : server.Name.Contains("WEB") ? 11 : 15;
					Assert.AreEqual(expected, listener.Events.Count);

					using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
					using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
					{
						var clientAddresses = clientDatabase.Addresses.ToList();
						var clientPeople = clientDatabase.People.ToList();
						var serverAddresses = serverDatabase.Addresses.ToList();
						var serverPeople = serverDatabase.People.ToList();

						Assert.AreEqual(2, clientAddresses.Count);
						Assert.AreEqual(2, clientPeople.Count);
						Assert.AreEqual(2, serverAddresses.Count);
						Assert.AreEqual(2, serverPeople.Count);

						TestHelper.AreEqual(clientAddresses[0].Unwrap(), serverAddresses[1].Unwrap(), "Id");
						TestHelper.AreEqual(clientAddresses[1].Unwrap(), serverAddresses[0].Unwrap(), "Id");
						TestHelper.AreEqual(clientPeople[0].Unwrap(), serverPeople[1].Unwrap(), "Id", nameof(Person.AddressId));
						TestHelper.AreEqual(clientPeople[1].Unwrap(), serverPeople[0].Unwrap(), "Id", nameof(Person.AddressId));

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
		public void SyncEngineAddItemWithServerAndClientWithSameItem()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var person = new Person { Address = NewAddress("Foo"), Name = "Foo Bar" };
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(person);

				person.Id = 0;
				person.Address.Id = 0;
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(person);

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
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

		[TestMethod]
		public void SyncEngineDeleteItemOnClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(address);

				address.Id = 0;
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(address);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());

					clientDatabase.Addresses.Remove(x => x.SyncId == address.SyncId);
					clientDatabase.SaveChanges();

					Assert.AreEqual(0, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(0, clientDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(address);

				address.Id = 0;
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(address);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());

					serverDatabase.Addresses.Remove(x => x.Id > 0);
					serverDatabase.SaveChanges();

					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.Addresses.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(0, clientDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineShouldUpdateClientSessionId()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.SessionId.Dump();
				client.SessionId.Dump();

				var engine = new SyncEngine(client, server, new SyncOptions());

				engine.SessionId.Dump();
				server.SessionId.Dump();
				client.SessionId.Dump();

				Assert.AreEqual(engine.SessionId, client.SessionId);
				Assert.AreEqual(engine.SessionId, server.SessionId);
			});
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnClientThenServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(address);

				address.Id = 0;
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(address);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
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

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
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
		public void SyncEngineUpdateItemOnServerThenClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(address);

				address.Id = 0;
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(address);

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
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

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", clientDatabase.Addresses.First().Line1);
					Assert.AreEqual(address.Line2, clientDatabase.Addresses.First().Line2);
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", serverDatabase.Addresses.First().Line1);
					Assert.AreEqual(address.Line2, serverDatabase.Addresses.First().Line2);
				}
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnClientAndServerDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Bar"));

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
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

					var removedAddress = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.Addresses.Remove(removedAddress);
					serverDatabase.SaveChanges();
				}

				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual("Foo", serverDatabase.People.Including(x => x.Address).First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(0, clientDatabase.SyncTombstones.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
					Assert.AreEqual(0, serverDatabase.SyncTombstones.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnServerAndClientDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				((IContosoDatabase) client.GetDatabase()).AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				((IContosoDatabase) server.GetDatabase()).AddAndSaveChanges(NewAddress("Bar"));

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}
				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
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

				using (var clientDatabase = (IContosoDatabase) client.GetDatabase())
				using (var serverDatabase = (IContosoDatabase) server.GetDatabase())
				{
					Assert.AreEqual("Bar", serverDatabase.People.Include(x => x.Address).First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(0, clientDatabase.SyncTombstones.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
					Assert.AreEqual(0, serverDatabase.SyncTombstones.Count());
				}
			});
		}

		private static Address NewAddress(string line1, string line2 = null)
		{
			return new Address
			{
				Line1 = line1,
				Line2 = line2 ?? Guid.NewGuid().ToString(),
				City = Guid.NewGuid().ToString(),
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString()
			};
		}

		#endregion
	}
}