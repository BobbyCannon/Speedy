#region References

using System;
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public interface ISyncClient : ISyncServerProxy
	{
		#region Properties

		/// <summary>
		/// Gets or sets the name of the sync client.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// An optional converter to process sync objects from Server to Client
		/// </summary>
		SyncClientIncomingConverter IncomingConverter { get; set; }
		
		/// <summary>
		/// An optional converter to process sync objects from Client to Server
		/// </summary>
		SyncClientOutgoingConverter OutgoingConverter { get; set; }

		/// <summary>
		/// The options for the sync client
		/// </summary>
		SyncClientOptions Options { get; }

		/// <summary>
		/// The communication statistics for this sync client.
		/// </summary>
		SyncStatistics Statistics { get; }

		/// <summary>
		/// The options for the sync
		/// </summary>
		SyncOptions SyncOptions { get; }
		
		/// <summary>
		/// The active sync session. Will be null when a session is not started.
		/// </summary>
		SyncSession SyncSession { get; }

		#endregion

		#region Methods

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
}