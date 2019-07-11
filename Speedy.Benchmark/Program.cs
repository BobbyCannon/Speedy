#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Samples.Sql;
using Speedy.Samples.Sqlite;
using Speedy.Sync;

#endregion

namespace Speedy.Benchmark
{
	public static class Program
	{
		#region Properties

		public static string DefaultConnection { get; private set; }

		public static string DefaultConnection2 { get; private set; }

		public static string DefaultSqliteConnection { get; private set; }

		public static string DefaultSqliteConnection2 { get; private set; }

		#endregion

		#region Methods

		public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			var command = "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF ''?'' NOT LIKE ''%MigrationHistory%'' AND ''?'' NOT LIKE ''%MigrationsHistory%'' DELETE FROM ?'\r\nEXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";
			database.Database.ExecuteSqlCommand(command);
			return database;
		}

		public static void Dump<T>(this T item, string prefix = "")
		{
			Console.Write(prefix);
			Console.WriteLine(item);
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x));
			}
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider2()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x));
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultConnection))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x));
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider2()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultConnection2))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x));
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options);
			return new SyncDatabaseProvider(x => database, options);
		}

		private static void Main(string[] args)
		{
			DefaultConnection = "server=localhost;database=Speedy;integrated security=true;";
			DefaultConnection2 = "server=localhost;database=Speedy2;integrated security=true;";
			DefaultSqliteConnection = "Data Source=Speedy.db";
			DefaultSqliteConnection2 = "Data Source=Speedy2.db";

			var options = new DatabaseOptions { DisableEntityValidations = true };
			var options2 = new DatabaseOptions { DisableEntityValidations = true };

			IEnumerable<(SyncClient server, SyncClient client)> GetScenarios()
			{
				//(new SyncClient("Server (mem)", GetSyncableMemoryProvider(options: options)), new SyncClient("Client (mem)", GetSyncableMemoryProvider(options: options2))),
				//yield return (new SyncClient("Server (mem)", GetSyncableMemoryProvider(options: options)), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
				//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (mem)", GetSyncableMemoryProvider(options: options)));
				yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (Sqlite2)", GetEntityFrameworkSqliteProvider2()));
				//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
				//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider()));
				//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (SQL2)", GetSyncableEntityFrameworkProvider2()));
			}

			var scenarios = GetScenarios();

			foreach (var scenario in scenarios)
			{
				ProcessScenario(scenario);
				ProcessScenario(scenario);
			}

			Console.ReadKey(true);
		}

		private static void ProcessScenario((SyncClient server, SyncClient client) scenario)
		{
			var server = scenario.server;
			var client = scenario.client;

			$"{client.Name} -> {server.Name}".Dump();

			var watch = Stopwatch.StartNew();
			var database = server.GetDatabase<IContosoDatabase>();
			var count = 5000;

			for (var i = 0; i < count; i++)
			{
				var address = new AddressEntity
				{
					City = Guid.NewGuid().ToString(),
					Id = default,
					Line1 = Guid.NewGuid().ToString(),
					Line2 = Guid.NewGuid().ToString(),
					LinkedAddressId = null,
					LinkedAddressSyncId = null,
					Postal = Guid.NewGuid().ToString(),
					State = Guid.NewGuid().ToString(),
					SyncId = Guid.NewGuid()
				};
				database.Addresses.Add(address);

				if (i > 0 && i % 300 == 0)
				{
					database.SaveChanges();
					database.Dispose();
					database = server.GetDatabase<IContosoDatabase>();
				}
			}

			database.SaveChanges();
			database.Dispose();
			watch.Elapsed.ToString().Dump("Data Written : ");
			watch.Restart();

			var engine = new SyncEngine(client, server, new SyncOptions());
			engine.Run();

			watch.Elapsed.ToString().Dump("Data Synced  : ");

			using (var clientDatabase = client.GetDatabase<IContosoDatabase>())
			using (var serverDatabase = server.GetDatabase<IContosoDatabase>())
			{
				clientDatabase.Addresses.Count().Dump("Client Count : ");
				serverDatabase.Addresses.Count().Dump("Server Count : ");
			}
		}

		#endregion
	}
}