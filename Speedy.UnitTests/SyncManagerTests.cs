#region References

using System;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Sync;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class SyncManagerTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void SyncShouldCallPostUpdateEvenIfSyncDoesNotRun()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			var syncPostUpdateCalled = false;
			using var logListener = LogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			manager.SyncAsync(TimeSpan.FromMilliseconds(30000), null, results => firstSyncOptions = results.Options);
			manager.WaitForSyncToStart(TimeSpan.FromMilliseconds(3000));
			Assert.IsTrue(manager.IsRunning, "The sync manager should be running");

			manager.SyncAccounts(null, null, results =>
			{
				syncPostUpdateCalled = true;
				secondSyncOptions = results?.Options;
			});

			Assert.IsTrue(syncPostUpdateCalled, "The sync postUpdate was not called as expected");
			Assert.IsTrue(manager.IsRunning, "The sync manager should still be running");

			manager.CancelSync();
			Assert.IsTrue(manager.IsCancellationPending, "The sync should be pending cancellation");

			manager.WaitForSyncToComplete();
			Assert.IsFalse(manager.IsRunning, "The sync manager should not be running because it was cancelled");

			Assert.IsNotNull(firstSyncOptions, "First sync options should not be null");
			Assert.IsNull(secondSyncOptions, "Second sync options should be null");

			var expected = new[]
			{
				"Sync All started",
				"Sync All is already running so Sync Accounts not started.",
				"Cancelling running Sync All...",
				"Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"Sync All stopped. 00:00.000"
			};

			var actual = logListener.Events.Select(x => x.GetMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
			Assert.AreEqual(1, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].FailedSyncs);
		}

		[TestMethod]
		public void SyncShouldNotLogIfNotVerbose()
		{
			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			using var logListener = LogListener.CreateSession(manager.SessionId, EventLevel.Informational);

			manager.SyncAccounts();
			manager.WaitForSyncToComplete();

			var expected = Array.Empty<string>();
			var actual = logListener.Events.Select(x => x.GetMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(1, manager.SyncTimers[TestSyncType.Accounts].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.Accounts].FailedSyncs);
		}

		[TestMethod]
		public void SyncsShouldNotAverageIfCancelled()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			using var logListener = LogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Elapsed.Ticks);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Samples);

			// Start a sync that will run for a long while
			var result1 = manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), null, results => secondSyncOptions = results?.Options);
			var result2 = manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, results => firstSyncOptions = results?.Options);
			manager.WaitForSyncToComplete();

			var expected = new[]
			{
				"4/23/2020 1:55:24 AM Verbose : Sync Accounts started",
				"4/23/2020 1:55:25 AM Verbose : Sync Accounts is already running so Sync All not started.",
				"4/23/2020 1:55:26 AM Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 1:55:41 AM Verbose : Sync Accounts stopped. 00:17.000",
			};

			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Elapsed.Ticks);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Samples);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].FailedSyncs);

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));
			var actualResult1 = result1.AwaitResults();
			var actualResult2 = result2.AwaitResults();

			TestHelper.AreEqual(expected, actual);
			Assert.IsTrue(actualResult1.SyncSuccessful, "Sync should have been successful");
			Assert.AreEqual(SyncResultStatus.Unknown, actualResult2.SyncStatus, "Sync should not have started");
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].FailedSyncs);
		}

		[TestMethod]
		public void SyncsShouldNotWaitForEachOther()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;
			SyncOptions thirdSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			using var logListener = LogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			var result1 = manager.SyncAsync(TimeSpan.FromMilliseconds(1000), null, results => firstSyncOptions = results?.Options);
			var result2 = manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), null, results => secondSyncOptions = results?.Options);
			var result3 = manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), null, results => thirdSyncOptions = results?.Options);
			manager.WaitForSyncToComplete();
			var actualResult1 = result1.AwaitResults();
			var actualResult2 = result2.AwaitResults();
			var actualResult3 = result3.AwaitResults();

			var expected = new[]
			{
				"4/23/2020 1:55:24 AM Verbose : Sync All started",
				"4/23/2020 1:55:25 AM Verbose : Sync All is already running so Sync Accounts not started.",
				"4/23/2020 1:55:26 AM Verbose : Sync All is already running so Sync Addresses not started.",
				"4/23/2020 1:55:27 AM Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 1:55:42 AM Verbose : Sync All stopped. 00:18.000"
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.IsTrue(actualResult1.SyncSuccessful, "Sync should have been successful");
			Assert.AreEqual(SyncResultStatus.Unknown, actualResult2.SyncStatus, "Sync should not have started");
			Assert.AreEqual(SyncResultStatus.Unknown, actualResult3.SyncStatus, "Sync should not have started");
			Assert.AreEqual(18000, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
		}

		[TestMethod]
		public void SyncsShouldWaitForEachOther()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;
			SyncOptions thirdSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () => startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			using var logListener = LogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, results => firstSyncOptions = results?.Options);
			manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), results => secondSyncOptions = results?.Options);
			manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), results => thirdSyncOptions = results?.Options);

			while (thirdSyncOptions == null || manager.IsRunning)
			{
				Thread.Sleep(10);
			}

			var expected = new[]
			{
				"4/23/2020 1:55:24 AM Verbose : Sync All started",
				"4/23/2020 1:55:25 AM Verbose : Waiting for Sync All to complete...",
				"4/23/2020 1:55:26 AM Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 1:55:41 AM Verbose : Sync All stopped. 00:17.000",
				"4/23/2020 1:55:43 AM Verbose : Sync Accounts started",
				"4/23/2020 1:55:44 AM Verbose : Waiting for Sync Accounts to complete...",
				"4/23/2020 1:55:45 AM Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 1:56:00 AM Verbose : Sync Accounts stopped. 00:17.000",
				"4/23/2020 1:56:01 AM Verbose : Sync Addresses started",
				"4/23/2020 1:56:02 AM Verbose : Syncing Addresses for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 1:56:16 AM Verbose : Sync Addresses stopped",
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(17000, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
		}

		#endregion
	}
}