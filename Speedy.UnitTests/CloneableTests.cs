#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Profiling;
using Speedy.Sync;
using Speedy.Website.Data.Entities;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class CloneableTests
	{
		#region Methods

		[TestMethod]
		public void DatabaseOptionsShouldClone()
		{
			var testItems = new[]
			{
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = true, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = true, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = true, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = true, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = true, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = new[] { "foo" }, Timeout = TimeSpan.FromSeconds(30), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromTicks(1), UnmaintainedEntities = Array.Empty<Type>() },
				new DatabaseOptions { DisableEntityValidations = false, MaintainCreatedOn = false, MaintainModifiedOn = false, MaintainSyncId = false, PermanentSyncEntityDeletions = false, SyncOrder = Array.Empty<string>(), Timeout = TimeSpan.FromTicks(1), UnmaintainedEntities = new[] { typeof(ISyncEntity) } }
			};

			BaseShouldCloneTest(testItems);
		}

		[TestMethod]
		public void SyncClientOptionsShouldClone()
		{
			var testItems = new[]
			{
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = true, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = false },
				new SyncClientOptions { EnablePrimaryKeyCache = false, IsServerClient = true }
			};

			BaseShouldCloneTest(testItems);
		}

		[TestMethod]
		public void SyncEngineStateShouldClone()
		{
			var testItems = new[]
			{
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 1, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = "123", Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Starting, Total = 0 },
				new SyncEngineState { Count = 0, Message = string.Empty, Status = SyncEngineStatus.Cancelled, Total = 1 }
			};

			BaseShouldCloneTest(testItems);
		}

		[TestMethod]
		public void SyncOptionsShouldClone()
		{
			var testItems = new[]
			{
				new SyncOptions { IncludeIssueDetails = false, ItemsPerSyncRequest = 0, LastSyncedOnClient = DateTime.MinValue, LastSyncedOnServer = DateTime.MinValue, PermanentDeletions = false, Values = new Dictionary<string, string>() },
				new SyncOptions { IncludeIssueDetails = true, ItemsPerSyncRequest = 0, LastSyncedOnClient = DateTime.MinValue, LastSyncedOnServer = DateTime.MinValue, PermanentDeletions = false, Values = new Dictionary<string, string>() },
				new SyncOptions { IncludeIssueDetails = false, ItemsPerSyncRequest = 1, LastSyncedOnClient = DateTime.MinValue, LastSyncedOnServer = DateTime.MinValue, PermanentDeletions = false, Values = new Dictionary<string, string>() },
				new SyncOptions { IncludeIssueDetails = false, ItemsPerSyncRequest = 0, LastSyncedOnClient = DateTime.MaxValue, LastSyncedOnServer = DateTime.MinValue, PermanentDeletions = false, Values = new Dictionary<string, string>() },
				new SyncOptions { IncludeIssueDetails = false, ItemsPerSyncRequest = 0, LastSyncedOnClient = DateTime.MinValue, LastSyncedOnServer = DateTime.MinValue, PermanentDeletions = false, Values = new Dictionary<string, string> { { "foo", "bar" } } }
			};

			testItems[0].AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>(x => !x.IsDeleted));

			BaseShouldCloneTest(testItems, (e, clone) =>
			{
				var expected = e.ShouldExcludeRepository(typeof(AddressEntity));
				var actual = clone.ShouldExcludeRepository(typeof(AddressEntity));
				TestHelper.AreEqual(expected, actual);
			});
		}

		private static void BaseShouldCloneTest<T>(IEnumerable<T> testItems, Action<T, T> additionalDeepCloneValidations = null) where T : ICloneable
		{
			foreach (var testItem in testItems)
			{
				// ReSharper disable once InvokeAsExtensionMethod
				var (deepClone, timer2) = Timer.Create(() => (T) testItem.DeepClone());
				var (shallowClone, timer3) = Timer.Create(() => (T) testItem.ShallowClone());

				TestHelper.AreEqual(testItem, deepClone);
				TestHelper.AreEqual(testItem, shallowClone);

				// Shallow clone will not work with deep clone validations
				additionalDeepCloneValidations?.Invoke(testItem, deepClone);
			}
		}

		#endregion
	}
}