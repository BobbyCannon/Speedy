#region References

#endregion

namespace Speedy.Automation.Tests.Database;

public abstract class DatabaseTest<T, T2> : SpeedyTest
	where T : DatabaseProvider<T2>
	where T2 : IDatabase
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