#region References

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncEngineTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void PullDownShouldNotPushClientContent()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(initialize: false));

			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress());

			var options = new SyncOptions { SyncDirection = SyncDirection.PullDown };
			using var engine = SyncEngine.Run(client, server, options);
			var issues = engine.SyncIssues;
			Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

			using var clientDatabase = client.GetDatabase<IContosoDatabase>();
			using var serverDatabase = server.GetDatabase<IContosoDatabase>();

			var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
			var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
			Assert.AreEqual(1, addresses1.Count);
			Assert.AreEqual(0, addresses2.Count);
		}

		[TestMethod]
		public void PullDownShouldSuccessButShouldNotPushClientContent()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(initialize: false));

			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress(line1: "Hello World"));
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress(line1: "Foo Bar"));

			var options = new SyncOptions { SyncDirection = SyncDirection.PullDown };
			using var engine = SyncEngine.Run(client, server, options);
			var issues = engine.SyncIssues;
			Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

			using var clientDatabase = client.GetDatabase<IContosoDatabase>();
			using var serverDatabase = server.GetDatabase<IContosoDatabase>();

			var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
			var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
			Assert.AreEqual(2, addresses1.Count);
			Assert.AreEqual(1, addresses2.Count);

			Assert.AreNotEqual(addresses1[0].Line1, addresses2[0].Line1);
			Assert.AreEqual(addresses1[1].Line1, addresses2[0].Line1);
		}

		[TestMethod]
		public void PushUpShouldNotPullServerContent()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(initialize: false));

			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress());

			var options = new SyncOptions { SyncDirection = SyncDirection.PushUp };
			using var engine = SyncEngine.Run(client, server, options);
			var issues = engine.SyncIssues;
			Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

			using var clientDatabase = client.GetDatabase<IContosoDatabase>();
			using var serverDatabase = server.GetDatabase<IContosoDatabase>();

			var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
			var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
			Assert.AreEqual(0, addresses1.Count);
			Assert.AreEqual(1, addresses2.Count);
		}

		[TestMethod]
		public void PushUpShouldSuccessButShouldNotPullServerContent()
		{
			var client = new SyncClient("Client", TestHelper.GetSyncableMemoryProvider(initialize: false));
			var server = new SyncClient("Server", TestHelper.GetSyncableMemoryProvider(initialize: false));

			client.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress(line1: "Hello World"));
			server.GetDatabase<IContosoDatabase>().AddSaveAndCleanup<AddressEntity, long>(EntityFactory.GetAddress(line1: "Foo Bar"));

			var options = new SyncOptions { SyncDirection = SyncDirection.PushUp };
			using var engine = SyncEngine.Run(client, server, options);
			var issues = engine.SyncIssues;
			Assert.AreEqual(0, issues.Count, string.Join(",", issues.Select(x => x.Message)));

			using var clientDatabase = client.GetDatabase<IContosoDatabase>();
			using var serverDatabase = server.GetDatabase<IContosoDatabase>();

			var addresses1 = clientDatabase.Addresses.OrderBy(x => x.Id).ToList();
			var addresses2 = serverDatabase.Addresses.OrderBy(x => x.Id).ToList();
			Assert.AreEqual(1, addresses1.Count);
			Assert.AreEqual(2, addresses2.Count);

			Assert.AreNotEqual(addresses1[0].Line1, addresses2[0].Line1);
			Assert.AreEqual(addresses1[0].Line1, addresses2[1].Line1);
		}

		#endregion
	}
}