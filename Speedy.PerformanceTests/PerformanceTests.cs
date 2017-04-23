#region References

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.IntegrationTests;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.PerformanceTests
{
	[TestClass]
	public class PerformanceTests
	{
		#region Methods

		[TestMethod]
		public void ChunkSpeed()
		{
			int changes;
			var total = 1500;
			var chunk = 500;
			var offset = 0;

			var expectedSpeeds = new[]
			{
				TimeSpan.FromMilliseconds(6500),
				TimeSpan.FromMilliseconds(2000),
				TimeSpan.FromMilliseconds(7000)
			};

			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					var watch = Stopwatch.StartNew();
					var last = TimeSpan.Zero;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 1; i <= total; i++)
						{
							var expected = new Address { City = "City", Line1 = $"Line{i}", Line2 = "Line2", Postal = "Postal", State = "State" };
							database.Addresses.Add(expected);

							if (i % chunk == 0)
							{
								changes = database.SaveChanges();

								if (changes > 0)
								{
									$"{changes} - {watch.Elapsed - last}".Dump();
								}

								last = watch.Elapsed;
							}
						}

						changes = database.SaveChanges();
						watch.Stop();

						if (changes > 0)
						{
							$"{changes} - {watch.Elapsed - last}".Dump();
						}
					}

					watch.Elapsed.Dump();

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(total, database.Addresses.Count());
						Assert.IsTrue(watch.Elapsed < expectedSpeeds[offset++]);
					}
				});
		}

		[TestMethod]
		public void NoChunkSpeed()
		{
			int changes;
			var total = 1500;
			var offset = 0;

			var expectedSpeeds = new[]
			{
				TimeSpan.FromMilliseconds(6500),
				TimeSpan.FromMilliseconds(1000),
				TimeSpan.FromMilliseconds(6500)
			};

			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					var watch = Stopwatch.StartNew();
					var last = TimeSpan.Zero;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);

						for (var i = 1; i <= total; i++)
						{
							var expected = new Address { City = "City", Line1 = $"Line{i}", Line2 = "Line2", Postal = "Postal", State = "State" };
							database.Addresses.Add(expected);
						}

						changes = database.SaveChanges();
						watch.Stop();

						if (changes > 0)
						{
							$"{changes} - {watch.Elapsed - last}".Dump();
						}
					}

					using (var database = provider.GetDatabase())
					{
						Assert.AreEqual(total, database.Addresses.Count());
						Assert.IsTrue(watch.Elapsed < expectedSpeeds[offset++]);
					}
				});
		}

		#endregion
	}
}