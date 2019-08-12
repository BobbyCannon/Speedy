namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases that is also a normal database provider.
	/// </summary>
	public interface ISyncableDatabaseProvider<out T> : ISyncableDatabaseProvider
		where T : ISyncableDatabase
	{
		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		T GetDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <returns> The database instance. </returns>
		T GetDatabase(DatabaseOptions options);

		#endregion
	}
	
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface ISyncableDatabaseProvider
	{
		#region Properties

		/// <summary>
		/// Gets or sets the options for the database provider.
		/// </summary>
		DatabaseOptions Options { get; set; }

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
		/// <returns> The database instance. </returns>
		ISyncableDatabase GetSyncableDatabase(DatabaseOptions options);

		#endregion
	}
}