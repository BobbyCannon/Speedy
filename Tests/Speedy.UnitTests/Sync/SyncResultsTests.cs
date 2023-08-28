#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests.Sync
{
	[TestClass]
	public class SyncResultsTests
	{
		#region Methods

		[TestMethod]
		public void PropertyDefaultsThenSet()
		{
			var scenarios = new Dictionary<SyncResultStatus, Action<SyncResults<SyncType>>>
			{
				{ SyncResultStatus.Cancelled, x => x.SyncCancelled = true },
				{ SyncResultStatus.Completed, x => x.SyncCompleted = true },
				{ SyncResultStatus.Started, x => x.SyncStarted = true },
				{ SyncResultStatus.Successful, x => x.SyncSuccessful = true }
			};

			foreach (var scenario in scenarios)
			{
				var actual = new SyncResults<SyncType>();
				Assert.AreEqual(false, actual.SyncCancelled);
				Assert.AreEqual(false, actual.SyncStatus.HasFlag(SyncResultStatus.Cancelled));
				Assert.AreEqual(false, actual.SyncCompleted);
				Assert.AreEqual(false, actual.SyncStatus.HasFlag(SyncResultStatus.Completed));
				Assert.AreEqual(false, actual.SyncStarted);
				Assert.AreEqual(false, actual.SyncStatus.HasFlag(SyncResultStatus.Started));
				Assert.AreEqual(false, actual.SyncSuccessful);
				Assert.AreEqual(false, actual.SyncStatus.HasFlag(SyncResultStatus.Successful));
				Assert.AreEqual(SyncType.All, actual.SyncType);

				scenario.Value(actual);

				Assert.AreEqual(scenario.Key == SyncResultStatus.Cancelled, actual.SyncCancelled);
				Assert.AreEqual(scenario.Key == SyncResultStatus.Cancelled, actual.SyncStatus.HasFlag(SyncResultStatus.Cancelled));
				Assert.AreEqual(scenario.Key == SyncResultStatus.Completed, actual.SyncCompleted);
				Assert.AreEqual(scenario.Key == SyncResultStatus.Completed, actual.SyncStatus.HasFlag(SyncResultStatus.Completed));
				Assert.AreEqual(scenario.Key == SyncResultStatus.Started, actual.SyncStarted);
				Assert.AreEqual(scenario.Key == SyncResultStatus.Started, actual.SyncStatus.HasFlag(SyncResultStatus.Started));
				Assert.AreEqual(scenario.Key == SyncResultStatus.Successful, actual.SyncSuccessful);
				Assert.AreEqual(scenario.Key == SyncResultStatus.Successful, actual.SyncStatus.HasFlag(SyncResultStatus.Successful));
			}
		}

		#endregion
	}
}