#region References

using System;
using System.Diagnostics;
using Speedy.Data.Client;
using Speedy.Sync;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark
{
	public static class SyncEntityBenchmark
	{
		#region Methods

		public static void Run()
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

		private static AddressEntity GetAddressEntity()
		{
			return new AddressEntity();
		}

		private static ClientAddress GetClientAddress()
		{
			return new ClientAddress();
		}

		private static ClientAccount GetClientPerson()
		{
			return new ClientAccount();
		}

		private static AccountEntity GetPersonEntity()
		{
			return new AccountEntity();
		}

		private static void UpdateWith(string message, int items, Func<ISyncEntity> getEntity)
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