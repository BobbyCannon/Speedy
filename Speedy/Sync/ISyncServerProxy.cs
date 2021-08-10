#region References

using System;
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a server proxy to communicate between a sync client and a sync engine.
	/// </summary>
	public interface ISyncServerProxy
	{
		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		ServiceResult<SyncIssue> ApplyChanges(Guid sessionId, ServiceRequest<SyncObject> changes);

		/// <summary>
		/// Sends issue corrections to a server.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		/// <param name="corrections"> The corrections to write to the server. </param>
		/// <returns> A list of sync issues if there were any. </returns>
		ServiceResult<SyncIssue> ApplyCorrections(Guid sessionId, ServiceRequest<SyncObject> corrections);

		/// <summary>
		/// Starts the sync session.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		/// <param name="options"> The options for the sync session. </param>
		SyncSession BeginSync(Guid sessionId, SyncOptions options);

		/// <summary>
		/// Ends the sync session.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		SyncStatistics EndSync(Guid sessionId);

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request);

		/// <summary>
		/// Gets the list of sync objects to try and resolve the issue list.
		/// </summary>
		/// <param name="sessionId"> The ID of the sync session. </param>
		/// <param name="issues"> The issues to process. </param>
		/// <returns> The sync objects to resolve the issues. </returns>
		ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues);

		#endregion
	}
}