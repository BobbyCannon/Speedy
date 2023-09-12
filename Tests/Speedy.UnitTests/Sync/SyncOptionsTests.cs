#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Sync;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Sync;

[TestClass]
public class SyncOptionsTests : CloneableSpeedUnitTests
{
	#region Methods

	[TestMethod]
	public void AllPropertiesSet()
	{
		ValidateAllValuesAreNotDefault(Activator.CreateInstanceWithNonDefaultValues<SyncOptions>());
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
			AreEqual(expected, actual);
		});
	}

	#endregion
}