namespace Speedy.Samples
{
	public class EntityFrameworkContosoDatabaseProvider : IContosoDatabaseProvider
	{
		#region Fields

		private readonly string _connectionString;

		#endregion

		#region Constructors

		public EntityFrameworkContosoDatabaseProvider(string connectionString)
		{
			_connectionString = connectionString;
		}

		#endregion

		#region Methods

		public IContosoDatabase GetDatabase()
		{
			return new EntityFrameworkContosoDatabase(_connectionString);
		}

		#endregion
	}
}