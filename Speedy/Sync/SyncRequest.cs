#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// The details to ask a sync client for changes.
	/// </summary>
	public class SyncRequest
	{
		#region Properties

		/// <summary>
		/// The start date and time to get changes for.
		/// </summary>
		public DateTime Since { get; set; }

		/// <summary>
		/// The number of items to skip.
		/// </summary>
		public int Skip { get; set; }

		/// <summary>
		/// The number of items to take.
		/// </summary>
		public int Take { get; set; }

		/// <summary>
		/// The end date and time to get changes for.
		/// </summary>
		public DateTime Until { get; set; }

		#endregion
	}
}