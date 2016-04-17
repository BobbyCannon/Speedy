#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy.Tests.Mocks;

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
			var client = new MockSyncClient();
			var server = new MockSyncServer();

			client.Addresses.Add(NewAddress("Blah"));
			client.SaveChanges();

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client.Addresses.Count());
			Assert.AreEqual(1, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineAddItemToClientAndServer()
		{
			var client = new MockSyncClient();
			var server = new MockSyncServer();

			client.Addresses.Add(NewAddress("Foo"));
			client.SaveChanges();

			server.Addresses.Add(NewAddress("Bar"));
			server.SaveChanges();

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(2, client.Addresses.Count());
			Assert.AreEqual(2, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineAddItemToClientThenSyncAnotherClient()
		{
			var client1 = new MockSyncClient();
			var client2 = new MockSyncClient();
			var server = new MockSyncServer();

			client1.Addresses.Add(NewAddress("Blah"));
			client1.SaveChanges();

			SyncEngine.PullAndPushChanges(client1, server);

			client1.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client1.Addresses.Count());
			Assert.AreEqual(1, server.Addresses.Count());

			SyncEngine.PullAndPushChanges(client2, server);

			client2.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client2.Addresses.Count());
			Assert.AreEqual(1, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineAddItemToServer()
		{
			var client = new MockSyncClient();
			var server = new MockSyncServer();

			server.Addresses.Add(NewAddress("Blah"));
			server.SaveChanges();

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client.Addresses.Count());
			Assert.AreEqual(1, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineAddItemToServerThenSyncToTwoClients()
		{
			var client1 = new MockSyncClient();
			var client2 = new MockSyncClient();
			var server = new MockSyncServer();

			server.Addresses.Add(NewAddress("Blah", "Blah2"));
			server.SaveChanges();

			SyncEngine.PullAndPushChanges(client1, server);

			client1.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client1.Addresses.Count());
			Assert.AreEqual("Blah", client1.Addresses.First().Line1);
			Assert.AreEqual("Blah2", client1.Addresses.First().Line2);
			Assert.AreEqual(1, server.Addresses.Count());
			Assert.AreEqual("Blah", server.Addresses.First().Line1);
			Assert.AreEqual("Blah2", server.Addresses.First().Line2);

			SyncEngine.PullAndPushChanges(client2, server);

			client2.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client2.Addresses.Count());
			Assert.AreEqual("Blah", client2.Addresses.First().Line1);
			Assert.AreEqual("Blah2", client2.Addresses.First().Line2);
			Assert.AreEqual(1, server.Addresses.Count());
			Assert.AreEqual("Blah", server.Addresses.First().Line1);
			Assert.AreEqual("Blah2", server.Addresses.First().Line2);
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnClient()
		{
			var client = MockSyncClient.Create(NewAddress("Foo", "Foo2"));
			var server = MockSyncServer.Create(client.Addresses.First());

			client.Addresses.RemoveRange(x => x.Id > 0);
			client.SaveChanges();

			Assert.AreEqual(0, client.Addresses.Count());
			Assert.AreEqual(1, server.Addresses.Count());

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(0, client.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnServer()
		{
			var client = MockSyncClient.Create(NewAddress("Foo", "Foo2"));
			var server = MockSyncServer.Create(client.Addresses.First());

			server.Addresses.RemoveRange(x => x.Id > 0);
			server.SaveChanges();

			Assert.AreEqual(1, client.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(0, client.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineDeleteItemOnServerAndThenToTwoClients()
		{
			var client1 = MockSyncClient.Create(NewAddress("Foo", "Foo2"));
			var client2 = MockSyncClient.Create(client1.Addresses.First());
			var server = MockSyncServer.Create(client1.Addresses.First());

			server.Addresses.RemoveRange(x => x.Id > 0);
			server.SaveChanges();

			Assert.AreEqual(1, client1.Addresses.Count());
			Assert.AreEqual(1, client2.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());

			SyncEngine.PullAndPushChanges(client1, server);

			client1.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(0, client1.Addresses.Count());
			Assert.AreEqual(1, client2.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());

			SyncEngine.PullAndPushChanges(client2, server);

			client2.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(0, client1.Addresses.Count());
			Assert.AreEqual(0, client2.Addresses.Count());
			Assert.AreEqual(0, server.Addresses.Count());
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnClientThenServer()
		{
			var client = MockSyncClient.Create(NewAddress("Foo", "Foo2"));
			var server = MockSyncServer.Create(client.Addresses.First());

			client.Addresses.First().Line1 = "Foo Client";
			client.SaveChanges();

			server.Addresses.First().Line1 = "Foo Server";
			server.Addresses.First().Line2 = "Foo Server2";
			server.SaveChanges();

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client.Addresses.Count());
			Assert.AreEqual("Foo Server", client.Addresses.First().Line1);
			Assert.AreEqual("Foo Server2", client.Addresses.First().Line2);
			Assert.AreEqual(1, server.Addresses.Count());
			Assert.AreEqual("Foo Server", server.Addresses.First().Line1);
			Assert.AreEqual("Foo Server2", server.Addresses.First().Line2);
		}

		[TestMethod]
		public void SyncEngineUpdateItemOnServerThenClient()
		{
			var client = MockSyncClient.Create(NewAddress("Foo", "Foo2"));
			var server = MockSyncServer.Create(client.Addresses.First());

			server.Addresses.First().Line1 = "Foo Server";
			server.Addresses.First().Line2 = "Foo Server2";
			server.SaveChanges();

			client.Addresses.First().Line1 = "Foo Client";
			client.SaveChanges();

			SyncEngine.PullAndPushChanges(client, server);

			client.SaveChanges();
			server.SaveChanges();

			Assert.AreEqual(1, client.Addresses.Count());
			Assert.AreEqual("Foo Client", client.Addresses.First().Line1);
			Assert.AreEqual("Foo2", client.Addresses.First().Line2);
			Assert.AreEqual(1, server.Addresses.Count());
			Assert.AreEqual("Foo Client", server.Addresses.First().Line1);
			Assert.AreEqual("Foo2", server.Addresses.First().Line2);
		}

		private Address NewAddress(string line1, string line2 = "")
		{
			return new Address { Line1 = line1, Line2 = line2, City = "", Postal = "", State = "" };
		}

		#endregion
	}
}