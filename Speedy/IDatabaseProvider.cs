#region References

using System;

#endregion

namespace Speedy;

/// <summary>
/// Represents a database provider for syncable databases.
/// </summary>
public interface IDatabaseProvider<out T> : IDatabaseProvider where T : IDatabase
{
	#region Methods

	/// <summary>
	/// Runs a bulk process where the database lifetime is based on the iteration size.
	/// A database will be instantiated and used for the iteration count. When the iteration
	/// count is reach the database will be saved and disposed. A new database will be created
	/// and processing will continue until the total count is reached. Finally the database
	/// will be saved and disposed.
	/// </summary>
	/// <param name="total"> The total amount of items to process. </param>
	/// <param name="iterationSize"> The iteration size of each process. </param>
	/// <param name="process"> The action to the process. </param>
	void BulkProcess(int total, int iterationSize, Action<int, T> process);

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