#region References

using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;

#endregion

namespace Speedy.Website.Data.Sql
{
	public class ContosoSqlDatabase : ContosoDatabase
	{
		#region Constructors

		public ContosoSqlDatabase()
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoSqlDatabase(DbContextOptions<ContosoSqlDatabase> options)
			: this(options, null, null)
		{
		}

		public ContosoSqlDatabase(DbContextOptions contextOptions, DatabaseOptions options, DatabaseKeyCache keyCache)
			: base(contextOptions, options, keyCache)
		{
		}

		#endregion

		#region Methods

		public static ContosoSqlDatabase UseSql(string connectionString, DatabaseOptions options, DatabaseKeyCache keyCache)
		{
			connectionString ??= GetConnectionString();

			var builder = new DbContextOptionsBuilder<ContosoSqlDatabase>();
			return new ContosoSqlDatabase(builder.UseSqlServer(connectionString, UpdateOptions).Options, options ?? GetDefaultOptions(), keyCache);
		}

		protected override void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options.UseSqlServer(GetConnectionString(), UpdateOptions);
			}

			ConfigureGlobalOptions(options);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			Json.ConfigureForSql(modelBuilder);
			base.OnModelCreating(modelBuilder);
		}

		#endregion
	}
}