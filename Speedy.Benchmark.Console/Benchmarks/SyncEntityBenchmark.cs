#region References

using System;
using System.Diagnostics;
using Speedy.Data.Client;
using Speedy.Sync;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark.Benchmarks
{
	public class SyncEntityBenchmark : BaseBenchmark
	{
		#region Methods

		public override void Run()
		{
			//
			// Test Updating
			// This should validate both exclusion caching and UpdateWith speed
			//

			var testCounts = new[] { 10000, 20000 };

			foreach (var items in testCounts)
			{
				UpdateWith($"Starting UpdateWith test with {items} Address Entity items", items, GetAddressEntity);
				UpdateWith($"Starting UpdateWith test with {items} Person Entity items", items, GetPersonEntity);
				UpdateWith($"Starting UpdateWith test with {items} Client Address items", items, GetClientAddress);
				UpdateWith($"Starting UpdateWith test with {items} Client Person items", items, GetClientPerson);
			}
		}

		private AddressEntity GetAddressEntity()
		{
			return new AddressEntity();
		}

		private ClientAddress GetClientAddress()
		{
			return new ClientAddress();
		}

		private ClientAccount GetClientPerson()
		{
			return new ClientAccount();
		}

		private AccountEntity GetPersonEntity()
		{
			return new AccountEntity();
		}

		private void UpdateWith(string message, int items, Func<ISyncEntity> getEntity)
		{
			Console.WriteLine(message);

			var watch = Stopwatch.StartNew();
			var updates = 0;

			for (var i = 0; i < items; i++)
			{
				var expected = getEntity();
				var actual = getEntity();

				expected.UpdateWith(actual, false, false, false);
				expected.UpdateWith(actual, true, false, false);
				expected.UpdateWith(actual, false, true, false);
				expected.UpdateWith(actual, false, false, true);
				expected.UpdateWith(actual, true, true, false);
				expected.UpdateWith(actual, true, false, true);
				expected.UpdateWith(actual, false, true, true);
				expected.UpdateWith(actual, true, true, true);

				updates += 8;
			}

			Console.WriteLine($"Elapsed: {watch.Elapsed}, updates {updates}");
		}

		#endregion
	}
}