#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Speedy.Samples;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Benchmarks
{
	internal class Program
	{
		#region Fields

		private static string _timeFormat;

		private static bool _verboseLog;

		#endregion

		#region Methods

		private static void CleanupDatabase(string connectionString)
		{
			Log("Cleaning up the test database...");

			using (var database = new EntityFrameworkSampleDatabase(connectionString))
			{
				var script = @"EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
				EXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'
				EXEC sp_MSForEachTable 'IF ''?'' NOT LIKE ''%MigrationHistory%'' DELETE FROM ?'
				EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'
				EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'";

				database.Database.ExecuteSqlCommand(script);
			}
		}

		private static void CleanupDirectory(string directory)
		{
			Log("Cleaning up the test data folder...");

			if (!directory.EndsWith("Database"))
			{
				throw new ArgumentException("Not valid directory.");
			}

			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, true);
				Directory.CreateDirectory(directory);
			}
			else
			{
				Directory.CreateDirectory(directory);
			}
		}

		private static void Log(string message, bool newLine = true)
		{
			if (newLine)
			{
				Console.WriteLine(message);
				return;
			}

			Console.Write(message);
		}

		private static void Main(string[] args)
		{
			_verboseLog = false;
			_timeFormat = "ss\\:fff";

			var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Speedy";
			var connectionString = "server=localhost;database=speedy;integrated security=true;";
			var results = new List<string>();

			TestRepository(results, directory + "\\Repository", 100000);
			TestDatabase(results, directory + "\\Database", connectionString, 10000);

			Log(string.Empty);
			results.ForEach(x => Log(x));
			Log("Press any key to continue...");
			Console.ReadKey();
		}

		private static void RandomReadsGroup(string directory, string name, HashSet<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read the randomly selected keys from the " + name + " repository all at once.");

			using (var repository = KeyValueRepository<string>.Create(directory, name))
			{
				var watch = Stopwatch.StartNew();

				var values = repository.Read(randomKeys);
				foreach (var item in values)
				{
					Verbose("Read " + item.Value + " using key " + item.Key);
				}

				Log("Total: " + watch.Elapsed.ToString(_timeFormat));
			}
		}

		private static void RandomReadsIndividually(string directory, string name, IEnumerable<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read the randomly selected keys from the " + name + " repository one at a time.");

			using (var repository = KeyValueRepository<string>.Create(directory, name))
			{
				var previousTime = new TimeSpan(0);
				var watch = Stopwatch.StartNew();

				foreach (var key in randomKeys)
				{
					try
					{
						var value = repository.Read(key);
						Verbose("Read " + value + " using key " + key + " in " + (watch.Elapsed - previousTime));
					}
					catch (Exception ex)
					{
						Log("Failed to read key " + key + ". " + ex.Message);
					}

					previousTime = watch.Elapsed;
				}

				Log("Total: " + watch.Elapsed.ToString(_timeFormat));
			}
		}

		private static void TestDatabase(List<string> results, string directory, string connectionString, int iterations)
		{
			Log($"Starting to benchmark Speedy Database writing {iterations}...");
			results.Add($"Starting to benchmark Speedy Database writing {iterations}...");

			var chunks = new[] { 150, 300, 600, 1200, 2400 };

			Log("JSON");
			results.Add("JSON");

			foreach (var chunk in chunks)
			{
				CleanupDirectory(directory);
				results.Add(TestDatabase(new SampleDatabaseProvider(directory), iterations, chunk));
			}

			Log("Entity Framework");
			results.Add("Entity Framework");

			foreach (var chunk in chunks)
			{
				CleanupDatabase(connectionString);
				results.Add(TestDatabase(new EntityFrameworkSampleDatabaseProvider(connectionString), iterations, chunk));
			}
		}

		private static string TestDatabase(ISampleDatabaseProvider provider, int total, int chunkSize)
		{
			Log($"Starting to benchmark Speedy database with {chunkSize} chunks...");

			var watch = Stopwatch.StartNew();
			var random = new Random();
			var count = 0;
			var loop = 0;

			while (count < total)
			{
				using (var database = provider.CreateContext())
				{
					for (var i = count; i < count + chunkSize; i++)
					{
						var address = new Address { Line1 = "Line " + i, Line2 = "Line " + i, City = "City " + i, Postal = "Postal " + i, State = "State " + i };

						if (random.Next(1, 100) % 2 == 0)
						{
							address.People.Add(new Person
							{
								Name = "Person " + i
							});
						}

						database.Addresses.Add(address);
					}

					count += chunkSize;

					database.SaveChanges();

					Log(".", false);

					if (loop++ >= 80)
					{
						Log(string.Empty);
						loop = 0;
					}
				}
			}

			var elapsed = watch.Elapsed.ToString(_timeFormat);

			Log(string.Empty);
			Log($"Done: {elapsed}");
			Log(string.Empty);

			return $"{elapsed} : {chunkSize} chunks.";
		}

		private static void TestRepository(List<string> results, string directory, int iterations)
		{
			Log($"Starting to benchmark Speedy Repository writing {iterations}...");
			results.Add($"Starting to benchmark Speedy Repository writing {iterations}...");

			results.Add(WriteCollection(directory, iterations, 100));
			results.Add(WriteCollection(directory, iterations, 1000));
			results.Add(WriteCollection(directory, iterations, 2500));
			results.Add(WriteCollection(directory, iterations, 10000));
			results.Add(WriteCollection(directory, iterations, 50000));
			results.Add(WriteCollection(directory, iterations, 100, TimeSpan.FromSeconds(30), 1000));
			results.Add(WriteCollection(directory, iterations, 1000, TimeSpan.FromSeconds(30), 10000));
			results.Add(WriteCollection(directory, iterations, 2500, TimeSpan.FromSeconds(30), 10000));
			results.Add(WriteCollection(directory, iterations, 10000, TimeSpan.FromSeconds(30), 25000));
			results.Add(WriteCollection(directory, iterations, 50000, TimeSpan.FromSeconds(30), 100000));

			// Populate the random keys.
			var random = new Random();
			var randomKeys = new HashSet<string>();

			for (var i = 0; i < 100; i++)
			{
				randomKeys.Add(random.Next(1, iterations).ToString());
			}

			Log("The random keys are " + string.Join(", ", randomKeys.Select(x => x)));

			RandomReadsIndividually(directory, "DB-" + iterations + "-0", randomKeys);
			RandomReadsGroup(directory, "DB-" + iterations + "-0", randomKeys);
		}

		private static void Verbose(string message, bool newLine = true)
		{
			if (!_verboseLog)
			{
				return;
			}

			if (newLine)
			{
				Console.WriteLine(message);
				return;
			}

			Console.Write(message);
		}

		private static string WriteCollection(string directory, int size, int chunkSize, TimeSpan? timeout = null, int limit = 0)
		{
			Log(string.Empty);
			Log(limit <= 0 ? $"Let's create a repository with {size} items @ {chunkSize} at a time."
				: $"Let's create a repository with {size} items @ {chunkSize} at a time with a cache of {limit} items.");

			var watch = Stopwatch.StartNew();

			using (var repository = KeyValueRepository<string>.Create(directory, $"DB-{size}-{limit}", timeout, limit))
			{
				var previousTime = new TimeSpan(0);

				for (var i = 1; i <= size; i++)
				{
					if (i % (chunkSize / 4) == 0)
					{
						Verbose(".", false);
					}

					if (limit > 0 && i % limit == 0)
					{
						Verbose("Flushing the repository because we hit the limit.");
						repository.Flush();
					}

					if (i % chunkSize == 0)
					{
						repository.Save();

						if (previousTime.Ticks > 0)
						{
							var difference = watch.Elapsed - previousTime;
							Verbose(watch.Elapsed + " + " + difference);
						}
						else
						{
							Verbose(watch.Elapsed.ToString());
						}

						previousTime = watch.Elapsed;
					}

					repository.Write(i.ToString(), i.ToString());
				}

				repository.Save();
				repository.Flush();

				var elapsed = watch.Elapsed.ToString(_timeFormat);

				Log($"Count: {repository.Count} : {elapsed}");
				Log(string.Empty);

				return limit <= 0 ? $"{elapsed}: {chunkSize} at a time."
					: $"{elapsed}: {chunkSize} at a time with a cache of {limit} items.";
			}
		}

		#endregion
	}
}