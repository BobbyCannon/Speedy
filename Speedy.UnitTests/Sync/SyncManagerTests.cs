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

namespace Speedy.UnitTests.Sync
{
	/// <summary>
	/// todo: we have to fix log order...
	/// </summary>
	[TestClass]
	public class SyncManagerTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void SyncShouldCallPostUpdateEvenIfSyncDoesNotRun()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 22, DateTimeKind.Utc);

			TimeService.AddUtcNowProvider(() => startTime += TimeSpan.FromSeconds(1));

			var manager = new TestSyncManager();
			var syncPostUpdateCalled = false;
			using var logListener = MemoryLogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			var resultsForSync = manager.SyncAsync(TimeSpan.FromSeconds(30), null, results => firstSyncOptions = results.Options);
			var started = manager.WaitForSyncToStart(TimeSpan.FromMilliseconds(150));
			Assert.IsTrue(started, "The sync manager never started?");
			var isRunning = manager.WaitForSyncToStartRunning(TimeSpan.FromMilliseconds(1000));
			Assert.IsTrue(isRunning, "The sync manager never started running?");
			Assert.IsTrue(manager.IsRunning, "The sync manager should be running");

			manager.SyncAccounts(null, null, x =>
			{
				syncPostUpdateCalled = true;
				secondSyncOptions = x?.Options;
			});

			Assert.IsTrue(syncPostUpdateCalled, "The sync postUpdate was not called as expected");
			Assert.IsTrue(manager.IsRunning, "The sync manager should still be running");

			manager.CancelSync();
			manager.WaitForSyncToComplete();
			Assert.IsFalse(manager.IsRunning, "The sync manager should not be running because it was cancelled");
			Assert.IsTrue(resultsForSync.Result.SyncCancelled, "The sync manager should have been cancelled");

			Assert.IsNotNull(firstSyncOptions, "First sync options should not be null");
			Assert.IsNull(secondSyncOptions, "Second sync options should be null");

			var expected = new[]
			{
				new[]
				{
					$"4/23/2020 1:55:23 AM - {manager.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {manager.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:26 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Accounts not started.",
					$"4/23/2020 1:55:27 AM - {manager.SessionId} Verbose : Cancelling running Sync All...",
					$"4/23/2020 1:55:28 AM - {manager.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:31 AM - {manager.SessionId} Verbose : Changing status to Cancelled.",
					$"4/23/2020 1:55:32 AM - {manager.SessionId} Verbose : Sync All stopped. 00:00.000"
				},
				new[]
				{
					$"4/23/2020 1:55:23 AM - {manager.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {manager.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:26 AM - {manager.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:27 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Accounts not started.",
					$"4/23/2020 1:55:28 AM - {manager.SessionId} Verbose : Cancelling running Sync All...",
					$"4/23/2020 1:55:31 AM - {manager.SessionId} Verbose : Changing status to Cancelled.",
					$"4/23/2020 1:55:32 AM - {manager.SessionId} Verbose : Sync All stopped. 00:00.000"
				}
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"$\"{x.Replace(manager.SessionId.ToString(), "{manager.SessionId}")}\","));
			var result = expected.Any(x => TestHelper.Compare(x, actual).AreEqual);
			Assert.IsTrue(result, "The log does not match one of the possible orders...");
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

			TestHelper.CurrentTime = startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			using var logListener = MemoryLogListener.CreateSession(manager.SessionId, EventLevel.Informational);

			manager.SyncAccounts();
			manager.WaitForSyncToComplete();

			var expected = Array.Empty<string>();
			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"$\"{x.Replace(manager.SessionId.ToString(), "{manager.SessionId}")}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(1, manager.SyncTimers[TestSyncType.Accounts].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.Accounts].FailedSyncs);
		}

		[TestMethod]
		public void SyncsShouldNotAverageIfCancelled()
		{
			var startTime = new DateTime(2020, 04, 23, 01, 55, 22, DateTimeKind.Utc);

			TimeService.AddUtcNowProvider(() => startTime += TimeSpan.FromSeconds(1));

			var manager = new TestSyncManager();
			using var logListener = MemoryLogListener.CreateSession(Guid.Empty, EventLevel.Verbose);

			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Elapsed.Ticks);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Samples);

			// Start a sync that will run for a long while
			var result1 = manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(150), null, _ => { });
			var result2 = manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, _ => { });
			manager.WaitForSyncToComplete();

			var expected = new[]
			{
				$"4/23/2020 1:55:23 AM - {manager.SessionId} Verbose : Sync Accounts started",
				$"4/23/2020 1:55:25 AM - {manager.SessionId} Verbose : Sync Accounts is already running so Sync All not started.",
				$"4/23/2020 1:55:26 AM - {manager.SessionId} Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				$"4/23/2020 1:55:27 AM - {manager.SessionId} Verbose : Changing status to Starting.",
				$"4/23/2020 1:55:30 AM - {manager.SessionId} Verbose : Changing status to Pulling.",
				$"4/23/2020 1:55:32 AM - {manager.SessionId} Verbose : Server to Client.",
				$"4/23/2020 1:55:33 AM - {manager.SessionId} Verbose : Changing status to Pushing.",
				$"4/23/2020 1:55:35 AM - {manager.SessionId} Verbose : Client to Server.",
				$"4/23/2020 1:55:36 AM - {manager.SessionId} Verbose : Changing status to Completed.",
				$"4/23/2020 1:55:38 AM - {manager.SessionId} Verbose : Sync Accounts stopped. 00:13.000"
			};

			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Elapsed.Ticks);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].Samples);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].CancelledSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].SuccessfulSyncs);
			Assert.AreEqual(0, manager.SyncTimers[TestSyncType.All].FailedSyncs);

			var actualResult1 = result1.AwaitResults();
			var actualResult2 = result2.AwaitResults();

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"$\"{x.Replace(manager.SessionId.ToString(), "{manager.SessionId}")}\","));

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
			var startTime = new DateTime(2020, 04, 23, 01, 55, 22, DateTimeKind.Utc);

			TimeService.AddUtcNowProvider(() => startTime += TimeSpan.FromSeconds(1));

			var manager = new TestSyncManager();
			using var logListener = MemoryLogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			var result1 = manager.SyncAsync(TimeSpan.FromMilliseconds(1000), null, _ => { });
			var result2 = manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), null, _ => { });
			var result3 = manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), null, _ => { });
			manager.WaitForSyncToComplete();
			var actualResult1 = result1.AwaitResults();
			var actualResult2 = result2.AwaitResults();
			var actualResult3 = result3.AwaitResults();

			var expected = new[]
			{
				new[]
				{
					$"4/23/2020 1:55:23 AM - {manager.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Accounts not started.",
					$"4/23/2020 1:55:26 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Addresses not started.",
					$"4/23/2020 1:55:27 AM - {manager.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:28 AM - {manager.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:31 AM - {manager.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:33 AM - {manager.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:34 AM - {manager.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:36 AM - {manager.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:37 AM - {manager.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:39 AM - {manager.SessionId} Verbose : Sync All stopped. 00:14.000"
				},
				new[]
				{
					$"4/23/2020 1:55:23 AM - {manager.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Accounts not started.",
					$"4/23/2020 1:55:26 AM - {manager.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:27 AM - {manager.SessionId} Verbose : Sync All is already running so Sync Addresses not started.",
					$"4/23/2020 1:55:28 AM - {manager.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:31 AM - {manager.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:33 AM - {manager.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:34 AM - {manager.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:36 AM - {manager.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:37 AM - {manager.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:39 AM - {manager.SessionId} Verbose : Sync All stopped. 00:14.000"
				}
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"$\"{x.Replace(manager.SessionId.ToString(), "{manager.SessionId}")}\","));

			var result = expected.Any(x => TestHelper.Compare(x, actual).AreEqual);
			Assert.IsTrue(result, "The log does not match one of the possible orders...");
			Assert.IsTrue(actualResult1.SyncSuccessful, "Sync should have been successful");
			Assert.AreEqual(SyncResultStatus.Unknown, actualResult2.SyncStatus, "Sync should not have started");
			Assert.AreEqual(SyncResultStatus.Unknown, actualResult3.SyncStatus, "Sync should not have started");
			Assert.AreEqual(14000, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
		}

		[TestMethod]
		public void SyncsShouldWaitForEachOther()
		{
			SyncOptions thirdSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 22, DateTimeKind.Utc);

			TimeService.AddUtcNowProvider(() => startTime += TimeSpan.FromSeconds(1));

			var manager = new TestSyncManager();
			using var logListener = MemoryLogListener.CreateSession(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			var result1 = manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, _ => { });
			var result2 = manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), _ => { });
			var result3 = manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), results => thirdSyncOptions = results?.Options);

			while ((thirdSyncOptions == null) || manager.IsRunning)
			{
				Thread.Sleep(10);
			}

			// todo: When waiting, print what sync is waiting for what?

			var expected = new[]
			{
				new[]
				{
					$"4/23/2020 1:55:23 AM - {result1.Result.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {result1.Result.SessionId} Verbose : Waiting for Sync All to complete...",
					$"4/23/2020 1:55:26 AM - {result1.Result.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:27 AM - {result1.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:30 AM - {result1.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:32 AM - {result1.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:33 AM - {result1.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:35 AM - {result1.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:36 AM - {result1.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:38 AM - {result1.Result.SessionId} Verbose : Sync All stopped. 00:13.000",
					$"4/23/2020 1:55:39 AM - {result2.Result.SessionId} Verbose : Sync Accounts started",
					$"4/23/2020 1:55:41 AM - {result2.Result.SessionId} Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:42 AM - {result2.Result.SessionId} Verbose : Waiting for Sync Accounts to complete...",
					$"4/23/2020 1:55:43 AM - {result2.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:46 AM - {result2.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:48 AM - {result2.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:49 AM - {result2.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:51 AM - {result2.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:52 AM - {result2.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:54 AM - {result2.Result.SessionId} Verbose : Sync Accounts stopped. 00:13.000",
					$"4/23/2020 1:55:55 AM - {result3.Result.SessionId} Verbose : Sync Addresses started",
					$"4/23/2020 1:55:56 AM - {result3.Result.SessionId} Verbose : Syncing Addresses for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:57 AM - {result3.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:56:00 AM - {result3.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:56:02 AM - {result3.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:56:03 AM - {result3.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:56:05 AM - {result3.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:56:06 AM - {result3.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:56:07 AM - {result3.Result.SessionId} Verbose : Sync Addresses stopped"
				},
				new[]
				{
					$"4/23/2020 1:55:23 AM - {result1.Result.SessionId} Verbose : Sync All started",
					$"4/23/2020 1:55:25 AM - {result1.Result.SessionId} Verbose : Waiting for Sync All to complete...",
					$"4/23/2020 1:55:26 AM - {result1.Result.SessionId} Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:27 AM - {result1.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:30 AM - {result1.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:32 AM - {result1.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:33 AM - {result1.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:35 AM - {result1.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:36 AM - {result1.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:38 AM - {result1.Result.SessionId} Verbose : Sync All stopped. 00:13.000",
					$"4/23/2020 1:55:39 AM - {result2.Result.SessionId} Verbose : Sync Accounts started",
					$"4/23/2020 1:55:41 AM - {result2.Result.SessionId} Verbose : Waiting for Sync Accounts to complete...",
					$"4/23/2020 1:55:42 AM - {result2.Result.SessionId} Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:43 AM - {result2.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:55:46 AM - {result2.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:55:48 AM - {result2.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:55:49 AM - {result2.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:55:51 AM - {result2.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:55:52 AM - {result2.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:55:54 AM - {result2.Result.SessionId} Verbose : Sync Accounts stopped. 00:13.000",
					$"4/23/2020 1:55:55 AM - {result3.Result.SessionId} Verbose : Sync Addresses started",
					$"4/23/2020 1:55:56 AM - {result3.Result.SessionId} Verbose : Syncing Addresses for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
					$"4/23/2020 1:55:57 AM - {result3.Result.SessionId} Verbose : Changing status to Starting.",
					$"4/23/2020 1:56:00 AM - {result3.Result.SessionId} Verbose : Changing status to Pulling.",
					$"4/23/2020 1:56:02 AM - {result3.Result.SessionId} Verbose : Server to Client.",
					$"4/23/2020 1:56:03 AM - {result3.Result.SessionId} Verbose : Changing status to Pushing.",
					$"4/23/2020 1:56:05 AM - {result3.Result.SessionId} Verbose : Client to Server.",
					$"4/23/2020 1:56:06 AM - {result3.Result.SessionId} Verbose : Changing status to Completed.",
					$"4/23/2020 1:56:07 AM - {result3.Result.SessionId} Verbose : Sync Addresses stopped"
				}
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x =>
			{
				var message = x.Replace(result1.Result.SessionId.ToString(), "{result1.Result.SessionId}")
					.Replace(result2.Result.SessionId.ToString(), "{result2.Result.SessionId}")
					.Replace(result3.Result.SessionId.ToString(), "{result3.Result.SessionId}");

				Console.WriteLine($"$\"{message}\",");
			});

			var result = expected.Any(x => TestHelper.Compare(x, actual).AreEqual);
			Assert.IsTrue(result, "The log does not match one of the possible orders...");
			Assert.AreEqual(13000, manager.SyncTimers[TestSyncType.All].Average.TotalMilliseconds);
		}

		#endregion
	}
}