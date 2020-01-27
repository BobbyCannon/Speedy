#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark.Benchmarks
{
	public class DatabaseBenchmark : BaseBenchmark
	{
		#region Methods

		public override void Run()
		{
			throw new NotImplementedException();
		}

		private IEnumerable<AddressEntity> GenerateData()
		{
			var list = new List<AddressEntity>(10000);

			for (var i = 0; i < 10000; i++)
			{
				list.Add(new AddressEntity
				{
					City = Guid.NewGuid().ToString(),
					Id = default,
					Line1 = Guid.NewGuid().ToString(),
					Line2 = Guid.NewGuid().ToString(),
					LinkedAddressId = null,
					LinkedAddressSyncId = null,
					Postal = Guid.NewGuid().ToString(),
					State = Guid.NewGuid().ToString(),
					SyncId = Guid.NewGuid()
				});
			}

			return list;
		}

		private void ProcessAllDatabases()
		{
			IEnumerable<(SyncClient server, SyncClient client)> GetScenarios()
			{
				//(new SyncClient("Server (mem)", GetSyncableMemoryProvider(options: options)), new SyncClient("Client (mem)", GetSyncableMemoryProvider(options: options2))),
				//yield return (new SyncClient("Server (mem)", GetSyncableMemoryProvider(options: options)), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
				//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (mem)", GetSyncableMemoryProvider(options: options)));
				yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (Sqlite2)", GetEntityFrameworkSqliteProvider2()));
				//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
				//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider()));
				//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (SQL2)", GetSyncableEntityFrameworkProvider2()));
			}

			var scenarios = GetScenarios();

			foreach (var scenario in scenarios)
			{
				ProcessScenario(scenario);
				ProcessScenario(scenario);
			}
		}

		private void ProcessScenario((SyncClient server, SyncClient client) scenario)
		{
			var server = scenario.server;
			var client = scenario.client;

			$"{client.Name} -> {server.Name}".Dump();

			var watch = Stopwatch.StartNew();
			var database = server.GetDatabase<IContosoDatabase>();
			var count = 5000;

			for (var i = 0; i < count; i++)
			{
				var address = new AddressEntity
				{
					City = Guid.NewGuid().ToString(),
					Id = default,
					Line1 = Guid.NewGuid().ToString(),
					Line2 = Guid.NewGuid().ToString(),
					LinkedAddressId = null,
					LinkedAddressSyncId = null,
					Postal = Guid.NewGuid().ToString(),
					State = Guid.NewGuid().ToString(),
					SyncId = Guid.NewGuid()
				};
				database.Addresses.Add(address);

				if (i > 0 && i % 300 == 0)
				{
					database.SaveChanges();
					database.Dispose();
					database = server.GetDatabase<IContosoDatabase>();
				}
			}

			database.SaveChanges();
			database.Dispose();
			watch.Elapsed.ToString().Dump("Data Written : ");
			watch.Restart();

			var engine = new SyncEngine(client, server, new SyncOptions());
			engine.Run();

			watch.Elapsed.ToString().Dump("Data Synced  : ");

			using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				clientDatabase.Addresses.Count().Dump("Client Count : ");
				serverDatabase.Addresses.Count().Dump("Server Count : ");
			}
		}

		private void WriteManyValuesToDatabase()
		{
			var memProvider = GetSyncableMemoryProvider();
			var liteProvider = GetEntityFrameworkSqliteProvider();
			var watch = Stopwatch.StartNew();

			// Generate the data
			var data = GenerateData();
			Console.WriteLine(watch.Elapsed + ": Data Created");
			watch.Restart();

			using (var database = (IContosoDatabase) liteProvider.GetSyncableDatabase())
			{
				foreach (var item in data)
				{
					database.Addresses.Add(item);
				}

				Console.WriteLine(watch.Elapsed + ": Data Added");
				watch.Restart();

				database.SaveChanges();
				Console.WriteLine(watch.Elapsed + ": Data Saved");
			}

			watch.Restart();

			using (var database = (IContosoDatabase) liteProvider.GetSyncableDatabase())
			{
				var count = database.Addresses.Count();
				Console.WriteLine(watch.Elapsed + $": {count}");
			}

			watch.Restart();

			using (var source = (IContosoDatabase) liteProvider.GetSyncableDatabase())
			using (var destination = (IContosoDatabase) memProvider.GetSyncableDatabase())
			{
				foreach (var item in source.Addresses)
				{
					// ~2.6s - Could speed this up by see UpdateWith for notes
					var address = new AddressEntity();
					address.UpdateWith(item);
					destination.Addresses.Add(address);

					// ~9s - Unwrap uses serialization and take a long time
					// May want to update unwrap to use "UpdateWith"?
					//destination.Addresses.Add(item.Unwrap<AddressEntity>());
				}

				Console.WriteLine(watch.Elapsed + ": Data Added");
				watch.Restart();

				destination.SaveChanges();
				Console.WriteLine(watch.Elapsed + ": Data Saved");
			}
		}

		#endregion
	}
}