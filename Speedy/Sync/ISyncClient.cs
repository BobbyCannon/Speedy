namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync client.
	/// </summary>
	public interface ISyncClient : ISyncServerProxy
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