#region References

#endregion

namespace Speedy.Automation.Tests.Database;

/// <summary>
/// Base for database testing.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class DatabaseTest<T, T2> : SpeedyTest
	where T : DatabaseProvider<T2>
	where T2 : IDatabase
{
	#region Fields

	private T _databaseProvider;

	#endregion

	#region Properties

	/// <summary>
	/// The provider for database instances.
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