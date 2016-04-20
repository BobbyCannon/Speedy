#region References

using System.Collections.ObjectModel;
using PropertyChanged;
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
			SyncClients = new ObservableCollection<ISyncClient>();
			SyncStatus = "Sync";
		}

		#endregion

		#region Properties

		public string Output { get; set; }

		public ObservableCollection<ISyncClient> SyncClients { get; set; }

		public string SyncStatus { get; set; }

		#endregion
	}
}