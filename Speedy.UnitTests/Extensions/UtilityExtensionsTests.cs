#region References

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.UnitTests.Factories;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class UtilityExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void RetryShouldCompleteWithoutException()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			UtilityExtensions.Retry(() =>
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
				UtilityExtensions.Retry(() =>
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
			var result = UtilityExtensions.Retry(() =>
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
				result = UtilityExtensions.Retry(() =>
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
		public void UpdateIf()
		{
			var account = new AccountEntity();
			Assert.AreEqual(null, account.Name);

			account.UpdateIf(x => x.Name == null, x => x.Name = "John");
			Assert.AreEqual("John", account.Name);
		}

		[TestMethod]
		public void UpdateShouldUpdateAllMembers()
		{
			var destination = new AccountEntity();
			var source = EntityFactory.GetAccount();

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
			Assert.AreNotEqual(destination.Pets, source.Pets);
			Assert.AreNotEqual(destination.SyncId, source.SyncId);

			// Update all members that are not virtual
			UtilityExtensions.UpdateWith(destination, source);

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
			Assert.AreNotEqual(destination.Pets, source.Pets);
			Assert.AreEqual(destination.SyncId, source.SyncId);

			// Update all members that are not virtual
			destination.UpdateWith(source, false);

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
			Assert.AreEqual(destination.Pets, source.Pets);
			Assert.AreEqual(destination.SyncId, source.SyncId);
		}

		[TestMethod]
		public void WaitShouldCompleteWhenConditionIsSatisfied()
		{
			var count = 0;
			var watch = Stopwatch.StartNew();
			var result = UtilityExtensions.Wait(() => ++count > 3, 1000, 50);
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
			var result = UtilityExtensions.Wait(() => ++count > 3, 500, 10);
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
			var result = UtilityExtensions.Wait(() => ++count > 100, 1000, 50);
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
			var result = UtilityExtensions.Wait(() => ++count > 100, 500, 100);
			watch.Stop();
			Assert.IsFalse(result);
			Assert.AreEqual(5, count);
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds > 500, watch.Elapsed.TotalMilliseconds.ToString());
			Assert.IsTrue(watch.Elapsed.TotalMilliseconds < 510, watch.Elapsed.TotalMilliseconds.ToString());
		}

		#endregion
	}
}