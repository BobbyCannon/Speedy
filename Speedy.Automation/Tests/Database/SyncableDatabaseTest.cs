#region References

using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Database;

/// <summary>
/// Represents a syncable database test.
/// </summary>
/// <typeparam name="T"> The database provider type. </typeparam>
/// <typeparam name="T2"> The database type. </typeparam>
public abstract class SyncableDatabaseTest<T, T2> : SpeedyTest
	where T : SyncableDatabaseProvider<T2>
	where T2 : ISyncableDatabase
{
	#region Fields

	private T _databaseProvider;

	#endregion

	#region Properties

	/// <summary>
	/// The database provider.
	/// </summary>
	public T DatabaseProvider => _databaseProvider ??= GetDatabaseProvider();

	#endregion

	#region Methods

	/// <summary>
	/// Gets the database provider.
	/// </summary>
	/// <returns> The database provider. </returns>
	protected abstract T GetDatabaseProvider();

	#endregion
}