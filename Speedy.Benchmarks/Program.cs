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

		private static bool _verboseLog;

		#endregion

		#region Methods

		private static void CleanupDirectory(string directory)
		{
			Log("Cleaning up the test data folder...");

			if (!directory.EndsWith("Speedy"))
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
			var iterations = 100000;
			var directory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Speedy";

			CleanupDirectory(directory);
			TestRepository(directory + "\\Repository", iterations);
			TestDatabase(new SampleDatabaseProvider(directory + "\\Database"), iterations, 1000);

			Log(string.Empty);
			Log("Press any key to continue...");
			Console.ReadKey();
		}

		private static void RandomReadsGroup(string directory, string name, HashSet<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read randomly into the " + name + " repository using all keys.");

			using (var repository = Repository<string>.Create(directory, name))
			{
				var watch = Stopwatch.StartNew();

				var values = repository.Read(randomKeys);
				foreach (var item in values)
				{
					Verbose("Read " + item.Value + " using key " + item.Key);
				}

				Log("Total: " + watch.Elapsed);
			}
		}

		private static void RandomReadsIndividually(string directory, string name, IEnumerable<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read randomly into the " + name + " repository @ 1 at a time.");

			using (var repository = Repository<string>.Create(directory, name))
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

				Log("Total: " + watch.Elapsed);
			}
		}

		private static void TestDatabase(ISampleDatabaseProvider provider, int total, int chunkSize)
		{
			Log("Starting to benchmark Speedy database...");

			var watch = Stopwatch.StartNew();
			var random = new Random();
			var count = 0;
			var loop = 0;

			while (count < total)
			{
				using (var database = provider.CreateContext())
				{
					for (var i = 0; i < chunkSize; i++)
					{
						var address = new Address
						{
							Line1 = "Line " + i,
							Line2 = "Line " + i,
							City = "City " + i,
							Postal = "Postal " + i,
							State = "State " + i
						};

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

			Log(string.Empty);
			Log("Done: " + watch.Elapsed);
			Log(string.Empty);
		}

		private static void TestRepository(string directory, int iterations)
		{
			Log("Starting to benchmark Speedy Repository...");

			WriteCollection(directory, iterations, 100);
			WriteCollection(directory, iterations, 1000);
			WriteCollection(directory, iterations, 2500);
			WriteCollection(directory, iterations, 10000);
			WriteCollection(directory, iterations, 50000);
			WriteCollection(directory, iterations, 100, TimeSpan.FromSeconds(30), 1000);
			WriteCollection(directory, iterations, 1000, TimeSpan.FromSeconds(30), 10000);
			WriteCollection(directory, iterations, 2500, TimeSpan.FromSeconds(30), 10000);
			WriteCollection(directory, iterations, 10000, TimeSpan.FromSeconds(30), 25000);
			WriteCollection(directory, iterations, 50000, TimeSpan.FromSeconds(30), 100000);

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

		private static void WriteCollection(string directory, int size, int chunkSize, TimeSpan? timeout = null, int limit = 0)
		{
			Log(string.Empty);
			Log(limit <= 0 ? $"Let's create a repository with {size} items @ {chunkSize} at a time."
				: $"Let's create a repository with {size} items @ {chunkSize} at a time with a cache of {limit} items.");

			var watch = Stopwatch.StartNew();

			using (var repository = Repository<string>.Create(directory, $"DB-{size}-{limit}", timeout, limit))
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

				Log("Done: " + watch.Elapsed);
				Log($"Count: {repository.Count}");
				Log(string.Empty);
			}
		}

		#endregion
	}
}