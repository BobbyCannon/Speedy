namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface IDatabaseProvider
	{
		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		DatabaseOptions Options { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		IDatabase GetDatabase();

		#endregion
	}
}