#region References

using System;
using System.Linq;
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
				server.Database.AddAndSaveChanges(NewAddress("Blah"));
				client.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.Database.AddAndSaveChanges(NewAddress("Foo"));
				client.SaveChanges();

				server.Database.Addresses.Add(NewAddress("Bar"));
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(2, client.Database.Addresses.Count());
				Assert.AreEqual(2, server.Database.Addresses.Count());
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
				client.Database.People.Add(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				client.SaveChanges();

				server.Database.Addresses.Add(NewAddress("Bar"));
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(2, server.Database.People.First().Address.Id);
				Assert.AreEqual("Foo", server.Database.People.First().Address.Line1);
				TestHelper.AreEqual(server.Database.Addresses.Count(), client.Database.Addresses.Count());
				TestHelper.AreEqual(server.Database.People.Count(), client.Database.People.Count());
			});
		}

		[TestMethod]
		public void SyncEngineAddItemToServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				server.Database.Addresses.Add(NewAddress("Blah"));
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
			});
		}

		[TestMethod]
		public void SyncEngineAddItemWithRelationshipToClientAndServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.Database.People.Add(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				client.SaveChanges();

				server.Database.People.Add(new Person { Address = NewAddress("Bar"), Name = "Bar Foo" });
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(2, client.Database.Addresses.Count());
				Assert.AreEqual(2, client.Database.People.Count());
				Assert.AreEqual(2, server.Database.Addresses.Count());
				Assert.AreEqual(2, server.Database.People.Count());
			});
		}

		[TestMethod]
		public void SyncEngineAddItemWithServerAndClientWithSameItem()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var person = new Person { Address = NewAddress("Foo"), Name = "Foo Bar" };

				client.Database.People.Add(person);
				client.SaveChanges();

				server.Database.People.Add(person);
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.People.Count());
			});
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.Database.AddAndSaveChanges(address);
				client.Database.AddAndSaveChanges(address);

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());

				client.Database.Addresses.Remove(address);
				client.SaveChanges();

				Assert.AreEqual(0, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(0, client.Database.Addresses.Count());
				Assert.AreEqual(0, server.Database.Addresses.Count());
			});
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.Database.AddAndSaveChanges(address);
				client.Database.AddAndSaveChanges(address);

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());

				server.Database.Addresses.Remove(x => x.Id > 0);
				server.SaveChanges();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(0, server.Database.Addresses.Count());

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(0, client.Database.Addresses.Count());
				Assert.AreEqual(0, server.Database.Addresses.Count());
			});
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnClientThenServer()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.Database.AddAndSaveChanges(address);
				client.Database.AddAndSaveChanges(address);

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());

				client.Database.Addresses.First().Line1 = "Foo Client";
				client.SaveChanges();

				server.Database.Addresses.First().Line1 = "Foo Server";
				server.Database.Addresses.First().Line2 = "Foo Server2";
				server.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual("Foo Server", client.Database.Addresses.First().Line1);
				Assert.AreEqual("Foo Server2", client.Database.Addresses.First().Line2);
				Assert.AreEqual(1, server.Database.Addresses.Count());
				Assert.AreEqual("Foo Server", server.Database.Addresses.First().Line1);
				Assert.AreEqual("Foo Server2", server.Database.Addresses.First().Line2);
			});
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnServerThenClient()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				var address = NewAddress("123 Elm Street");
				server.Database.AddAndSaveChanges(address);
				client.Database.AddAndSaveChanges(address);

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
				
				server.Database.Addresses.First().Line1 = "123 Server Street";
				server.Database.Addresses.First().Line2 = "Server2";
				server.SaveChanges();

				client.Database.Addresses.First().Line1 = "123 Client Street";
				client.SaveChanges();

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual("123 Client Street", client.Database.Addresses.First().Line1);
				Assert.AreEqual("Server2", client.Database.Addresses.First().Line2);
				Assert.AreEqual(1, server.Database.Addresses.Count());
				Assert.AreEqual("123 Client Street", server.Database.Addresses.First().Line1);
				Assert.AreEqual("Server2", server.Database.Addresses.First().Line2);
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnClientAndServerDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.Database.AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.Database.AddAndSaveChanges(NewAddress("Bar"));

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
				Assert.AreEqual(0, server.Database.People.Count());

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual("Foo", server.Database.People.First().Address.Line1);
				TestHelper.AreEqual(server.Database.Addresses.Count(), client.Database.Addresses.Count());
				TestHelper.AreEqual(server.Database.People.Count(), client.Database.People.Count());
				Assert.AreEqual(2, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(2, server.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.People.Count());

				var person = client.Database.People.First();
				person.Address = client.Database.Addresses.First(x => x.Line1 == "Bar");
				client.SaveChanges();

				var removedAddress = server.Database.Addresses.First(x => x.Line1 == "Bar");
				server.Database.Addresses.Remove(removedAddress);
				server.SaveChanges();

				engine.Run();

				Assert.AreEqual("Foo", server.Database.People.First().Address.Line1);
				TestHelper.AreEqual(server.Database.Addresses.Count(), client.Database.Addresses.Count());
				TestHelper.AreEqual(server.Database.People.Count(), client.Database.People.Count());
				Assert.AreEqual(2, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(2, server.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.People.Count());
			});
		}

		[TestMethod]
		public void SyncEngineUseItemOnServerAndClientDeletesIt()
		{
			TestHelper.TestServerAndClients((server, client) =>
			{
				client.Database.AddAndSaveChanges(new Person { Address = NewAddress("Foo"), Name = "Foo Bar" });
				server.Database.AddAndSaveChanges(NewAddress("Bar"));

				Assert.AreEqual(1, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(1, server.Database.Addresses.Count());
				Assert.AreEqual(0, server.Database.People.Count());

				var engine = new SyncEngine(client, server, DateTime.MinValue);
				engine.Run();

				Assert.AreEqual("Foo", server.Database.People.First().Address.Line1);
				TestHelper.AreEqual(server.Database.Addresses.Count(), client.Database.Addresses.Count());
				TestHelper.AreEqual(server.Database.People.Count(), client.Database.People.Count());
				Assert.AreEqual(2, client.Database.Addresses.Count());
				Assert.AreEqual(1, client.Database.People.Count());
				Assert.AreEqual(2, server.Database.Addresses.Count());
				Assert.AreEqual(1, server.Database.People.Count());

				server.Database.People.First().Address = server.Database.Addresses.First(x => x.Line1 == "Bar");
				server.SaveChanges();

				var removedAddress = client.Database.Addresses.First(x => x.Line1 == "Bar");
				client.Database.Addresses.Remove(removedAddress);
				client.SaveChanges();

				engine.Run();

				using (var serverDatabase = server.GetDatabase())
				{
					Assert.AreEqual("Bar", serverDatabase.People.First().Address.Line1);
					TestHelper.AreEqual(serverDatabase.Addresses.Count(), client.Database.Addresses.Count());
					TestHelper.AreEqual(serverDatabase.People.Count(), client.Database.People.Count());
					Assert.AreEqual(2, client.Database.Addresses.Count());
					Assert.AreEqual(1, client.Database.People.Count());
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