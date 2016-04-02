namespace Speedy.Samples
{
	public class EntityFrameworkSampleDatabaseProvider : ISampleDatabaseProvider
	{
		#region Fields

		private readonly string _connectionString;

		#endregion

		#region Constructors

		public EntityFrameworkSampleDatabaseProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		#endregion

		#region Methods

		public ISampleDatabase CreateContext()
		{
			return new EntityFrameworkSampleDatabase(_connectionString);
		}

		#endregion
	}
}