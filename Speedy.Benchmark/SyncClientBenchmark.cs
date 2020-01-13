#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.EntityFramework;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark
{
	public static class SyncClientBenchmark
	{
		#region Methods

		public static void Run()
		{
			//
			// Test Updating
			// This should validate both exclusion caching and UpdateWith speed
			//

			var testCounts = new[] { 1000, 2000 };

			foreach (var items in testCounts)
			{
				// Ensure we get new memory repositories
				var scenarios = GetScenarios().ToList();

				foreach (var scenario in scenarios)
				{
					Sync($"Starting Sync from {scenario.client.Name} to {scenario.server.Name} with {items} items", items, scenario);
				}
			}
		}

		private static void Sync(string message, int items, (SyncClient server, SyncClient client) scenario)
		{
			Console.WriteLine(message);

			var (server, client) = scenario;
			var watch = Stopwatch.StartNew();

			server.ClearDatabase();
			client.ClearDatabase();

			Assert.AreEqual(0, server.Count<AddressEntity, long>());
			Assert.AreEqual(0, client.Count<AddressEntity, long>());

			AddData(client, items);

			Assert.AreEqual(0, server.Count<AddressEntity, long>());
			Assert.AreEqual(items, client.Count<AddressEntity, long>());

			var engine = new SyncEngine(client, server, new SyncOptions());
			engine.Run();

			Assert.AreEqual(items, server.Count<AddressEntity, long>());
			Assert.AreEqual(items, client.Count<AddressEntity, long>());

			Console.WriteLine($"Elapsed: {watch.Elapsed}");
		}

		private static void ClearDatabase(this ISyncClient client)
		{
			using (var database = client.GetDatabase())
			{
				if (database is EntityFrameworkDatabase efDatabase)
				{
					efDatabase.ClearDatabase();
				}
			}
		}
		
		private static int Count<T, T2>(this ISyncClient client) where T : Entity<T2>
		{
			using (var database = client.GetDatabase())
			{
				return database.GetRepository<T,T2>().Count();
			}
		}

		private static IEnumerable<(SyncClient server, SyncClient client)> GetScenarios()
		{
			yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()));
			yield return (new SyncClient("Server (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (Sqlite2)", GetEntityFrameworkSqliteProvider2()));
			//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider()));
			//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (SQL2)", GetSyncableEntityFrameworkProvider2()));
		}
		private static void AddData(SyncClient client, int count)
		{
			var database = client.GetDatabase<IContosoDatabase>();

			for (var i = 0; i < count; i++)
			{
				var address = new AddressEntity
				{
					City = "City",
					Id = default,
					Line1 = "Line1",
					Line2 = "Line2",
					LinkedAddressId = null,
					LinkedAddressSyncId = null,
					Postal = "Postal",
					State = "State"
				};
				database.Addresses.Add(address);

				if (i > 0 && i % 300 == 0)
				{
					database.SaveChanges();
					database.Dispose();
					database = client.GetDatabase<IContosoDatabase>();
				}
			}

			database.SaveChanges();
			database.Dispose();
		}

		#endregion
	}
}