#region References

using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
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

		public ContosoClientDatabase(DbContextOptions contextOptions)
			: base(contextOptions, null)
		{
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

		public static string GetConnectionString()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("AppSettings.json", true)
				.Build();

			var connectionString = configuration.GetConnectionString("DefaultConnection");

			if (connectionString == null)
			{
				// Mobile Database Location ApplicationSettings.json
				var path = GetPersonalDataPath();
				configuration = new ConfigurationBuilder()
					.SetBasePath(path)
					.AddJsonFile("AppSettings.json", true)
					.Build();

				var dbPath = Path.Combine(path, "Speedy.db");
				connectionString = configuration.GetConnectionString("DefaultConnection") ?? $"Data Source={dbPath}";
			}

			if (connectionString == null)
			{
				throw new Exception("Failed to locate the connection string.");
			}

			return connectionString;
		}

		public static DatabaseOptions GetDefaultOptions()
		{
			return new DatabaseOptions
			{
				SyncOrder = new[]
				{
					typeof(ClientAddress).ToAssemblyName(),
					typeof(ClientAccount).ToAssemblyName(),
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
				connectionString = GetConnectionString();
			}

			var builder = new DbContextOptionsBuilder<ContosoClientDatabase>();
			return new ContosoClientDatabase(builder.UseSqlite(connectionString, UpdateOptions).Options, options);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				optionsBuilder.UseSqlite(GetConnectionString(), UpdateOptions);
			}

			base.OnConfiguring(optionsBuilder);
		}

		private static void UpdateOptions(SqliteDbContextOptionsBuilder obj)
		{
		}

		#endregion
	}
}