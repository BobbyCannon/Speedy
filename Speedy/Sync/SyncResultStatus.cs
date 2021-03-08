#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// The status of the sync result.
	/// </summary>
	[Flags]
	public enum SyncResultStatus
	{
		/// <summary>
		/// No flags have been set.
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// The sync was completed successfully.
		/// </summary>
		Successful = 0x0001,

		/// <summary>
		/// The sync was able to start.
		/// </summary>
		Started = 0x0002,

		/// <summary>
		/// The sync was cancelled.
		/// </summary>
		Cancelled = 0x0004,

		/// <summary>
		/// The sync was able to run the full sync cycle.
		/// </summary>
		Completed = 0x0008
	}
}