#region References

using System;
using System.Collections;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Storage;

#endregion

namespace Speedy.UnitTests.Storage
{
	[TestClass]
	public class MemoryCacheTests
	{
		#region Methods

		[TestMethod]
		public void CacheShouldExpire()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(1));
			cache.Set("foo", "bar");
			Assert.IsTrue(cache.TryGet("foo", out var item));
			Assert.AreEqual("bar", (string) item.Value);

			// Bump to expiration date
			TestHelper.IncrementTime(TimeSpan.FromSeconds(1));
			Assert.IsFalse(cache.TryGet("foo", out item));
			Assert.IsNull(item);
		}

		[TestMethod]
		public void ClearShouldWork()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			Assert.AreEqual(2, cache.Count);

			cache.Clear();

			Assert.AreEqual(0, cache.Count);
			Assert.AreEqual(0, cache.ToArray().Length);
		}

		[TestMethod]
		public void Constructor()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache();
			Assert.IsTrue(cache.IsEmpty);
			Assert.IsTrue(cache.IsSynchronized);
			Assert.IsTrue(cache.SlidingExpiration);
			Assert.AreEqual(TimeSpan.FromMinutes(15), cache.DefaultTimeout);

			cache = new MemoryCache(TimeSpan.FromSeconds(1));
			Assert.IsTrue(cache.IsEmpty);
			Assert.IsTrue(cache.IsSynchronized);
			Assert.IsTrue(cache.SlidingExpiration);
			Assert.AreEqual(TimeSpan.FromSeconds(1), cache.DefaultTimeout);
		}

		[TestMethod]
		public void CopyTo()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var array = new MemoryCacheItem[2];
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			Assert.AreEqual(2, cache.Count);

			cache.CopyTo(array, 0);

			TestHelper.AreEqual(cache.ToArray(), array);
		}

		[TestMethod]
		public void EnumeratorShouldWork()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			Assert.IsTrue(cache.SlidingExpiration);

			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			var actual = cache.ToArray();
			Assert.AreEqual(2, actual.Length);

			// Enumerating the cache will not change access time therefore will not effect expiration date.
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), actual[0].ExpirationDate);
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), actual[1].ExpirationDate);

			TestHelper.IncrementTime(seconds: 2);

			var count = 0;
			foreach (MemoryCacheItem item in (IEnumerable) cache)
			{
				count++;
				Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
			}
			Assert.AreEqual(2, count);
		}

		[TestMethod]
		public void RemoveShouldWork()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			Assert.AreEqual(2, cache.Count);

			var actual = cache.Remove("foo");
			Assert.IsNotNull(actual);
			Assert.AreEqual("foo", actual.Key);

			Assert.AreEqual(1, cache.Count);
			Assert.AreEqual("hello", cache.First().Key);
		}

		[TestMethod]
		public void SetMultipleCallsShouldUpdateNotAdd()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			cache.Set("foo", "bar");

			Assert.AreEqual(1, cache.Count);
			Assert.IsTrue(cache.TryGet("foo", out var item));
			Assert.AreEqual("bar", (string) item.Value);

			TestHelper.IncrementTime(seconds: 1);
			cache.Set("foo", "bar2");

			Assert.AreEqual(1, cache.Count);
			Assert.IsTrue(cache.TryGet("foo", out item));
			Assert.AreEqual("bar2", (string) item.Value);
		}

		[TestMethod]
		public void SlidingExpirationDisabledShouldNotAffectItems()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10))
			{
				SlidingExpiration = false
			};
			Assert.IsFalse(cache.SlidingExpiration);
			Assert.AreEqual(0, cache.Count);

			cache.Set("foo", "bar");
			Assert.IsTrue(cache.TryGet("foo", out var item));
			Assert.AreEqual("bar", (string) item.Value);
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
			Assert.AreEqual(1, cache.Count);

			// Accessing the value should not bump expiration
			TestHelper.IncrementTime(seconds: 5);
			cache.TryGet("foo", out item);
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
			Assert.IsFalse(item.HasExpired);
			Assert.AreEqual(1, cache.Count);

			// Bump to expiration date
			TestHelper.IncrementTime(seconds: 5);
			Assert.IsFalse(cache.TryGet("foo", out item));
			Assert.IsNull(item);
			Assert.AreEqual(0, cache.Count);
		}

		[TestMethod]
		public void SlidingExpirationShouldWork()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			Assert.IsTrue(cache.SlidingExpiration);

			cache.Set("foo", "bar");
			Assert.IsTrue(cache.TryGet("foo", out var item));
			Assert.AreEqual("bar", (string) item.Value);
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);

			// Accessing the value should bump expiration
			TestHelper.IncrementTime(seconds: 5);
			cache.TryGet("foo", out item);
			Assert.AreEqual(new DateTime(2022, 09, 17, 09, 36, 15), item.ExpirationDate);
			Assert.IsFalse(item.HasExpired);

			// Bump to expiration date
			TestHelper.IncrementTime(seconds: 10);
			Assert.IsFalse(cache.TryGet("foo", out item));
			Assert.IsNull(item);
		}

		[TestMethod]
		public void TryGetInvalidKey()
		{
			TestHelper.SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
			var cache = new MemoryCache(TimeSpan.FromSeconds(10));
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			Assert.AreEqual(2, cache.Count);

			var actual = cache.TryGet("aoeu", out var item);
			Assert.IsFalse(actual);
			Assert.IsNull(item);
		}

		#endregion
	}
}