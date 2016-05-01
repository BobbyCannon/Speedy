#region References

using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using Speedy.Samples.Sync;
using Speedy.Sync;

#endregion

namespace Speed.Benchmarks
{
	[ImplementPropertyChanged]
	public class MainWindowModel
	{
		#region Constructors

		public MainWindowModel()
		{
			Output = string.Empty;
			Progress = 0;
			RepositoryStatus = "Repository";
			SyncClients = new ObservableCollection<SyncClientState>();
			SyncStatus = "Sync";
		}

		#endregion

		#region Properties

		public string Errors { get; set; }

		public string Output { get; set; }

		public int Progress { get; set; }

		public string RepositoryStatus { get; set; }

		public ObservableCollection<SyncClientState> SyncClients { get; set; }

		public string SyncStatus { get; set; }

		#endregion
	}

	[ImplementPropertyChanged]
	public class SyncClientState
	{
		#region Constructors

		public SyncClientState(IContosoSyncClient syncClient)
		{
			Client = syncClient;
			PreviousSyncedOn = DateTime.MinValue;
			LastSyncedOn = DateTime.MinValue;
			Status = SyncEngineStatus.Stopped;
		}

		#endregion

		#region Properties

		public int AddressCount { get; set; }

		public IContosoSyncClient Client { get; }

		public DateTime LastSyncedOn { get; set; }

		public int PeopleCount { get; set; }

		public DateTime PreviousSyncedOn { get; set; }

		public SyncEngineStatus Status { get; set; }

		#endregion
	}
}