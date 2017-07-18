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
		/// Gets or sets the name of the sync client.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets or sets the ID of the current session.
		/// </summary>
		Guid SessionId { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		IEnumerable<SyncIssue> ApplyChanges(IEnumerable<SyncObject> changes);

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		IEnumerable<SyncIssue> ApplyCorrections(IEnumerable<SyncObject> corrections);

		/// <summary>
		/// Gets the count of the changes from the server.
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

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		IEnumerable<SyncObject> GetCorrections(IEnumerable<SyncIssue> issues);

		/// <summary>
		/// Gets an instance of the database this sync client is for.
		/// </summary>
		/// <returns> The database that is syncable. </returns>
		ISyncableDatabase GetDatabase();

		/// <summary>
		/// Gets an instance of the database this sync client is for.
		/// </summary>
		/// <returns> The database that is syncable. </returns>
		T GetDatabase<T>() where T : class, ISyncableDatabase, IDatabase;

		#endregion
	}
}