#region References

using System;
using Microsoft.EntityFrameworkCore;
using Speedy.Website.Samples;

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
			: this(options, null)
		{
		}

		public ContosoSqliteDatabase(DbContextOptions contextOptions, DatabaseOptions options)
			: base(contextOptions, options)
		{
		}

		#endregion

		#region Methods

		public static ContosoSqliteDatabase UseSqlite(string connectionString = null, DatabaseOptions options = null)
		{
			if (connectionString == null)
			{
				connectionString = GetConnectionString();
			}

			var builder = new DbContextOptionsBuilder<ContosoSqliteDatabase>();
			return new ContosoSqliteDatabase(builder.UseSqlite(connectionString, UpdateOptions).Options, options ?? GetDefaultOptions());
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