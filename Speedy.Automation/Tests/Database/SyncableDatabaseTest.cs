#region References

using Speedy.Sync;

#endregion

namespace Speedy.Automation.Tests.Database;

public abstract class SyncableDatabaseTest<T, T2> : SpeedyTest
	where T : SyncableDatabaseProvider<T2>
	where T2 : ISyncableDatabase
{
	#region Fields

	private T _databaseProvider;

	#endregion

	#region Properties

	public T DatabaseProvider => _databaseProvider ??= GetDatabaseProvider();

	#endregion

	#region Methods

	protected abstract T GetDatabaseProvider();

	#endregion
}