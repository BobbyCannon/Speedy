#region References

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Speedy.Data.Client;
using Speedy.EntityFramework;
using Speedy.Extensions;

#endregion

namespace Speedy.Client.Data
{
	public class ContosoClientDatabase : EntityFrameworkDatabase
	{
		#region Constructors

		public ContosoClientDatabase()
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoClientDatabase(DbContextOptions<ContosoClientDatabase> options)
			: this(options, null)
		{
		}

		public ContosoClientDatabase(DbContextOptions contextOptions, DatabaseOptions options)
			: base(contextOptions, options ?? GetDefaultOptions())
		{
		}

		#endregion

		#region Properties

		public IRepository<ClientAccount, int> Accounts => GetSyncableRepository<ClientAccount, int>();

		public IRepository<ClientAddress, long> Addresses => GetSyncableRepository<ClientAddress, long>();

		public IRepository<ClientLogEvent, long> LogEvents => GetSyncableRepository<ClientLogEvent, long>();

		#endregion

		#region Methods

		public static DatabaseOptions GetDefaultOptions()
		{
			return new DatabaseOptions
			{
				SyncOrder = new[]
				{
					typeof(ClientAccount).ToAssemblyName(),
					typeof(ClientAddress).ToAssemblyName(),
					typeof(ClientLogEvent).ToAssemblyName()
				}
			};
		}

		public static string GetPersonalDataPath()
		{
			var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var directoryInfo = new DirectoryInfo(directoryPath);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return directoryPath;
		}

		public static ContosoClientDatabase UseSqlite(string connectionString = null, DatabaseOptions options = null)
		{
			if (connectionString == null)
			{
				var path = GetPersonalDataPath();
				var dbPath = Path.Combine(path, "Speedy.db");
				connectionString = $"Data Source={dbPath}";
			}

			var builder = new DbContextOptionsBuilder<ContosoClientDatabase>();
			return new ContosoClientDatabase(builder.UseSqlite(connectionString, UpdateOptions).Options, options);
		}

		private static void UpdateOptions(SqliteDbContextOptionsBuilder obj)
		{
		}

		#endregion
	}
}