#region References

using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Tests
{
	/// <summary>
	/// Summary description for SyncEngineTests.
	/// </summary>
	[TestClass]
	public class SyncEngineTests
	{
		#region Methods

		[TestMethod]
		public void SyncEngineAddItemToClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase().AddAndSaveChanges(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				Assert.AreEqual(0, engine.SyncIssues.Count);

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase().AddAndSaveChanges(NewAddress("Foo"));
				server.GetDatabase().AddAndSaveChanges(NewAddress("Bar"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
				}
			});
		}

		/// <summary>
		/// This test will force both sides to have a local address with ID of 1 then sync.
		/// This means the engine should sync over the client address as 2 and update the
		/// relationship to point to ID 2 instead of the old local relatioship of 1.
		/// </summary>
		[TestMethod]
		public void SyncEngineAddItemToClientAndServerForceRelationshipUpdate()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase().AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase().AddAndSaveChanges(NewAddress("Bar"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(2, serverDatabase.People.First().Address.Id);
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.GetDatabase().AddAndSaveChanges(NewAddress("Blah"));

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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
				client.GetDatabase().AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase().AddAndSaveChanges(new Person { Address = NewAddress("Bar"), Name = "Bar Foo" });

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(2, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(2, serverDatabase.People.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineAddItemWithServerAndClientWithSameItem()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var person = new Person { Address = NewAddress("Foo"), Name = "Foo Bar" };

				client.GetDatabase().AddAndSaveChanges(person);
				server.GetDatabase().AddAndSaveChanges(person);

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.GetDatabase().AddAndSaveChanges(address);
				client.GetDatabase().AddAndSaveChanges(address);

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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
				server.GetDatabase().AddAndSaveChanges(address);
				client.GetDatabase().AddAndSaveChanges(address);

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(0, clientDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.Addresses.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnClientThenServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.GetDatabase().AddAndSaveChanges(address);
				client.GetDatabase().AddAndSaveChanges(address);

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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
				server.GetDatabase().AddAndSaveChanges(address);
				client.GetDatabase().AddAndSaveChanges(address);

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
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

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", clientDatabase.Addresses.First().Line1);
					Assert.AreEqual("", clientDatabase.Addresses.First().Line2);
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual("123 Client Street", serverDatabase.Addresses.First().Line1);
					Assert.AreEqual("", serverDatabase.Addresses.First().Line2);
				}
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnClientAndServerDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase().AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase().AddAndSaveChanges(NewAddress("Bar"));

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());

					var person = client.GetDatabase().People.First();
					person.Address = clientDatabase.Addresses.First(x => x.Line1 == "Bar");
					clientDatabase.SaveChanges();

					var removedAddress = serverDatabase.Addresses.First(x => x.Line1 == "Bar");
					serverDatabase.Addresses.Remove(removedAddress);
					serverDatabase.SaveChanges();
				}
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
				}
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnServerAndClientDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.GetDatabase().AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.GetDatabase().AddAndSaveChanges(NewAddress("Bar"));

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual(1, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(1, serverDatabase.Addresses.Count());
					Assert.AreEqual(0, serverDatabase.People.Count());
				}
				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.Run();

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual("Foo", serverDatabase.People.First().Address.Line1);
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

				using (var clientDatabase = client.GetDatabase())
				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual("Bar", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), clientDatabase.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), clientDatabase.People.Count());
					Assert.AreEqual(2, clientDatabase.Addresses.Count());
					Assert.AreEqual(1, clientDatabase.People.Count());
					Assert.AreEqual(2, serverDatabase.Addresses.Count());
					Assert.AreEqual(1, serverDatabase.People.Count());
				}
			});
		}

		private static Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		#endregion
	}
}