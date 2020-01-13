#region References

using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Speedy.EntityFramework;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples
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
			: this(options, null)
		{
		}

		protected ContosoDatabase(DbContextOptions contextOptions, DatabaseOptions options)
			: base(contextOptions, options)
		{
			SetDefaultOptions(options);
		}

		#endregion

		#region Properties

		public IRepository<AccountEntity, int> Accounts => GetSyncableRepository<AccountEntity, int>();

		public IRepository<AddressEntity, long> Addresses => GetSyncableRepository<AddressEntity, long>();
		public IRepository<FoodEntity, int> Food => GetRepository<FoodEntity, int>();
		public IRepository<FoodRelationshipEntity, int> FoodRelationships => GetRepository<FoodRelationshipEntity, int>();
		public IRepository<GroupMemberEntity, int> GroupMembers => GetRepository<GroupMemberEntity, int>();
		public IRepository<GroupEntity, int> Groups => GetRepository<GroupEntity, int>();
		public IRepository<LogEventEntity, string> LogEvents => GetRepository<LogEventEntity, string>();
		public IRepository<PetEntity, (string Name, int OwnerId)> Pets => GetRepository<PetEntity, (string Name, int OwnerId)>();
		public IRepository<PetTypeEntity, string> PetTypes => GetRepository<PetTypeEntity, string>();
		public IRepository<SettingEntity, long> Settings => GetSyncableRepository<SettingEntity, long>();

		#endregion

		#region Methods

		public static string GetConnectionString()
		{
			var connection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
			if (connection != null && !string.IsNullOrWhiteSpace(connection.ConnectionString))
			{
				return connection.ConnectionString;
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
			SetDefaultOptions(response);
			return response;
		}

		public static void SetDefaultOptions(DatabaseOptions options)
		{
			options.SyncOrder = new[] { typeof(AddressEntity).ToAssemblyName(), typeof(AccountEntity).ToAssemblyName() };
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

		protected override Assembly GetMappingAssembly()
		{
			return typeof(AddressEntity).Assembly;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			ConfigureDatabaseOptions(optionsBuilder);
			base.OnConfiguring(optionsBuilder);
		}

		#endregion
	}
}