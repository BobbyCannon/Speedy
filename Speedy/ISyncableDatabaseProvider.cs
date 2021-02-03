namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases that is also a normal database provider.
	/// </summary>
	public interface ISyncableDatabaseProvider<out T> : ISyncableDatabaseProvider, IDatabaseProvider<T>
		where T : ISyncableDatabase
	{
		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		new T GetSyncableDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <param name="keyCache"> An optional key manager for tracking entity IDs (primary and sync). </param>
		/// <returns> The database instance. </returns>
		new T GetSyncableDatabase(DatabaseOptions options, DatabaseKeyCache keyCache);

		#endregion
	}

	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface ISyncableDatabaseProvider : IDatabaseProvider
	{
		#region Properties

		/// <summary>
		/// An optional key manager for tracking entity IDs (primary and sync).
		/// </summary>
		DatabaseKeyCache KeyCache { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		ISyncableDatabase GetSyncableDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <param name="keyCache"> An optional key manager for tracking entity IDs (primary and sync). </param>
		/// <returns> The database instance. </returns>
		ISyncableDatabase GetSyncableDatabase(DatabaseOptions options, DatabaseKeyCache keyCache);

		#endregion
	}
}