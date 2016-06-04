namespace Speedy.Samples.EntityFramework
{
	public class EntityFrameworkContosoDatabaseProvider : IContosoDatabaseProvider
	{
		#region Fields

		private readonly string _connectionString;

		#endregion

		#region Constructors

		public EntityFrameworkContosoDatabaseProvider(DatabaseOptions options = null)
			: this("name=DefaultConnection", options)
		{
		}

		public EntityFrameworkContosoDatabaseProvider(string connectionString, DatabaseOptions options = null)
		{
			_connectionString = connectionString;
			Options = options ?? new DatabaseOptions();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the options for this database.
		/// </summary>
		public DatabaseOptions Options { get; }

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			return new EntityFrameworkContosoDatabase(_connectionString, Options.DeepClone());
		}

		/// <summary>
		/// Gets an instance of the syncable database.
		/// </summary>
		/// <returns> The syncable database instance. </returns>
		ISyncableDatabase ISyncableDatabaseProvider.GetDatabase()
		{
			return GetDatabase();
		}

		/// <summary>
		/// Gets an instance of the database.
		/// </summary>
		/// <returns> The database instance. </returns>
		IDatabase IDatabaseProvider.GetDatabase()
		{
			return GetDatabase();
		}

		#endregion
	}
}