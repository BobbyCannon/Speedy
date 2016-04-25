#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents options to be used during a sync.
	/// </summary>
	public class SyncOptions
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		public SyncOptions()
		{
			LastSyncedOn = DateTime.MinValue;
			ItemsPerSyncRequest = 300;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the number of objects to be processed per sync request.
		/// </summary>
		public int ItemsPerSyncRequest { get; set; }

		/// <summary>
		/// Gets or sets the last synced on date and time.
		/// </summary>
		public DateTime LastSyncedOn { get; set; }

		#endregion
	}
}