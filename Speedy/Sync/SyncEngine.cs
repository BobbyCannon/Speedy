#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync engine.
	/// </summary>
	public class SyncEngine
	{
		#region Fields

		private bool _cancelPending;
		private readonly List<SyncIssue> _syncIssues;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the sync engine.
		/// </summary>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="lastSyncedOn"> The last date and time the database synced. </param>
		public SyncEngine(ISyncClient client, ISyncClient server, DateTime lastSyncedOn)
		{
			_cancelPending = false;
			_syncIssues = new List<SyncIssue>();

			Client = client;
			Server = server;
			LastSyncedOn = lastSyncedOn;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The client.
		/// </summary>
		public ISyncClient Client { get; }

		/// <summary>
		/// Gets or sets the last synced on date and time.
		/// </summary>
		public DateTime LastSyncedOn { get; set; }

		/// <summary>
		/// The server.
		/// </summary>
		public ISyncClient Server { get; }

		/// <summary>
		/// Gets or sets the start date and time.
		/// </summary>
		public DateTime StartTime { get; set; }

		/// <summary>
		/// Current status of the sync engine.
		/// </summary>
		public SyncEngineStatus Status { get; set; }

		/// <summary>
		/// Gets the list of issues that happened during syncing.
		/// </summary>
		public IReadOnlyList<SyncIssue> SyncIssues => new ReadOnlyCollection<SyncIssue>(_syncIssues);

		#endregion

		#region Methods

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public void Run()
		{
			_cancelPending = false;
			_syncIssues.Clear();

			Status = SyncEngineStatus.Starting;
			NotifyOfStatusChange();

			StartTime = DateTime.UtcNow;

			if (!_cancelPending)
			{
				Status = SyncEngineStatus.Pulling;
				Process(Server, Client, StartTime);
			}

			if (!_cancelPending)
			{
				Status = SyncEngineStatus.Pushing;
				Process(Client, Server, DateTime.UtcNow);
			}

			Status = SyncEngineStatus.Stopped;
			NotifyOfStatusChange();
		}

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public async void RunAsync()
		{
			Status = SyncEngineStatus.Starting;
			await Extensions.Wrap(Run);
		}

		/// <summary>
		/// Stops the sync process.
		/// </summary>
		public void Stop()
		{
			_cancelPending = true;
			var timeOut = DateTime.UtcNow.AddSeconds(30);

			while (Status != SyncEngineStatus.Stopped && DateTime.UtcNow <= timeOut)
			{
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Notify that the global status has changed.
		/// </summary>
		private void NotifyOfStatusChange()
		{
			OnSyncStatusChanged(new SyncEngineStatusArgs { Name = Client.Name, Count = -1, Total = -1, Status = Status });
			OnSyncStatusChanged(new SyncEngineStatusArgs { Name = Server.Name, Count = -1, Total = -1, Status = Status });
		}

		private void OnSyncStatusChanged(SyncEngineStatusArgs args)
		{
			SyncStatusChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Get changes from one client and apply them to another client.
		/// </summary>
		/// <param name="getClient"> The client to get changes from. </param>
		/// <param name="applyClient"> The client to apply change to. </param>
		/// <param name="until"> The end date and time to get changes for. </param>
		private void Process(ISyncClient getClient, ISyncClient applyClient, DateTime until)
		{
			List<SyncObject> syncObjects;
			var issues = new List<SyncIssue>();
			var request = new SyncRequest { Since = LastSyncedOn, Until = until, Skip = 0, Take = 1 };
			var total = getClient.GetChangeCount(request);

			do
			{
				syncObjects = getClient.GetChanges(request).ToList();
				issues.AddRange(applyClient.ApplyChanges(syncObjects));
				OnSyncStatusChanged(new SyncEngineStatusArgs { Name = applyClient.Name, Count = request.Skip, Total = total, Status = Status });
				request.Skip += syncObjects.Count;
			} while (!_cancelPending && syncObjects.Count > 0 && request.Skip < total);

			while (!_cancelPending && issues.Any())
			{
				var issuesToProcess = issues.Take(1).ToList();
				syncObjects = applyClient.GetCorrections(issuesToProcess).ToList();
				_syncIssues.AddRange(getClient.ApplyCorrections(syncObjects));
				issuesToProcess.ForEach(x => issues.Remove(x));
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Notifies when the sync status changes.
		/// </summary>
		public event EventHandler<SyncEngineStatusArgs> SyncStatusChanged;

		#endregion
	}
}