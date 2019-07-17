#region References

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Tests.EntityFactories;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class ExtensionTests
	{
		#region Methods

		[TestMethod]
		public void RetryShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			Extensions.Retry(() =>
			{
				if (++count < 3)
				{
					throw new Exception("Nope");
				}
			}, 1000, 50);
			watch.Stop();
			Assert.AreEqual(3, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryShouldTimeoutAndThrowLastException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();

			TestHelper.ExpectedException<Exception>(() =>
				Extensions.Retry(() =>
				{
					if (++count < 3)
					{
						throw new Exception("Nope..." + count);
					}
				}, 100, 50), "Nope...2");

			watch.Stop();
			Assert.AreEqual(2, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryTypedShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Retry(() =>
			{
				if (++count < 3)
				{
					throw new Exception("Nope");
				}

				return count;
			}, 1000, 50);
			watch.Stop();
			Assert.AreEqual(3, result);
			Assert.AreEqual(3, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void RetryTypeShouldTimeoutAndThrowLastException()
		{
			var count = 0;
			var result = 0;
			var watch = Stopwatch.StartNew();

			TestHelper.ExpectedException<Exception>(() =>
				result = Extensions.Retry(() =>
				{
					if (++count < 3)
					{
						throw new Exception("Nope..." + count);
					}

					return count;
				}, 100, 50), "Nope...2");

			watch.Stop();
			Assert.AreEqual(0, result);
			Assert.AreEqual(2, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 100, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 150, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void ToJsonCamelCaseParameter()
		{
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc);
			var logEvent = LogEventFactory.Get(x =>
			{
				x.Id = "098e05d6-8086-402a-acd7-56cf6bfb80fc";
				x.Message = "Hello";
			});

			// First use the default values
			var expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":\"098e05d6-8086-402a-acd7-56cf6bfb80fc\",\"Level\":3,\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\"}";
			var actual = logEvent.ToJson();
			Assert.AreEqual(expected, actual);

			// Now override the default
			expected = "{\"$id\":\"1\",\"createdOn\":\"2019-07-17T20:05:55Z\",\"id\":\"098e05d6-8086-402a-acd7-56cf6bfb80fc\",\"level\":3,\"message\":\"Hello\",\"modifiedOn\":\"2019-07-17T20:05:55Z\"}";
			actual = logEvent.ToJson(true);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ToJsonConvertEnumToStringParameter()
		{
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc);
			var logEvent = LogEventFactory.Get(x =>
			{
				x.Id = "098e05d6-8086-402a-acd7-56cf6bfb80fc";
				x.Message = "Hello";
			});

			// First use the default values
			var expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":\"098e05d6-8086-402a-acd7-56cf6bfb80fc\",\"Level\":3,\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\"}";
			var actual = logEvent.ToJson();
			Assert.AreEqual(expected, actual);

			// Now override the default
			expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":\"098e05d6-8086-402a-acd7-56cf6bfb80fc\",\"Level\":\"Information\",\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\"}";
			actual = logEvent.ToJson(convertEnumsToString: true);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ToJsonIgnoreNullParameter()
		{
			TimeService.UtcNowProvider = () => new DateTime(2019, 07, 17, 20, 05, 55, DateTimeKind.Utc);
			var logEvent = LogEventFactory.Get(x =>
			{
				x.Id = null;
				x.Message = "Hello";
			});

			// First use the default values
			var expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Id\":null,\"Level\":3,\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\"}";
			var actual = logEvent.ToJson();
			Assert.AreEqual(expected, actual);

			// Now override the default
			expected = "{\"$id\":\"1\",\"CreatedOn\":\"2019-07-17T20:05:55Z\",\"Level\":3,\"Message\":\"Hello\",\"ModifiedOn\":\"2019-07-17T20:05:55Z\"}";
			actual = logEvent.ToJson(ignoreNullValues: true);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ToJsonIndentParameter()
		{
			var test = new { Name = "John", Age = 21 };
			var expected = "{\"$id\":\"1\",\"Age\":21,\"Name\":\"John\"}";
			var actual = test.ToJson();
			Assert.AreEqual(expected, actual);

			expected = "{\r\n  \"$id\": \"1\",\r\n  \"Age\": 21,\r\n  \"Name\": \"John\"\r\n}";
			actual = test.ToJson(indented: true);
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UpdateShouldUpdateAllMembers()
		{
			var destination = new PersonEntity();
			var source = PersonFactory.Get();

			source.Id = 99;
			source.SyncId = Guid.NewGuid();
			source.IsDeleted = true;
			source.Address.Id = 199;
			source.AddressId = 199;

			Assert.AreNotEqual(destination.Address, source.Address);
			Assert.AreNotEqual(destination.AddressId, source.AddressId);
			Assert.AreNotEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreNotEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreNotEqual(destination.Groups, source.Groups);
			Assert.AreNotEqual(destination.Id, source.Id);
			Assert.AreNotEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreNotEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreNotEqual(destination.Name, source.Name);
			Assert.AreNotEqual(destination.Owners, source.Owners);
			Assert.AreNotEqual(destination.SyncId, source.SyncId);

			// Update all members that are not virtual
			Extensions.UpdateWith(destination, source);

			// All non virtual should be equal
			Assert.AreNotEqual(destination.Address, source.Address);
			Assert.AreEqual(destination.AddressId, source.AddressId);
			Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreNotEqual(destination.Groups, source.Groups);
			Assert.AreEqual(destination.Id, source.Id);
			Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreEqual(destination.Name, source.Name);
			Assert.AreNotEqual(destination.Owners, source.Owners);
			Assert.AreEqual(destination.SyncId, source.SyncId);

			// Update all members that are not virtual
			Extensions.UpdateWith(destination, source, false);

			// All members should be equal now
			Assert.AreEqual(destination.Address, source.Address);
			Assert.AreEqual(destination.AddressId, source.AddressId);
			Assert.AreEqual(destination.AddressSyncId, source.AddressSyncId);
			Assert.AreEqual(destination.CreatedOn, source.CreatedOn);
			Assert.AreEqual(destination.Groups, source.Groups);
			Assert.AreEqual(destination.Id, source.Id);
			Assert.AreEqual(destination.IsDeleted, source.IsDeleted);
			Assert.AreEqual(destination.ModifiedOn, source.ModifiedOn);
			Assert.AreEqual(destination.Name, source.Name);
			Assert.AreEqual(destination.Owners, source.Owners);
			Assert.AreEqual(destination.SyncId, source.SyncId);
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 3, 1000, 50);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 150, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 200, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 3, 500, 10);
			watch.Stop();
			Assert.IsTrue(result);
			Assert.AreEqual(4, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 30, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 40, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 100, 1000, 50);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(20, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 1000, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 1050, watch.Elapsed.TotalMilliseconds.ToString());
		}

		[TestMethod]
		public void WaitShouldTimeOutIfConditionIsNotSatisfiedWithCustomTimeoutAndDelay()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = Extensions.Wait(() => ++count > 100, 500, 100);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 500, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 510, watch.Elapsed.TotalMilliseconds.ToString());
		}

		#endregion
	}
}