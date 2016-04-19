#region References

using System;
using System.Collections.Generic;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync server.
	/// </summary>
	public interface ISyncServer
	{
		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		DateTime ApplyChanges(IEnumerable<SyncObject> changes);

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of changes from the server. </returns>
		IEnumerable<SyncObject> GetChanges(DateTime since);

		#endregion
	}
}