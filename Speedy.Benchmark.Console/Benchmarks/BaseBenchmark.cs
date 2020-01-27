#region References

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;
using Speedy.Sync;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Samples;

#endregion

namespace Speedy.Benchmark.Benchmarks
{
	public abstract class BaseBenchmark
	{
		#region Constructors

		public BaseBenchmark()
		{
			DefaultConnection = "server=localhost;database=Speedy;integrated security=true;";
			DefaultConnection2 = "server=localhost;database=Speedy2;integrated security=true;";
			DefaultSqliteConnection = "Data Source=Speedy.db";
			DefaultSqliteConnection2 = "Data Source=Speedy2.db";
		}

		#endregion

		#region Properties

		public string DefaultConnection { get; }

		public string DefaultConnection2 { get; }

		public string DefaultSqliteConnection { get; }

		public string DefaultSqliteConnection2 { get; }

		#endregion

		#region Methods

		public ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x));
			}
		}

		public ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider2()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x));
			}
		}

		public abstract void Run();

		protected void ClearDatabase(ISyncClient client)
		{
			using (var database = client.GetDatabase())
			{
				if (database is EntityFrameworkDatabase efDatabase)
				{
					efDatabase.ClearDatabase();
				}
			}
		}

		protected int Count<T, T2>(ISyncClient client) where T : Entity<T2>
		{
			using (var database = client.GetDatabase())
			{
				return database.GetRepository<T, T2>().Count();
			}
		}

		protected IEnumerable<(SyncClient server, SyncClient client)> GetScenarios()
		{
			//yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			//yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()));
			//yield return (new SyncClient("Server (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (Sqlite2)", GetEntityFrameworkSqliteProvider2()));
			//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider()));
			yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (SQL2)", GetSyncableEntityFrameworkProvider2()));
		}

		internal ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultConnection))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x));
			}
		}

		internal ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider2()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultConnection2))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x));
			}
		}

		internal ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options);
			return new SyncDatabaseProvider(x => database, options);
		}

		#endregion
	}
}