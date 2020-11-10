#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Net;
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

			TimeService.UtcNowProvider = () =>  startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			var syncPostUpdateCalled = false;
			var logListener = new LogListener(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			manager.SyncAsync(TimeSpan.FromMilliseconds(30000), null, options => firstSyncOptions = options);
			manager.WaitForSyncToStart(TimeSpan.FromMilliseconds(3000));
			Assert.IsTrue(manager.IsRunning, "The sync manager should be running");

			manager.SyncAccounts(null, null, options => 
				{
					syncPostUpdateCalled = true;
					secondSyncOptions = options;
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
				"Sync All stopped. 00:12.000"
			};

			var actual = logListener.Events.Select(x => x.GetMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(12000, manager.AverageSyncTimeForAll.Average.TotalMilliseconds);
		}
		
		[TestMethod]
		public void SyncsShouldNotWaitForEachOther()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;
			SyncOptions thirdSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () =>  startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			var logListener = new LogListener(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			manager.SyncAsync(TimeSpan.FromMilliseconds(1000), null, options => firstSyncOptions = options);
			manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), null, options => secondSyncOptions = options);
			manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), null, options => thirdSyncOptions = options);
			manager.WaitForSyncToComplete();
			
			var expected = new[]
			{
				"4/23/2020 01:55:24 AM Verbose : Sync All started",
				"4/23/2020 01:55:25 AM Verbose : Sync All is already running so Sync Accounts not started.",
				"4/23/2020 01:55:26 AM Verbose : Sync All is already running so Sync Addresses not started.",
				"4/23/2020 01:55:27 AM Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 01:55:42 AM Verbose : Sync All stopped. 00:18.000"
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.IsTrue(manager.IsSyncSuccessful, "Sync should have been successful");
			Assert.AreEqual(18000, manager.AverageSyncTimeForAll.Average.TotalMilliseconds);
		}

		[TestMethod]
		public void SyncsShouldNotAverageIfCancelled()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () =>  startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			var logListener = new LogListener(manager.SessionId, EventLevel.Verbose);

			Assert.AreEqual(0, manager.AverageSyncTimeForAll.Elapsed.Ticks);
			Assert.AreEqual(0, manager.AverageSyncTimeForAll.Samples);

			// Start a sync that will run for a long while
			manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), null, options => secondSyncOptions = options);
			manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, options => firstSyncOptions = options);
			manager.WaitForSyncToComplete();
			
			var expected = new[]
			{
				"4/23/2020 01:55:23 AM Verbose : Sync Accounts started",
				"4/23/2020 01:55:24 AM Verbose : Sync Accounts is already running so Sync All not started.",
				"4/23/2020 01:55:25 AM Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 01:55:39 AM Verbose : Sync Accounts stopped"
			};

			Assert.AreEqual(0, manager.AverageSyncTimeForAll.Elapsed.Ticks);
			Assert.AreEqual(0, manager.AverageSyncTimeForAll.Samples);

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.IsTrue(manager.IsSyncSuccessful, "Sync should have been successful");
			Assert.AreEqual(0, manager.AverageSyncTimeForAll.Average.TotalMilliseconds);
		}
		
		[TestMethod]
		public void SyncsShouldWaitForEachOther()
		{
			SyncOptions firstSyncOptions = null;
			SyncOptions secondSyncOptions = null;
			SyncOptions thirdSyncOptions = null;

			var startTime = new DateTime(2020, 04, 23, 01, 55, 23, DateTimeKind.Utc);
			var offset = 0;

			TimeService.UtcNowProvider = () =>  startTime.AddSeconds(offset++);

			var manager = new TestSyncManager();
			var logListener = new LogListener(manager.SessionId, EventLevel.Verbose);

			// Start a sync that will run for a long while
			manager.SyncAsync(TimeSpan.FromMilliseconds(100), null, options => firstSyncOptions = options);
			manager.SyncAccountsAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), options => secondSyncOptions = options);
			manager.SyncAddressesAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), options => thirdSyncOptions = options);

			while (thirdSyncOptions == null || manager.IsRunning)
			{
				Thread.Sleep(10);
			}
			
			var expected = new[]
			{
				"4/23/2020 01:55:24 AM Verbose : Sync All started",
				"4/23/2020 01:55:25 AM Verbose : Waiting for Sync All to complete...",
				"4/23/2020 01:55:26 AM Verbose : Syncing All for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 01:55:41 AM Verbose : Sync All stopped. 00:17.000",
				"4/23/2020 01:55:42 AM Verbose : Sync Accounts started",
				"4/23/2020 01:55:43 AM Verbose : Waiting for Sync Accounts to complete...",
				"4/23/2020 01:55:44 AM Verbose : Syncing Accounts for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 01:55:58 AM Verbose : Sync Accounts stopped",
				"4/23/2020 01:55:59 AM Verbose : Sync Addresses started",
				"4/23/2020 01:56:00 AM Verbose : Syncing Addresses for 1/1/0001 12:00:00 AM, 1/1/0001 12:00:00 AM",
				"4/23/2020 01:56:14 AM Verbose : Sync Addresses stopped"
			};

			var actual = logListener.Events.Select(x => x.GetDetailedMessage()).ToArray();
			actual.ForEach(x => Console.WriteLine($"\"{x}\","));

			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(17000, manager.AverageSyncTimeForAll.Average.TotalMilliseconds);
		}

		#endregion
	}

	public enum TestSyncType
	{
		All = 0,
		Accounts = 1,
		Addresses = 2
	}

	public class TestSyncManager : SyncManager<TestSyncType>
	{
		#region Constructors

		public TestSyncManager() : this(new DefaultDispatcher())
		{
		}

		public TestSyncManager(IDispatcher dispatcher) : base(dispatcher)
		{
			SyncSystemVersion = new Version(1, 2, 3, 4);

			// Setup our sync options
			GetOrAddSyncOptions(TestSyncType.All, options => {});
			GetOrAddSyncOptions(TestSyncType.Accounts, options => {});
			GetOrAddSyncOptions(TestSyncType.Addresses, options => {});

			// Setup tracking of certain syncs
			AverageSyncTimeForAll = SyncTimers.GetOrAdd(TestSyncType.All, new AverageTimer(10, Dispatcher));
		}

		#endregion

		#region Properties

		public AverageTimer AverageSyncTimeForAll { get; }

		public override Version SyncSystemVersion { get; }

		#endregion

		#region Methods

		public void Sync(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			SyncAsync(testDelay, waitFor, postAction).Wait(waitFor ?? ProcessTimeout);
		}

		public Task SyncAsync(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			return ProcessAsync(TestSyncType.All, options => DoTestDelay(testDelay), waitFor, postAction);
		}
		
		public void SyncAccounts(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			SyncAccountsAsync(testDelay, waitFor, postAction).Wait(waitFor ?? ProcessTimeout);
		}

		public Task SyncAccountsAsync(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			return ProcessAsync(TestSyncType.Accounts, options => DoTestDelay(testDelay), waitFor, postAction);
		}

		public void SyncAddresses(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			SyncAddressesAsync(testDelay, waitFor, postAction).Wait(waitFor ?? ProcessTimeout);
		}

		public Task SyncAddressesAsync(TimeSpan? testDelay = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null)
		{
			return ProcessAsync(TestSyncType.Addresses, options => DoTestDelay(testDelay), waitFor, postAction);
		}

		protected override ISyncClient GetSyncClientForClient()
		{
			var corrections = new List<SyncObject>();
			var client = new Mock<ISyncClient>();
			var statistics = new SyncStatistics();

			client.Setup(x => x.Statistics).Returns(() => statistics);

			client.Setup(x => x.BeginSync(It.IsAny<Guid>(), It.IsAny<SyncOptions>()))
				.Returns<Guid, SyncOptions>((i, o) => new SyncSession { Id = i, StartedOn = TimeService.UtcNow });

			client.Setup(x => x.GetChanges(It.IsAny<Guid>(), It.IsAny<SyncRequest>()))
				.Returns<Guid, SyncRequest>((id, x) => new ServiceResult<SyncObject>());

			client.Setup(x => x.ApplyChanges(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) => new ServiceResult<SyncIssue>());

			client.Setup(x => x.ApplyCorrections(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) =>
				{
					corrections.AddRange(x.Collection);
					return new ServiceResult<SyncIssue>();
				});

			return client.Object;
		}

		protected override ISyncClient GetSyncClientForServer()
		{
			var corrections = new List<SyncObject>();
			var client = new Mock<ISyncClient>();
			var statistics = new SyncStatistics();

			client.Setup(x => x.Statistics).Returns(() => statistics);

			client.Setup(x => x.BeginSync(It.IsAny<Guid>(), It.IsAny<SyncOptions>()))
				.Returns<Guid, SyncOptions>((i, o) => new SyncSession { Id = i, StartedOn = TimeService.UtcNow });

			client.Setup(x => x.GetChanges(It.IsAny<Guid>(), It.IsAny<SyncRequest>()))
				.Returns<Guid, SyncRequest>((id, x) => new ServiceResult<SyncObject>());

			client.Setup(x => x.ApplyChanges(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) => new ServiceResult<SyncIssue>());

			client.Setup(x => x.ApplyCorrections(It.IsAny<Guid>(), It.IsAny<ServiceRequest<SyncObject>>()))
				.Returns<Guid, ServiceRequest<SyncObject>>((id, x) =>
				{
					corrections.AddRange(x.Collection);
					return new ServiceResult<SyncIssue>();
				});

			return client.Object;
		}

		private void DoTestDelay(TimeSpan? testDelay)
		{
			if (testDelay == null)
			{
				return;
			}

			var watch = Stopwatch.StartNew();

			while (watch.Elapsed <= testDelay.Value && !IsCancellationPending)
			{
				// Delay while getting options
				Thread.Sleep(10);
			}
		}

		#endregion
	}
}