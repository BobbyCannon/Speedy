#region References

using Microsoft.EntityFrameworkCore;
using Speedy.Website.Samples;

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
			: this(options, null)
		{
		}

		public ContosoSqlDatabase(DbContextOptions contextOptions, DatabaseOptions options)
			: base(contextOptions, options)
		{
		}

		#endregion

		#region Methods

		public static ContosoSqlDatabase UseSql(string connectionString = null, DatabaseOptions options = null)
		{
			if (connectionString == null)
			{
				connectionString = GetConnectionString();
			}

			var builder = new DbContextOptionsBuilder<ContosoSqlDatabase>();
			return new ContosoSqlDatabase(builder.UseSqlServer(connectionString, UpdateOptions).Options, options ?? GetDefaultOptions());
		}

		protected override void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
		{
			if (!options.IsConfigured)
			{
				options.UseSqlServer(GetConnectionString(), UpdateOptions);
			}

			ConfigureGlobalOptions(options);
		}

		#endregion
	}
}