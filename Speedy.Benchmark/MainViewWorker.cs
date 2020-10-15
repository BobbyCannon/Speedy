#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Benchmark
{
	public static class MainViewWorker
	{
		#region Fields

		private static BackgroundWorker _worker;

		#endregion

		#region Constructors

		static MainViewWorker()
		{
			DefaultConnection = "server=localhost;database=Speedy;integrated security=true;";
			DefaultConnection2 = "server=localhost;database=Speedy2;integrated security=true;";
			DefaultSqliteConnection = "Data Source=Speedy.db";
			DefaultSqliteConnection2 = "Data Source=Speedy2.db";
		}

		#endregion

		#region Properties

		public static string DefaultConnection { get; }

		public static string DefaultConnection2 { get; }

		public static string DefaultSqliteConnection { get; }

		public static string DefaultSqliteConnection2 { get; }

		#endregion

		#region Methods

		public static void DoWork(object sender, DoWorkEventArgs e)
		{
			_worker = (BackgroundWorker) sender;

			var arguments = (object[]) e.Argument;
			var mainViewModel = arguments[1] as MainViewModel;

			CacheManager.Clear();

			BenchmarkSyncEngine(_worker, mainViewModel);
			//Test(dispatcher, worker);

			_worker.ReportProgress(100);
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider()
		{
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x), ContosoDatabase.GetDefaultOptions());
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider2()
		{
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x), ContosoDatabase.GetDefaultOptions());
		}

		private static void AddAccounts(int testCount, SyncClient client, MainViewModel view, AddressEntity address)
		{
			var entities = new List<AccountEntity>(testCount);

			for (var i = 1; i <= testCount; i++)
			{
				entities.Add(EntityFactory.GetAccount(x => { }, $"Account {i}", address));
			}

			StartResult($"{client.Name}: Adding accounts to database");

			var database = client.GetDatabase<IContosoDatabase>();

			if (view.UseBulkProcessing)
			{
				database.Accounts.BulkAddOrUpdate(entities);
			}
			else
			{
				var lastPercent = -1.0;
				var count = 0;

				for (var i = 0; i < entities.Count; i++)
				{
					database.Accounts.Add(entities[i]);
					count++;

					if (count >= 300)
					{
						database.SaveChanges();
						database.Dispose();
						database = client.GetDatabase<IContosoDatabase>();
						count = 0;
					}

					var percent = Math.Round(i / (double) testCount * 100.0, 1);

					if (Math.Abs(percent - lastPercent) > double.Epsilon)
					{
						lastPercent = percent;
						_worker.ReportProgress((int) MainViewWorkerStatus.UpdateResultProgress, percent);
					}

					if (_worker.CancellationPending)
					{
						break;
					}
				}

				database.SaveChanges();
				database.Dispose();
			}

			_worker.ReportProgress((int) MainViewWorkerStatus.StopResult);
		}

		private static void AddAddresses(int testCount, ISyncClient client, MainViewModel view)
		{
			var entities = new List<AddressEntity>(testCount);

			for (var i = 1; i <= testCount; i++)
			{
				entities.Add(EntityFactory.GetAddress(x => x.Line1 = $"Address {i} Line 1"));
			}

			StartResult($"{client.Name}: Adding addresses to database");

			var database = client.GetDatabase<IContosoDatabase>();

			if (view.UseBulkProcessing)
			{
				database.Addresses.BulkAddOrUpdate(entities);
			}
			else
			{
				var lastPercent = -1.0;
				var count = 0;

				for (var i = 0; i < entities.Count; i++)
				{
					database.Addresses.Add(entities[i]);
					count++;

					if (count >= 300)
					{
						database.SaveChanges();
						database.Dispose();
						database = client.GetDatabase<IContosoDatabase>();
						count = 0;
					}

					var percent = Math.Round(i / (double) testCount * 100.0, 1);

					if (Math.Abs(percent - lastPercent) > double.Epsilon)
					{
						lastPercent = percent;
						_worker.ReportProgress((int) MainViewWorkerStatus.UpdateResultProgress, percent);
					}

					if (_worker.CancellationPending)
					{
						break;
					}
				}

				database.SaveChanges();
				database.Dispose();
			}

			_worker.ReportProgress((int) MainViewWorkerStatus.StopResult);
		}

		private static void BenchmarkSyncEngine(BackgroundWorker worker, MainViewModel view)
		{
			foreach (var (server, client) in GetScenarios())
			{
				_worker.ReportProgress((int) MainViewWorkerStatus.Log, $"{server.Name}: Clearing the database...");
				_worker.ReportProgress((int) MainViewWorkerStatus.Log, $"{client.Name}: Clearing the database...");

				if (view.CreateData)
				{
					AddAddresses(view.AddressCount, server, view);

					using var database = (IContosoDatabase) server.GetDatabase();
					var address = database.Addresses.FirstOrDefault();

					AddAccounts(view.AccountCount, server, view, address);
				}

				if (view.SyncData)
				{
					client.Options.EnablePrimaryKeyCache = view.CachePrimaryKeys;
					client.Options.PrimaryKeyCacheTimeout = TimeSpan.FromMinutes(1);
					server.Options.EnablePrimaryKeyCache = view.CachePrimaryKeys;
					server.Options.PrimaryKeyCacheTimeout = TimeSpan.FromMinutes(1);

					SyncAddresses(worker, client, server);
				}
			}
		}

		private static void ClearDatabase(this ISyncClient client)
		{
			using var database = client.GetDatabase();

			if (database is EntityFrameworkDatabase efDatabase)
			{
				efDatabase.ClearDatabase();
			}
		}

		private static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			database.Database.Migrate();
			//var command = "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF ''?'' NOT LIKE ''%MigrationHistory%'' AND ''?'' NOT LIKE ''%MigrationsHistory%'' DELETE FROM ?'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? ENABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";
			var command = @"
