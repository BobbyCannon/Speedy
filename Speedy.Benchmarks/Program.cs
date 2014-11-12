#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

#endregion

namespace Speedy.Benchmarks
{
	internal class Program
	{
		#region Static Methods

		private static void CleanupDirectory(string directory)
		{
			Log("Cleaning up the test data folder...");

			if (Directory.Exists(directory))
			{
				var directoryInfo = new DirectoryInfo(directory);
				foreach (var file in directoryInfo.EnumerateFiles())
				{
					file.Delete();
				}
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
			Log("Starting to benchmark Speedy... hold on to your hats!");

			var directory = "C:\\SpeedyTest";
			CleanupDirectory(directory);

			WriteCollection(directory, 10000, 100);
			WriteCollection(directory, 10000, 1000);
			WriteCollection(directory, 10000, 2500);

			// Populate the random keys.
			var random = new Random();
			var randomKeys = new HashSet<string>();
			for (var i = 0; i < 10; i++)
			{
				randomKeys.Add(random.Next(1, 10000).ToString());
			}

			Log("The random keys are " + string.Join(", ", randomKeys.Select(x => x)));

			RandomReadsIndividually(directory, "DB-10000-AT-100", randomKeys);
			RandomReadsGroup(directory, "DB-10000-AT-100", randomKeys);

			Log(string.Empty);
			Log("Press any key to continue...");
			Console.ReadKey();
		}

		private static void RandomReadsGroup(string directory, string name, HashSet<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read randomly into the " + name + " repository using all keys.");

			var repository = new Repository(directory, name);
			var watch = Stopwatch.StartNew();

			var values = repository.Read(randomKeys);
			foreach (var item in values)
			{
				Log("Read " + item.Value + " using key " + item.Key);
			}

			Log("Total: " + watch.Elapsed);
		}

		private static void RandomReadsIndividually(string directory, string name, IEnumerable<string> randomKeys)
		{
			Log(string.Empty);
			Log("Let's read randomly into the " + name + " repository @ 1 at a time.");

			var repository = new Repository(directory, name);
			var previousTime = new TimeSpan(0);
			var watch = Stopwatch.StartNew();

			foreach (var key in randomKeys)
			{
				try
				{
					var value = repository.Read(key);
					Log("Read " + value + " using key " + key + " in " + (watch.Elapsed - previousTime));
				}
				catch (Exception ex)
				{
					Log("Failed to read key " + key + ". " + ex.Message);
				}

				previousTime = watch.Elapsed;
			}

			Log("Total: " + watch.Elapsed);
		}
		
		private static void WriteCollection(string directory, int size, int chunkSize)
		{
			Log(string.Empty);
			Log("Let's create a repository with " + size + " items @ " + chunkSize + " at a time.");
			var watch = Stopwatch.StartNew();
			var repository = new Repository(directory, "DB-" + size + "-AT-" + chunkSize);
			var previousTime = new TimeSpan(0);

			for (var i = 1; i <= size; i++)
			{
				if (i % (chunkSize / 4) == 0)
				{
					Log(".", false);
				}

				if (i % chunkSize == 0)
				{
					repository.Save();

					if (previousTime.Ticks > 0)
					{
						var difference = watch.Elapsed - previousTime;
						Log(watch.Elapsed + " + " + difference);
					}
					else
					{
						Log(watch.Elapsed.ToString());
					}

					previousTime = watch.Elapsed;
				}

				repository.Write(i.ToString(), i.ToString());
			}

			repository.Save();

			Log(string.Empty);
			Log("Done: " + watch.Elapsed);
		}

		#endregion
	}
}