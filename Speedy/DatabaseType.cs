namespace Speedy
{
	/// <summary>
	/// Represents the type of database.
	/// </summary>
	public enum DatabaseType
	{
		/// <summary>
		/// Unknown database type?
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// An in memory database that behaves like EF SQL database.
		/// </summary>
		Memory = 1,

		/// <summary>
		/// Sql Database
		/// </summary>
		Sql = 2,

		/// <summary>
		/// Sqlite Database
		/// </summary>
		Sqlite = 3
	}
}