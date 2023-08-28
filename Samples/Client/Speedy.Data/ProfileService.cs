#region References

using Speedy.Profiling;
using Speedy.Storage.KeyValue;
using Speedy.Sync;

#endregion

namespace Speedy.Data
{
	public class ProfileService : Tracker
	{
		#region Constructors

		public ProfileService(ITrackerPathRepository profilerRepository, IDispatcher dispatcher)
			: base(profilerRepository, new KeyValueMemoryRepositoryProvider<TrackerPath>(), dispatcher)
		{
			RuntimeTimer = new Timer(dispatcher);
		}

		#endregion

		#region Properties

		public SyncTimer AverageSyncTimeForAccounts { get; set; }

		public SyncTimer AverageSyncTimeForAddress { get; set; }

		public SyncTimer AverageSyncTimeForAddresses { get; set; }

		public SyncTimer AverageSyncTimeForAll { get; set; }

		public SyncTimer AverageSyncTimeForLogEvents { get; set; }

		public Timer RuntimeTimer { get; }

		#endregion

		#region Methods

		public void Reset()
		{
			AverageSyncTimeForAccounts?.Reset();
			AverageSyncTimeForAddress?.Reset();
			AverageSyncTimeForAddresses?.Reset();
			AverageSyncTimeForAll?.Reset();
			AverageSyncTimeForLogEvents?.Reset();
			RuntimeTimer.Reset();
		}

		#endregion
	}

	public class ProfilerRepository : ITrackerPathRepository
	{
		#region Methods

		public void Write(params TrackerPath[] paths)
		{
		}

		#endregion
	}
}