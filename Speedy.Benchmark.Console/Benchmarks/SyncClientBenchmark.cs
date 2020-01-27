#region References

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark.Benchmarks
{
	public class SyncClientBenchmark : BaseBenchmark
	{
		#region Methods

		public override void Run()
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

		private void Sync(string message, int items, (SyncClient server, SyncClient client) scenario)
		{
			Console.WriteLine(message);

			var (server, client) = scenario;
			var watch = Stopwatch.StartNew();

			ClearDatabase(server);
			ClearDatabase(client);

			Assert.AreEqual(0, Count<AddressEntity, long>(server));
			Assert.AreEqual(0, Count<AddressEntity, long>(client));

			AddData(client, items);

			Assert.AreEqual(0, Count<AddressEntity, long>(server));
			Assert.AreEqual(items, Count<AddressEntity, long>(client));

			var engine = new SyncEngine(client, server, new SyncOptions());
			engine.Run();

			Assert.AreEqual(items, Count<AddressEntity, long>(server));
			Assert.AreEqual(items, Count<AddressEntity, long>(client));

			Console.WriteLine($"Elapsed: {watch.Elapsed}");
		}

		private void AddData(SyncClient client, int count)
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