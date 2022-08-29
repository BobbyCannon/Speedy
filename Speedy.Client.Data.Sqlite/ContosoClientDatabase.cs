#region References

using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Speedy.Data.Client;
using Speedy.EntityFramework;

#endregion

namespace Speedy.Client.Data
{
	public class ContosoClientDatabase : EntityFrameworkDatabase, IContosoClientDatabase
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

		public ContosoClientDatabase(DbContextOptions contextOptions, DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
			: base(contextOptions, options ?? ContosoClientMemoryDatabase.GetDefaultOptions(), keyCache)
		{
		}

		#endregion

		#region Properties

		public ISyncableRepository<ClientAccount, int> Accounts => GetSyncableRepository<ClientAccount, int>();

		public ISyncableRepository<ClientAddress, long> Addresses => GetSyncableRepository<ClientAddress, long>();

		public ISyncableRepository<ClientLogEvent, long> LogEvents => GetSyncableRepository<ClientLogEvent, long>();

		public ISyncableRepository<ClientSetting, long> Settings => GetSyncableRepository<ClientSetting, long>();

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

		public override Assembly GetMappingAssembly()
		{
			return typeof(ContosoClientMemoryDatabase).Assembly;
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

		public static ContosoClientDatabase UseSqlite(string connectionString = null, DatabaseOptions options = null, DatabaseKeyCache keyCache = null)
		{
			connectionString ??= GetConnectionString();

			var builder = new DbContextOptionsBuilder<ContosoClientDatabase>();
			return new ContosoClientDatabase(builder.UseSqlite(connectionString, UpdateOptions).Options, options, keyCache);
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