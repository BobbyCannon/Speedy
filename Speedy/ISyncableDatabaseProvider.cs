namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface ISyncableDatabaseProvider : IDatabaseProvider
	{
		#region Methods

		/// <summary>
		/// Gets an instance of the syncable database.
		/// </summary>
		/// <returns> The syncable database instance. </returns>
		new ISyncableDatabase GetDatabase();

		#endregion
	}
}