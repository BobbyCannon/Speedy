#region References

using System;
using Speedy.Net;

#endregion

namespace Speedy.Sync;

/// <summary>
/// Represents a sync client.
/// </summary>
public interface ISyncClient
{
	#region Properties

	/// <summary>
	/// The database provider to use during a sync session.
	/// </summary>
	ISyncableDatabaseProvider DatabaseProvider { get; }

	/// <summary>
	/// An optional converter to process sync objects from Server to Client
	/// </summary>
	SyncClientIncomingConverter IncomingConverter { get; set; }

	/// <summary>
	/// Gets or sets the name of the sync client.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The options for the sync client
	/// </summary>
	SyncClientOptions Options { get; }

	/// <summary>
	/// An optional converter to process sync objects from Client to Server
	/// </summary>
	SyncClientOutgoingConverter OutgoingConverter { get; set; }

	/// <summary>
	/// Profiler for tracking specific points during sync client processing.
	/// </summary>
	SyncClientProfiler Profiler { get; }

	/// <summary>
	/// The communication statistics for this sync client.
	/// </summary>
	SyncStatistics Statistics { get; }

	/// <summary>
	/// The options for the sync
	/// </summary>
	SyncOptions SyncOptions { get; }

	#endregion

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

	/// <summary>
	/// Gets an instance of the database this sync client is for.
	/// </summary>
	/// <returns> The database that is syncable. </returns>
	ISyncableDatabase GetDatabase();

	/// <summary>
	/// Gets an instance of the database this sync client is for.
	/// </summary>
	/// <returns> The database that is syncable. </returns>
	T GetDatabase<T>() where T : class, ISyncableDatabase;

	#endregion
}