#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Event arguments for the sync engine status change event.
	/// </summary>
	public class SyncEngineStatusArgs : EventArgs
	{
		#region Properties

		/// <summary>
		/// Gets or sets the current count of items processed.
		/// </summary>
		public long Count { get; set; }

		/// <summary>
		/// Gets or sets the current status of the sync.
		/// </summary>
		public SyncEngineStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the total count of the items to process.
		/// </summary>
		public long Total { get; set; }

		/// <summary>
		/// Gets the percentage of progress. Ranging from [0.00] to [100.00].
		/// </summary>
		public decimal Percent => Math.Round((decimal) Count / Total * 100, 2);

		#endregion
	}
}