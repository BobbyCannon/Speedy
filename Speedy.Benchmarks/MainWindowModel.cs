#region References

using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Speedy.Sync;

#endregion

namespace Speed.Benchmarks
{
	[AddINotifyPropertyChangedInterface]
	public class MainWindowModel
	{
		#region Constructors

		public MainWindowModel()
		{
			DatabaseStatus = "Database";
			DatabaseClients = new ObservableCollection<DatabaseClient>();
			Output = string.Empty;
			Progress = 0;
			RepositoryStatus = "Repository";
			SyncClients = new ObservableCollection<SyncClientState>();
			SyncStatus = "Sync";
		}

		#endregion

		#region Properties

		public ObservableCollection<DatabaseClient> DatabaseClients { get; set; }

		public string DatabaseStatus { get; set; }

		public string Errors { get; set; }

		public string Output { get; set; }

		public int Progress { get; set; }

		public string RepositoryStatus { get; set; }

		public ObservableCollection<SyncClientState> SyncClients { get; set; }

		public string SyncStatus { get; set; }

		#endregion
	}

	[AddINotifyPropertyChangedInterface]
	public class DatabaseClient
	{
		#region Properties

		public int AddressCount { get; set; }

		public DateTime LastProcessedOn { get; set; }

		public string Name { get; set; }

		public int PeopleCount { get; set; }

		#endregion
	}

	[AddINotifyPropertyChangedInterface]
	public class SyncClientState
	{
		#region Constructors

		public SyncClientState(SyncClient syncClient)
		{
			Client = syncClient;
			PreviousSyncedOn = DateTime.MinValue;
			LastSyncedOn = DateTime.MinValue;
			Status = SyncEngineStatus.Stopped;
		}

		#endregion

		#region Properties

		public int AddressCount { get; set; }

		public SyncClient Client { get; }

		public DateTime LastSyncedOn { get; set; }

		public int PeopleCount { get; set; }

		public DateTime PreviousSyncedOn { get; set; }

		public SyncEngineStatus Status { get; set; }

		#endregion
	}
}