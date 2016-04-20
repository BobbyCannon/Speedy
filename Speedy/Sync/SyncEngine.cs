#region References

using System;
using System.Collections.Generic;
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

		#endregion

		#region Methods

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public async void Run()
		{
			Status = SyncEngineStatus.Starting;

			await Extensions.Wrap(() =>
			{
				StartTime = DateTime.UtcNow;

				if (!_cancelPending)
				{
					Status = SyncEngineStatus.Pulling;
					Process(Server, Client);
				}

				if (!_cancelPending)
				{
					Status = SyncEngineStatus.Pushing;
					Process(Client, Server);
				}

				Status = SyncEngineStatus.Stopped;
			});
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

		private void OnSyncStatusChanged(SyncEngineStatusArgs args)
		{
			SyncStatusChanged?.Invoke(this, args);
		}

		/// <summary>
		/// Get changes from one client and apply them to another client.
		/// </summary>
		/// <param name="getClient"> The client to get changes from. </param>
		/// <param name="applyClient"> The client to apply change to. </param>
		private void Process(ISyncClient getClient, ISyncClient applyClient)
		{
			List<SyncObject> changes;
			var request = new SyncRequest { Since = LastSyncedOn, Until = StartTime, Skip = 0, Take = 512 };
			var total = getClient.GetChangeCount(request);

			do
			{
				changes = getClient.GetChanges(request).ToList();
				applyClient.ApplyChanges(changes);
				OnSyncStatusChanged(new SyncEngineStatusArgs { Count = request.Skip, Total = total, Status = Status });
				request.Skip += changes.Count;
			} while (!_cancelPending && (changes.Count > 0 || request.Skip < total));
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