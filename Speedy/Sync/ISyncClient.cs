#region References

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
		/// Gets or sets the name of the sync client.
		/// </summary>
		string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		void ApplyChanges(IEnumerable<SyncObject> changes);

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		int GetChangeCount(SyncRequest request);

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		IEnumerable<SyncObject> GetChanges(SyncRequest request);

		#endregion
	}
}