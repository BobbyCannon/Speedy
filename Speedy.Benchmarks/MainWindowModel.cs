#region References

using System;
using System.Collections.ObjectModel;
using PropertyChanged;

#endregion

namespace Speed.Benchmarks
{
	[ImplementPropertyChanged]
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
		
		public string SyncStatus { get; set; }

		#endregion
	}

	[ImplementPropertyChanged]
	public class DatabaseClient
	{
		#region Properties

		public int AddressCount { get; set; }

		public DateTime LastProcessedOn { get; set; }

		public string Name { get; set; }

		public int PeopleCount { get; set; }

		#endregion
	}
}