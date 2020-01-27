#region References

using System;
using System.Windows.Forms;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples;

#endregion

namespace Speedy.Benchmark.Benchmarks
{
	public class SyncEngineBenchmark : BaseBenchmark
	{
		#region Methods

		public override void Run()
		{
			foreach (var (server, client) in GetScenarios())
			{
				ClearDatabase(server);
				ClearDatabase(client);

				AddData(server);

				var engine = new SyncEngine(client, server, new SyncOptions());
				engine.SyncStateChanged += EngineOnSyncStateChanged;
				engine.Run();
			}
		}

		private void EngineOnSyncStateChanged(object sender, SyncEngineState e)
		{
			Console.WriteLine($"{e.Status}, {e.Message}, {e.Count}/{e.Percent}");
		}

		private void AddData(SyncClient server)
		{
			var database = server.GetDatabase<IContosoDatabase>();
			var count = 0;

			Console.WriteLine("Adding addresses...");

			for (var i = 0; i < 10000; i++)
			{
				database.Addresses.Add(EntityFactory.GetAddress(x => x.Line1 = $"Address {i} Line 1"));
				count++;

				if (count >= 300)
				{
					database.SaveChanges();
					database.Dispose();

					Console.WriteLine($"{i}...");
					database = server.GetDatabase<IContosoDatabase>();
					count = 0;
				}
			}

			Console.WriteLine("10000...");
			database.SaveChanges();
			database.Dispose();
		}

		#endregion
	}
}