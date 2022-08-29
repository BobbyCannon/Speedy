#region References

using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Speedy.Data.SyncApi;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Storage;
using Speedy.Website.Data.Entities;
using ConfigurationManager = System.Configuration.ConfigurationManager;

#endregion

namespace Speedy.Website.Data
{
	[ExcludeFromCodeCoverage]
	public abstract class ContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		protected ContosoDatabase()
		{
			// Default constructor needed for Add-Migration
		}

		protected ContosoDatabase(DbContextOptions<ContosoDatabase> options)
			: this(options, null, null)
		{
		}

		protected ContosoDatabase(DbContextOptions contextOptions, DatabaseOptions options, DatabaseKeyCache keyCache)
			: base(contextOptions, options, keyCache)
		{
			SetRequiredOptions(options);
		}

		#endregion

		#region Properties

		public ISyncableRepository<AccountEntity, int> Accounts => GetSyncableRepository<AccountEntity, int>();
		public ISyncableRepository<AddressEntity, long> Addresses => GetSyncableRepository<AddressEntity, long>();
		public bool EnableSaveProcessing { get; set; }
		public IRepository<FoodEntity, int> Food => GetRepository<FoodEntity, int>();
		public IRepository<FoodRelationshipEntity, int> FoodRelationships => GetRepository<FoodRelationshipEntity, int>();
		public IRepository<GroupMemberEntity, int> GroupMembers => GetRepository<GroupMemberEntity, int>();
		public IRepository<GroupEntity, int> Groups => GetRepository<GroupEntity, int>();
		public ISyncableRepository<LogEventEntity, long> LogEvents => GetSyncableRepository<LogEventEntity, long>();
		public IRepository<PetEntity, (string Name, int OwnerId)> Pets => GetRepository<PetEntity, (string Name, int OwnerId)>();
		public IRepository<PetTypeEntity, string> PetTypes => GetRepository<PetTypeEntity, string>();
		public ISyncableRepository<SettingEntity, long> Settings => GetSyncableRepository<SettingEntity, long>();
		public IRepository<TrackerPathConfigurationEntity, int> TrackerPathConfigurations => GetRepository<TrackerPathConfigurationEntity, int>();
		public IRepository<TrackerPathEntity, long> TrackerPaths => GetRepository<TrackerPathEntity, long>();

		#endregion

		#region Methods

		public static string GetConnectionString()
		{
			var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
			if ((connection != null) && !string.IsNullOrWhiteSpace(connection.ConnectionString))
			{
				return connection.ConnectionString;
			}

			if (connection == null)
			{
				// NOTE: EF 3.1 design tools no longer initializes ConfigurationManager so we have to load it manually for migrations
				// https://github.com/dotnet/efcore/issues/19760
				connection = GetFromAppConfig();

				if (connection != null)
				{
					return connection.ConnectionString;
				}
			}

			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("AppSettings.json", true)
				.Build();

			return configuration.GetConnectionString("DefaultConnection");
		}

		public static DatabaseOptions GetDefaultOptions()
		{
			var response = new DatabaseOptions();
			SetRequiredOptions(response);
			return response;
		}

		public override Assembly GetMappingAssembly()
		{
			return typeof(AddressEntity).Assembly;
		}

		public static void SetRequiredOptions(DatabaseOptions options)
		{
			options.SyncOrder = new[]
			{
				typeof(AddressEntity).ToAssemblyName(),
				typeof(SettingEntity).ToAssemblyName(),
				typeof(AccountEntity).ToAssemblyName(),
				typeof(LogEventEntity).ToAssemblyName()
			};
		}

		/// <summary>
		/// Update the options for default Speedy values. Ex. Migration History will be [system].[MigrationHistory] instead of [dbo].[__EFMigrationsHistory].
		/// </summary>
		/// <param name="builder"> The builder to set the options on. </param>
		public static void UpdateOptions(SqlServerDbContextOptionsBuilder builder)
		{
			builder.MigrationsHistoryTable("MigrationHistory", "system");
		}

		/// <summary>
		/// Update the options for default Speedy values. Ex. Migration History will be [system].[MigrationHistory] instead of [dbo].[__EFMigrationsHistory].
		/// </summary>
		/// <param name="builder"> The builder to set the options on. </param>
		public static void UpdateOptions(SqliteDbContextOptionsBuilder builder)
		{
			builder.MigrationsHistoryTable("MigrationHistory", "system");
		}

		protected virtual void ConfigureDatabaseOptions(DbContextOptionsBuilder options)
		{
			options.UseLazyLoadingProxies();
		}

		protected static void ConfigureGlobalOptions(DbContextOptionsBuilder options)
		{
			options.UseLazyLoadingProxies();
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			ConfigureDatabaseOptions(optionsBuilder);
			base.OnConfiguring(optionsBuilder);
		}

		/// <inheritdoc />
		protected override void OnChangesSaved(CollectionChangeTracker e)
		{
			if (EnableSaveProcessing)
			{
				ProcessSavedChanges(this, e);
			}
			base.OnChangesSaved(e);
		}

		internal static void ProcessSavedChanges(IContosoDatabase database, CollectionChangeTracker changes)
		{
			foreach (var entity in changes.Added)
			{
				switch (entity)
				{
					case AddressEntity address:
					{
						database.LogEvents.Add(new LogEventEntity { Message = $"Address Added {address.Id} {address.Line1}", Level = LogLevel.Critical });
						break;
					}
				}
			}

			foreach (var entity in changes.Removed)
			{
				switch (entity)
				{
					case AddressEntity address:
					{
						database.LogEvents.Add(new LogEventEntity { Message = $"Address Deleted {address.Id} {address.Line1}", Level = LogLevel.Critical });
						break;
					}
				}
			}

			foreach (var entity in changes.Modified)
			{
				switch (entity)
				{
					case AddressEntity address:
					{
						database.LogEvents.Add(new LogEventEntity { Message = $"Address Modified {address.Id} {address.Line1}", Level = LogLevel.Critical });
						break;
					}
				}
			}
		}

		private static ConnectionStringSettings GetFromAppConfig()
		{
			var directory = Environment.CurrentDirectory;
			var configFileNames = new[] { "web.config", "app.config" };

			foreach (var configFileName in configFileNames)
			{
				var filePath = Path.Combine(directory, configFileName);

				if (!File.Exists(filePath))
				{
					continue;
				}

				var map = new ExeConfigurationFileMap { ExeConfigFilename = filePath };
				var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
				var connection = config.ConnectionStrings.ConnectionStrings["DefaultConnection"];

				if (connection != null)
				{
					return connection;
				}
			}

			return null;
		}

		#endregion
	}
}