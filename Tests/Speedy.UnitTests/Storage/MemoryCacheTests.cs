#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.UnitTests.Storage;

[TestClass]
public class MemoryCacheTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void CacheShouldExpire()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(1));
		var count = 0;

		void onCacheOnItemRemoved(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (args.Action == NotifyCollectionChangedAction.Remove)
			{
				count++;
			}
		}

		try
		{
			cache.CollectionChanged += onCacheOnItemRemoved;
			cache.Set("foo", "bar");
			IsTrue(cache.TryGet("foo", out var item));
			AreEqual("bar", (string) item.Value);

			// Bump to expiration date
			IncrementTime(TimeSpan.FromSeconds(1));
			IsFalse(cache.TryGet("foo", out item));
			IsNull(item);
			AreEqual(1, count);
		}
		finally
		{
			cache.CollectionChanged -= onCacheOnItemRemoved;
		}
	}

	[TestMethod]
	public void CleanUpShouldWork()
	{
		var count = 0;

		void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			count += args.NewItems?.Count ?? 0;
			count += args.OldItems?.Count ?? 0;
		}

		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));

		try
		{
			cache.CollectionChanged += onCollectionChanged;
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			AreEqual(2, cache.Count);
			AreEqual(2, count);

			IncrementTime(seconds: 9);

			cache.Cleanup();
			AreEqual(2, cache.Count);

			IncrementTime(seconds: 1);

			cache.Cleanup();
			AreEqual(0, cache.Count);
			AreEqual(4, count);
		}
		finally
		{
			cache.CollectionChanged -= onCollectionChanged;
		}
	}

	[TestMethod]
	public void ClearShouldWork()
	{
		var count = 0;

		void onCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			count += args.NewItems?.Count ?? 0;
			count += args.OldItems?.Count ?? 0;
		}

		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));

		try
		{
			cache.CollectionChanged += onCollectionChanged;
			cache.Set("foo", "bar");
			cache.Set("hello", "world");

			AreEqual(2, cache.Count);
			AreEqual(2, count);

			cache.Clear();

			AreEqual(0, cache.Count);
			AreEqual(0, cache.ToArray().Length);
			AreEqual(4, count);
		}
		finally
		{
			cache.CollectionChanged -= onCollectionChanged;
		}
	}

	[TestMethod]
	public void Constructor()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache();
		IsTrue(cache.IsEmpty);
		IsTrue(cache.IsSynchronized);
		IsTrue(cache.SlidingExpiration);
		AreEqual(TimeSpan.FromMinutes(15), cache.DefaultTimeout);

		cache = new MemoryCache(TimeSpan.FromSeconds(1));
		IsTrue(cache.IsEmpty);
		IsTrue(cache.IsSynchronized);
		IsTrue(cache.SlidingExpiration);
		AreEqual(TimeSpan.FromSeconds(1), cache.DefaultTimeout);
	}

	[TestMethod]
	public void CopyTo()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var array = new MemoryCacheItem[2];
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");
		cache.Set("hello", "world");

		AreEqual(2, cache.Count);

		cache.CopyTo(array, 0);

		AreEqual(cache.ToArray(), array);
	}

	[TestMethod]
	public void EnumeratorShouldWork()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		IsTrue(cache.SlidingExpiration);

		cache.Set("foo", "bar");
		cache.Set("hello", "world");

		var actual = cache.ToArray();
		AreEqual(2, actual.Length);

		// Enumerating the cache will not change access time therefore will not effect expiration date.
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), actual[0].ExpirationDate);
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), actual[1].ExpirationDate);

		IncrementTime(seconds: 2);

		var count = 0;
		foreach (MemoryCacheItem item in (IEnumerable) cache)
		{
			count++;
			AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
		}
		AreEqual(2, count);
	}

	[TestMethod]
	public void ExpiredEntriesShouldBeCleanedUp()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");
		IncrementTime(seconds: 1);
		cache.Set("hello", "world");
		IsFalse(cache.IsEmpty);
		AreEqual(2, cache.Count);
		IncrementTime(seconds: 10);
		var internalDictionary = cache.GetMemberValue("_dictionary") as Dictionary<string, MemoryCacheItem>;
		IsNotNull(internalDictionary);
		AreEqual(2, internalDictionary?.Count);
		cache.Cleanup();
		AreEqual(0, internalDictionary?.Count);
	}

	[TestMethod]
	public void ExpiredEntriesShouldBeIgnored()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");
		IncrementTime(seconds: 1);
		cache.Set("hello", "world");
		IsFalse(cache.IsEmpty);
		AreEqual(2, cache.Count);
		IncrementTime(seconds: 10);
		IsTrue(cache.IsEmpty);
		AreEqual(0, cache.Count);
	}

	[TestMethod]
	public void RemoveShouldWork()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");
		cache.Set("hello", "world");

		AreEqual(2, cache.Count);

		var actual = cache.Remove("foo");
		IsNotNull(actual);
		AreEqual("foo", actual.Key);

		AreEqual(1, cache.Count);
		AreEqual("hello", cache.First().Key);
	}

	[TestMethod]
	public void RemoveShouldWorkWithInvalidKey()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		AreEqual(0, cache.Count);

		// Remove key is not available so [default] (null) should be returned
		var actual = cache.Remove("foo");
		IsNull(actual);
	}

	[TestMethod]
	public void SetMultipleCallsShouldUpdateNotAdd()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");

		AreEqual(1, cache.Count);
		IsTrue(cache.TryGet("foo", out var item));
		AreEqual("bar", (string) item.Value);

		IncrementTime(seconds: 1);
		cache.Set("foo", "bar2");

		AreEqual(1, cache.Count);
		IsTrue(cache.TryGet("foo", out item));
		AreEqual("bar2", (string) item.Value);
	}

	[TestMethod]
	public void SlidingExpirationDisabledShouldNotAffectItems()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10))
		{
			SlidingExpiration = false
		};
		IsFalse(cache.SlidingExpiration);
		AreEqual(0, cache.Count);

		cache.Set("foo", "bar");
		IsTrue(cache.HasKey("foo"));
		IsTrue(cache.TryGet("foo", out var item));
		AreEqual("bar", (string) item.Value);
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
		AreEqual(1, cache.Count);

		// Accessing the value should not bump expiration
		IncrementTime(seconds: 5);
		IsTrue(cache.HasKey("foo"));
		IsTrue(cache.TryGet("foo", out item));
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);
		IsFalse(item.HasExpired);
		AreEqual(1, cache.Count);

		// Bump to expiration date
		IncrementTime(seconds: 5);
		IsFalse(cache.HasKey("foo"));
		IsFalse(cache.TryGet("foo", out item));
		IsNull(item);
		AreEqual(0, cache.Count);
	}

	[TestMethod]
	public void SlidingExpirationShouldWork()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		IsTrue(cache.SlidingExpiration);

		cache.Set("foo", "bar");
		IsTrue(cache.TryGet("foo", out var item));
		AreEqual("bar", (string) item.Value);
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 10), item.ExpirationDate);

		// Accessing the value should bump expiration
		IncrementTime(seconds: 5);
		cache.TryGet("foo", out item);
		AreEqual(new DateTime(2022, 09, 17, 09, 36, 15), item.ExpirationDate);
		IsFalse(item.HasExpired);

		// Bump to expiration date
		IncrementTime(seconds: 10);
		IsFalse(cache.TryGet("foo", out item));
		IsNull(item);
	}

	[TestMethod]
	public void TryGetInvalidKey()
	{
		SetTime(new DateTime(2022, 09, 17, 09, 36, 00));
		var cache = new MemoryCache(TimeSpan.FromSeconds(10));
		cache.Set("foo", "bar");
		cache.Set("hello", "world");

		AreEqual(2, cache.Count);

		var actual = cache.TryGet("aoeu", out var item);
		IsFalse(actual);
		IsNull(item);
	}

	#endregion
}