#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Samples.EntityFramework;

#endregion

namespace Speedy.Benchmarks
{
	internal class Program
	{
		#region Fields

		private static string _timeFormat;

		#endregion

		#region Methods

		private static void CleanupDatabase(string connectionString)
		{
			Log("Cleaning up the test database...");

			using (var database = new ContosoDatabase(connectionString))
			{
				var script = @"EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'
				EXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'
				EXEC sp_MSForEachTable 'IF ''?'' NOT LIKE ''%MigrationHistory%'' DELETE FROM ?'
				EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'
				EXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'
				EXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";

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

		private static void Log(string message, bool newLine = true, ICollection<string> results = null)
		{
			results?.Add(message);

			if (newLine)
			{
				Console.WriteLine(message);
				return;
			}

			Console.Write(message);
		}

		private static void Main(string[] args)
		{
			_timeFormat = "mm\\:ss\\:fff";

			var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Speedy";
			var connectionString = "server=localhost;database=speedy;integrated security=true;";
			var results = new List<string>();

			//TestSqlDatabase(results, connectionString, 10000);
			TestJsonDatabase(results, directory + "\\Database", 10000);
			TestRepository(results, directory + "\\Repository", 10000);

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
				repository.Read(randomKeys);

				Log("Total: " + watch.Elapsed.ToString(_timeFormat));
			}
		}

		private static void RandomReadsIndividually(string directory, string name, IEnumerable<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read the randomly selected keys from the " + name + " repository one at a time.");

			using (var repository = KeyValueRepository<string>.Create(directory, name))
			{
				var watch = Stopwatch.StartNew();

				foreach (var key in randomKeys)
				{
					try
					{
						repository.Read(key);
					}
					catch (Exception ex)
					{
						Log("Failed to read key " + key + ". " + ex.Message);
					}
				}

				Log("Total: " + watch.Elapsed.ToString(_timeFormat));
			}
		}

		private static string TestDatabase(DatabaseProvider<IContosoDatabase> provider, int total, int chunkSize)
		{
			Log($"Starting to benchmark Speedy database with {chunkSize} chunks...");

			var watch = Stopwatch.StartNew();
			var random = new Random();
			var count = 0;

			while (count < total)
			{
				using (var database = provider.GetDatabase())
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
				}

				//Console.Write(".");
			}

			var elapsed = watch.Elapsed.ToString(_timeFormat);
			Log($"Done: {elapsed}");
			Log(string.Empty);
			return $"{elapsed} : {chunkSize} chunks.";
		}

		private static void TestJsonDatabase(ICollection<string> results, string directory, int iterations)
		{
			var chunks = new[] { 150, 300, 600, 1200, 2400 };

			Log($"Starting to benchmark Speedy JSON Database writing {iterations}...", true, results);
			Log("JSON", true, results);

			foreach (var chunk in chunks)
			{
				CleanupDirectory(directory);
				results.Add(TestDatabase(new DatabaseProvider<IContosoDatabase>(x => new ContosoMemoryDatabase(directory, x)), iterations, chunk));
			}

			Log(string.Empty, true, results);
		}

		private static void TestRepository(ICollection<string> results, string directory, int iterations)
		{
			Log($"Starting to benchmark Speedy Repository writing {iterations}...", true, results);

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

		private static void TestSqlDatabase(ICollection<string> results, string connectionString, int iterations)
		{
			var chunks = new[] { 150, 300, 600, 1200, 2400 };

			Log($"Starting to benchmark Speedy SQL Database writing {iterations}...", true, results);
			Log("Entity Framework", true, results);

			foreach (var chunk in chunks)
			{
				CleanupDatabase(connectionString);
				results.Add(TestDatabase(new DatabaseProvider<IContosoDatabase>(x => new ContosoDatabase(connectionString, x)), iterations, chunk));
			}

			Log(string.Empty, true, results);
		}

		private static string WriteCollection(string directory, int size, int chunkSize, TimeSpan? timeout = null, int limit = 0)
		{
			Log(string.Empty);
			Log(limit <= 0
				? $"Let's create a repository with {size} items @ {chunkSize} at a time."
				: $"Let's create a repository with {size} items @ {chunkSize} at a time with a cache of {limit} items.");

			var watch = Stopwatch.StartNew();

			using (var repository = KeyValueRepository<string>.Create(directory, $"DB-{size}-{limit}", timeout, limit))
			{
				for (var i = 1; i <= size; i++)
				{
					if (limit > 0 && i % limit == 0)
					{
						repository.Flush();
					}

					if (i % chunkSize == 0)
					{
						repository.Save();
					}

					repository.Write(i.ToString(), i.ToString());
				}

				repository.Save();
				repository.Flush();

				var elapsed = watch.Elapsed.ToString(_timeFormat);
				Log($"Count: {repository.Count} : {elapsed}");

				return limit <= 0
					? $"{elapsed}: {chunkSize} at a time."
					: $"{elapsed}: {chunkSize} at a time with a cache of {limit} items.";
			}
		}

		#endregion
	}
}