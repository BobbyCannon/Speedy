namespace Speedy
{
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface ISyncableDatabaseProvider<out T> : IDatabaseProvider<T>
		where T : ISyncableDatabase
	{
	}
	
	/// <summary>
	/// Represents a database provider for syncable databases.
	/// </summary>
	public interface ISyncableDatabaseProvider : ISyncableDatabaseProvider<ISyncableDatabase>
	{
	}
}