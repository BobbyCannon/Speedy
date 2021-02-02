#region References

using Microsoft.EntityFrameworkCore;

#endregion

namespace Speedy.Website.Data.Sqlite
{
	public class ContosoSqliteDatabase : ContosoDatabase
	{
		#region Constructors

		public ContosoSqliteDatabase()
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoSqliteDatabase(DbContextOptions<ContosoSqliteDatabase> options)
			: this(options, null, null)
		{
		}

		public ContosoSqliteDatabase(DbContextOptions contextOptions, DatabaseOptions options, DatabaseKeyCache keyCache)
			: base(contextOptions, options, keyCache)
		{
		}

		#endregion

		#region Methods

		public static ContosoSqliteDatabase UseSqlite(string connectionString, DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			connectionString ??= GetConnectionString();

			var builder = new DbContextOptionsBuilder<ContosoSqliteDatabase>();
			return new ContosoSqliteDatabase(builder.UseSqlite(connectionString, UpdateOptions).Options, options ?? GetDefaultOptions(), keyCache);
		}

		protected override void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options.UseSqlite(GetConnectionString(), UpdateOptions);
			}

			ConfigureGlobalOptions(options);
		}

		#endregion
	}
}