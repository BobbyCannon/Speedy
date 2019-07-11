namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface IDatabaseProvider<out T> where T : IDatabase
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
		T GetDatabase();

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <param name="options"> The database options to use for the new database instance. </param>
		/// <returns> The database instance. </returns>
		T GetDatabase(DatabaseOptions options);

		#endregion
	}
}