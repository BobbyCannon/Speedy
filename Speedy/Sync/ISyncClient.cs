#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public interface ISyncClient
	{
		#region Properties

		/// <summary>
		/// Gets or sets the last date and time the client synced.
		/// </summary>
		DateTime LastSyncedOn { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Apply changes from the server.
		/// </summary>
		/// <param name="changes"> The changes from the server. </param>
		void ApplyChanges(IEnumerable<SyncEntity> changes);

		/// <summary>
		/// Gets the changes from the client.
		/// </summary>
		/// <returns> The list of changes from the client. </returns>
		IEnumerable<SyncEntity> GetChanges();

		#endregion
	}
}