DELETE FROM [Settings];
DELETE FROM [PetType];
DELETE FROM [Pets];
DELETE FROM [LogEvents];
DELETE FROM [Groups];
DELETE FROM [GroupMembers];
DELETE FROM [FoodRelationships];
DELETE FROM [Foods];
DELETE FROM [Accounts];
DELETE FROM [Addresses];
";
			database.Database.ExecuteSqlRaw(command);
			return database;
		}

		private static IEnumerable<(SyncClient server, SyncClient client)> GetScenarios()
		{
			//yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			//yield return (new SyncClient("Server (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()), new SyncClient("Client (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()));
			//yield return (new SyncClient("Server (Sqlite)", DatabaseBenchmark.GetEntityFrameworkSqliteProvider()), new SyncClient("Client (mem)", DatabaseBenchmark.GetSyncableMemoryProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (Sqlite2)", GetEntityFrameworkSqliteProvider2()));

			yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider()));
			//yield return (new SyncClient("Server (Sqlite)", GetEntityFrameworkSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider()));

			//yield return (new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (SQL2)", GetSyncableEntityFrameworkProvider2()));
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider()
		{
			using var database = ContosoSqlDatabase.UseSql(DefaultConnection);
			_worker.ReportProgress((int) MainViewWorkerStatus.Log, "Clearing the database...");
			database.Database.Migrate();
			database.ClearDatabase();
			return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x), ContosoDatabase.GetDefaultOptions());
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider2()
		{
			using var database = ContosoSqlDatabase.UseSql(DefaultConnection2);
			_worker.ReportProgress((int) MainViewWorkerStatus.Log, "Clearing the database...");
			database.Database.Migrate();
			database.ClearDatabase();
			return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x), ContosoDatabase.GetDefaultOptions());
		}

		private static ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options);
			return new SyncDatabaseProvider(x => database, options ?? ContosoDatabase.GetDefaultOptions());
		}

		private static BenchmarkResult StartResult(string name)
		{
			var result = new BenchmarkResult { Name = name };
			_worker.ReportProgress((int) MainViewWorkerStatus.AddResult, result);
			return result;
		}

		private static void SyncAddresses(BackgroundWorker worker, ISyncClient client, ISyncClient server)
		{
			StartResult($"{client.Name} <> {server.Name}");

			var options = new SyncOptions { ItemsPerSyncRequest = 600 };
			var engine = new SyncEngine(client, server, options);
			var watch = Stopwatch.StartNew();

			engine.SyncStateChanged += (sender, state) =>
			{
				_worker.ReportProgress((int) MainViewWorkerStatus.Log, $"{watch.Elapsed} : {state.Status} - {state.Count} / {state.Total} {state.Message}");
				_worker.ReportProgress((int) MainViewWorkerStatus.UpdateResultProgress, state.Percent);
				watch.Restart();
			};

			var task = engine.RunAsync();

			while (!worker.CancellationPending && !task.IsCompleted())
			{
				// Wait for sync engine...
			}

			if (!task.IsCompleted())
			{
				engine.Stop();
			}

			_worker.ReportProgress((int) MainViewWorkerStatus.StopResult);

			var builder = new StringBuilder();

			foreach (var issue in engine.SyncIssues)
			{
				builder.AppendLine($"{issue.Id}: {issue.IssueType} - {issue.Message}");
			}

			_worker.ReportProgress((int) MainViewWorkerStatus.Log, builder.ToString());
		}

		private static void Test()
		{
			var total = 0.0;
			var loop = 100;
			var delay = 25;

			for (var i = 0; i < 2; i++)
			{
				var count = 0.0;
				var result = new BenchmarkResult { Name = $"Worker {i}" };

				_worker.ReportProgress((int) MainViewWorkerStatus.Log, result);

				while (count < loop)
				{
					if (count++ < loop)
					{
						result.Percent = count / loop * 100;
						_worker.ReportProgress((int) (total / (loop * 2) * 100));
					}

					result.Percent = count;
					total++;

					if (_worker.CancellationPending)
					{
						return;
					}

					Thread.Sleep(delay);
				}

				result.Percent = 100;
			}
		}

		#endregion
	}
}