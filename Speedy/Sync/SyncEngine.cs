#region References

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync engine.
	/// </summary>
	public class SyncEngine
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the sync engine.
		/// </summary>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="lastSyncedOn"> The last date and time the database synced. </param>
		public SyncEngine(ISyncClient client, ISyncClient server, DateTime lastSyncedOn)
		{
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
		/// Start to sync the systems.
		/// </summary>
		public void Run()
		{
			Status = SyncEngineStatus.Starting;
			StartTime = DateTime.UtcNow;

			Status = SyncEngineStatus.Pulling;
			Process(Server, Client);

			Status = SyncEngineStatus.Pushing;
			Process(Client, Server);
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
				OnSyncStatusChanged(new SyncEngineStatusArgs { Count = request.Skip, Total = total, Status = Status });
				applyClient.ApplyChanges(changes);
				request.Skip += changes.Count;
			} while (changes.Count > 0 || request.Skip < total);

			OnSyncStatusChanged(new SyncEngineStatusArgs { Count = request.Skip, Total = total, Status = Status });
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