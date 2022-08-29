#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class TimeServiceTests
	{
		#region Methods

		[TestMethod]
		public void CanPopOldProvidersWithoutAffectingCurrentTime()
		{
			var start = TimeService.UtcNow;
			var provider1 = () => new DateTime(2022, 08, 26, 02, 44, 00, DateTimeKind.Utc);
			var provider2 = () => new DateTime(2022, 08, 26, 02, 44, 01, DateTimeKind.Utc);
			var provider3 = () => new DateTime(2022, 08, 26, 02, 44, 02, DateTimeKind.Utc);

			var id1 = TimeService.AddUtcNowProvider(provider1);
			Assert.IsNotNull(id1);
			Assert.AreEqual(provider1.Invoke(), TimeService.UtcNow);
			var id2 = TimeService.AddUtcNowProvider(provider2);
			Assert.IsNotNull(id2);
			Assert.AreEqual(provider2.Invoke(), TimeService.UtcNow);
			var id3 = TimeService.AddUtcNowProvider(provider3);
			Assert.IsNotNull(id3);
			Assert.AreEqual(provider3.Invoke(), TimeService.UtcNow);

			Assert.AreEqual(provider3.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id1.Value);
			Assert.AreEqual(provider3.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id2.Value);
			Assert.AreEqual(provider3.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id3.Value);
			Assert.IsTrue(DateTime.UtcNow > start);
		}

		[TestMethod]
		public void LastProviderShouldBeUsed()
		{
			var provider1 = () => new DateTime(2022, 08, 26, 02, 44, 00, DateTimeKind.Utc);
			var provider2 = () => new DateTime(2022, 08, 26, 02, 44, 01, DateTimeKind.Utc);
			var provider3 = () => new DateTime(2022, 08, 26, 02, 44, 02, DateTimeKind.Utc);

			var id1 = TimeService.AddUtcNowProvider(provider1);
			Assert.IsNotNull(id1);
			var id2 = TimeService.AddUtcNowProvider(provider2);
			Assert.IsNotNull(id2);
			var id3 = TimeService.AddUtcNowProvider(provider3);
			Assert.IsNotNull(id3);

			DateTime actual1, actual2;

			var result1 = Profiler.Profile(() => actual1 = TimeService.UtcNow);
			var result2 = Profiler.Profile(() => actual2 = DateTime.UtcNow);

			result1.Dump();
			result2.Dump();

			result1 = Profiler.Profile(() => actual1 = TimeService.UtcNow);
			result2 = Profiler.Profile(() => actual2 = DateTime.UtcNow);

			result1.Dump();
			result2.Dump();

			Assert.AreEqual(provider3.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id3.Value);
			Assert.AreEqual(provider2.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id2.Value);
			Assert.AreEqual(provider1.Invoke(), TimeService.UtcNow);
			TimeService.RemoveUtcNowProvider(id1.Value);
		}

		[TestMethod]
		public void RandomRemoves()
		{
			var current = TimeService.UtcNow;
			var start = new DateTime(2022, 08, 26, 02, 44, 00, DateTimeKind.Utc);
			var ids = new List<uint>(1000);

			for (var i = 0; i < ids.Count; i++)
			{
				var providerDateTime = start.AddSeconds(i);
				if (TimeService.TryAddUtcNowProvider(() => providerDateTime, out var id) && id is { } uid)
				{
					ids.Add(uid);
				}
			}

			var random = new Random();

			while (ids.Count > 0)
			{
				var actual = TimeService.UtcNow;
				Assert.IsTrue(actual < current);

				var randomIndex = random.Next(0, ids.Count);
				var providerId = ids[randomIndex];

				TimeService.RemoveUtcNowProvider(providerId);
			}
		}

		#endregion
	}
}