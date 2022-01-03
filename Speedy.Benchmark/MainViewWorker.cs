#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Sync;
using Speedy.UnitTests.Factories;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Timer = Speedy.Profiling.Timer;

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

			BenchmarkSyncEngine(_worker, mainViewModel);

			_worker.ReportProgress(100);
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider(DatabaseKeyCache keyCache)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, null, null);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			return new SyncableDatabaseProvider((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), ContosoDatabase.GetDefaultOptions(), keyCache);
		}

		public static ISyncableDatabaseProvider GetEntityFrameworkSqliteProvider2(DatabaseKeyCache keyCache)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2, null, null);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			return new SyncableDatabaseProvider((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), ContosoDatabase.GetDefaultOptions(), keyCache);
		}

		private static void AddAccounts(int testCount, SyncClient client, MainViewModel view, AddressEntity address)
		{
			var entities = new List<AccountEntity>(testCount);
			var database = client.GetDatabase<IContosoDatabase>();
			var start = database.Accounts.Count();

			for (var i = 1; i <= testCount; i++)
			{
				var offset = start + i;
				var account = EntityFactory.GetAccount(x => { }, $"Account {offset}", address);

				if (view.IncrementSyncIds)
				{
					account.SyncId = offset.ToGuid();
				}

				entities.Add(account);
			}

			StartResult($"{client.Name}: Adding accounts to database");

			if (view.UseBulkProcessing)
			{
				database.Accounts.BulkAddOrUpdate(entities);
				client.DatabaseProvider.KeyCache.Initialize(database);
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
			var database = client.GetDatabase<IContosoDatabase>();
			var start = database.Addresses.Count();

			for (var i = 1; i <= testCount; i++)
			{
				var offset = i + start;
				var address = EntityFactory.GetAddress(x => x.Line1 = $"Address {offset} Line 1");

				if (view.IncrementSyncIds)
				{
					address.SyncId = offset.ToGuid();
				}

				entities.Add(address);
			}

			StartResult($"{client.Name}: Adding addresses to database");

			if (view.UseBulkProcessing)
			{
				database.Addresses.BulkAddOrUpdate(entities);
				client.DatabaseProvider.KeyCache.Initialize(database);
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
			var clientKeyManager = view.UseKeyManagerForClient ? new DatabaseKeyCache(TimeSpan.MaxValue) : null;
			var serverKeyManager = view.UseKeyManagerForServer ? new DatabaseKeyCache(TimeSpan.FromMinutes(15)) : null;

			foreach (var (client, server) in GetScenarios(view, clientKeyManager, serverKeyManager))
			{
				client.Options.EnablePrimaryKeyCache = view.CachePrimaryKeys;
				server.Options.EnablePrimaryKeyCache = view.CachePrimaryKeys;
				server.Options.IsServerClient = true;

				_worker.ReportProgress((int) MainViewWorkerStatus.Log, $"{server.Name}: Clearing the database...");
				_worker.ReportProgress((int) MainViewWorkerStatus.Log, $"{client.Name}: Clearing the database...");

				//
				// Server -> Client
				//

				if (view.CreateData)
				{
					AddAddresses(view.AddressCount, server, view);

					using var database = (IContosoDatabase) server.GetDatabase();
					var address = database.Addresses.FirstOrDefault();

					AddAccounts(view.AccountCount, server, view, address);

					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
				}

				if (view.SyncData)
				{
					var timer = Timer.Create(() => Sync(worker, client, server, view));
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
					worker.ReportProgress((int) MainViewWorkerStatus.Log, server.Profiler.ToString(timer.Elapsed));
					worker.ReportProgress((int) MainViewWorkerStatus.Log, client.Profiler.ToString(timer.Elapsed));
				}

				if (view.UseVerboseLogging)
				{
					PrintClientDetails(worker, client);
					PrintClientDetails(worker, server);
				}
				else
				{
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "Client Cached Items: " + client.DatabaseProvider.KeyCache.TotalCachedItems);
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "Server Cached Items: " + server.DatabaseProvider.KeyCache.TotalCachedItems);
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
				}

				//
				// Client -> Server
				//

				if (view.CreateData)
				{
					AddAddresses(view.AddressCount, client, view);

					using var database = (IContosoDatabase) client.GetDatabase();
					var address = database.Addresses.FirstOrDefault();

					AddAccounts(view.AccountCount, client, view, address);

					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
				}

				if (view.SyncData)
				{
					var timer = Timer.Create(() => Sync(worker, client, server, view));
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
					worker.ReportProgress((int) MainViewWorkerStatus.Log, server.Profiler.ToString(timer.Elapsed));
					worker.ReportProgress((int) MainViewWorkerStatus.Log, client.Profiler.ToString(timer.Elapsed));
				}

				if (view.UseVerboseLogging)
				{
					PrintClientDetails(worker, client);
					PrintClientDetails(worker, server);
				}
				else
				{
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "Client Cached Items: " + client.DatabaseProvider.KeyCache.TotalCachedItems);
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "Server Cached Items: " + server.DatabaseProvider.KeyCache.TotalCachedItems);
					worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
				}
			}
		}

		private static void PrintClientDetails(BackgroundWorker worker, SyncClient client)
		{
			using var database = client.GetDatabase<IContosoDatabase>();
			worker.ReportProgress((int) MainViewWorkerStatus.Log, client.Name);
			var addresses = database.Addresses.Select(x => $"{x.SyncId}-{x.Id}").ToList();
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "\tAddresses: " + addresses.Count);
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "\t\t" + string.Join("\r\n\t\t", addresses));
			var accounts = database.Accounts.Select(x => $"{x.SyncId}-{x.Id}").ToList();
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "\tAccounts: " + accounts.Count);
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "\t\t" + string.Join("\r\n\t\t", accounts));
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "Total Cached Items");
			worker.ReportProgress((int) MainViewWorkerStatus.Log, client.DatabaseProvider.KeyCache.ToDetailedString());
			worker.ReportProgress((int) MainViewWorkerStatus.Log, "");
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

		private static IEnumerable<(SyncClient client, SyncClient server)> GetScenarios(MainViewModel view, DatabaseKeyCache clientKeyCache = null, DatabaseKeyCache serverKeyCache = null)
		{
			if (view.Scenarios.SqlToSql)
			{
				yield return (new SyncClient("Client (SQL)", GetSyncableEntityFrameworkProvider2(clientKeyCache)),
					new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider(serverKeyCache)));
			}

			if (view.Scenarios.SqliteToSql)
			{
				yield return (new SyncClient("Client (Sqlite)", GetEntityFrameworkSqliteProvider(clientKeyCache)),
					new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider(serverKeyCache)));
			}

			if (view.Scenarios.MemToSql)
			{
				yield return (new SyncClient("Client (mem)", GetSyncableMemoryProvider(clientKeyCache)),
					new SyncClient("Server (SQL)", GetSyncableEntityFrameworkProvider(serverKeyCache)));
			}

			if (view.Scenarios.SqlToMem)
			{
				yield return (new SyncClient("Client (sql)", GetSyncableEntityFrameworkProvider(clientKeyCache)),
					new SyncClient("Server (mem)", GetSyncableMemoryProvider(serverKeyCache)));
			}

			if (view.Scenarios.MemToMem)
			{
				yield return (new SyncClient("Client (mem)", GetSyncableMemoryProvider(clientKeyCache)),
					new SyncClient("Server (mem)", GetSyncableMemoryProvider(serverKeyCache)));
			}
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider(DatabaseKeyCache keyCache)
		{
			using var database = ContosoSqlDatabase.UseSql(DefaultConnection, null, null);
			_worker.ReportProgress((int) MainViewWorkerStatus.Log, "Clearing the database...");
			database.Database.Migrate();
			database.ClearDatabase();
			return new SyncableDatabaseProvider((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), ContosoDatabase.GetDefaultOptions(), keyCache);
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider2(DatabaseKeyCache keyCache)
		{
			using var database = ContosoSqlDatabase.UseSql(DefaultConnection2, null, null);
			_worker.ReportProgress((int) MainViewWorkerStatus.Log, "Clearing the database...");
			database.Database.Migrate();
			database.ClearDatabase();
			return new SyncableDatabaseProvider((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), ContosoDatabase.GetDefaultOptions(), keyCache);
		}

		private static ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseKeyCache keyCache, DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options, keyCache);
			database.Options.DisableEntityValidations = true;
			return new SyncableDatabaseProvider((x, y) => database, options ?? ContosoDatabase.GetDefaultOptions(), keyCache);
		}

		private static BenchmarkResult StartResult(string name)
		{
			var result = new BenchmarkResult { Name = name };
			_worker.ReportProgress((int) MainViewWorkerStatus.AddResult, result);
			return result;
		}

		private static void Sync(BackgroundWorker worker, ISyncClient client, ISyncClient server, MainViewModel view)
		{
			Thread.Sleep(10);

			StartResult($"{client.Name} <> {server.Name}");

			var options = new SyncOptions
			{
				ItemsPerSyncRequest = view.ItemsPerSync,
				LastSyncedOnClient = view.LastSyncedOnClient,
				LastSyncedOnServer = view.LastSyncedOnServer
			};

			using var test = LogListener.CreateSession(Guid.Empty, view.UseVerboseLogging ? EventLevel.Verbose : EventLevel.Informational);
			test.EventWritten += (sender, args) => _worker.ReportProgress((int) MainViewWorkerStatus.Log, args.GetDetailedMessage());

			using var engine = new SyncEngine(client, server, options);
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

			if (builder.Length > 0)
			{
				_worker.ReportProgress((int) MainViewWorkerStatus.Log, builder.ToString());
			}

			view.LastSyncedOnClient = engine.Options.LastSyncedOnClient;
			view.LastSyncedOnServer = engine.Options.LastSyncedOnServer;
		}

		#endregion
	}
}