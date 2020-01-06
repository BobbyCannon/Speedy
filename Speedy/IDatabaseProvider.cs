namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface IDatabaseProvider<out T> : IDatabaseProvider where T : IDatabase
	{
		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		new T GetDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <returns> The database instance. </returns>
		new T GetDatabase(DatabaseOptions options);

		#endregion
	}
	
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface IDatabaseProvider
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
		IDatabase GetDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <returns> The database instance. </returns>
		IDatabase GetDatabase(DatabaseOptions options);

		#endregion
	}
}