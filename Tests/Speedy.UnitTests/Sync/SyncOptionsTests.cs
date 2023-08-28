#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Sync;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncOptionsTests : SpeedyUnitTest<SyncOptions>
	{
		#region Methods

		[TestMethod]
		public void AllPropertiesSet()
		{
			ValidateAllValuesAreNotDefault(GetModelWithNonDefaultValues());
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

			CloneableHelper.BaseShouldCloneTest(testItems, (e, clone) =>
			{
				var expected = e.ShouldExcludeRepository(typeof(AddressEntity));
				var actual = clone.ShouldExcludeRepository(typeof(AddressEntity));
				TestHelper.AreEqual(expected, actual);
			});
		}

		#endregion
	}
}