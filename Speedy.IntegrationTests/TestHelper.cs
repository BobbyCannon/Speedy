#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Client.Data;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Enumerations;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Services;
using Timer = Speedy.Profiling.Timer;

#endregion

namespace Speedy.IntegrationTests
{
	public static class TestHelper
	{
		#region Constants

		public const string AdministratorEmailAddress = "admin@speedy.local";

		public const int AdministratorId = 1;
		public const string AdministratorPassword = "Password";

		#endregion

		#region Fields

		public static readonly DirectoryInfo Directory;

		#endregion

		#region Constructors

		static TestHelper()
		{
			Directory = new DirectoryInfo($"{Path.GetTempPath()}\\SpeedyTests");
			DefaultSqlConnection = "server=localhost;database=Speedy;integrated security=true;";
			DefaultSqlConnection2 = "server=localhost;database=Speedy2;integrated security=true;";
			DefaultSqliteConnection = "Data Source=Speedy.db";
			DefaultSqliteConnection2 = "Data Source=Speedy2.db";

			var hostEntries = new List<string> { "speedy.local" };

			var lines = File.ReadAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts")
				.Select(x => x.Trim())
				.Where(x => !x.StartsWith("#") && x.Length > 0)
				.ToList();

			var missing = hostEntries.Where(x => !lines.Any(y => y.Contains(x))).ToList();
			if (missing.Count > 0)
			{
				Console.WriteLine("Be sure to host file for development mode. Missing entries are:");
				missing.ForEach(x => Console.WriteLine($"\t\t{x}"));
				throw new Exception("Be sure to host file for development mode.");
			}
		}

		#endregion

		#region Properties

		public static string ClearDatabaseScript => "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF ''?'' NOT LIKE ''%MigrationHistory%'' AND ''?'' NOT LIKE ''%MigrationsHistory%'' DELETE FROM ?'\r\nEXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";

		public static string DefaultSqlConnection { get; }

		public static string DefaultSqlConnection2 { get; }

		public static string DefaultSqliteConnection { get; }

		public static string DefaultSqliteConnection2 { get; }

		#endregion

		#region Methods

		public static void AddSaveAndCleanup<T, T2>(this IContosoDatabase database, T item) where T : Entity<T2>
		{
			database.Add<T, T2>(item);
			database.SaveChanges();
			database.Dispose();

			// Because sync is based on "until" (less than, not equal) then we must delay at least a millisecond to delay the data.
			Thread.Sleep(1);
		}
		public static void RemoveSaveAndCleanup<T, T2>(this IContosoDatabase database, T item) where T : Entity<T2>
		{
			database.Remove<T, T2>(item);
			database.SaveChanges();
			database.Dispose();

			// Because sync is based on "until" (less than, not equal) then we must delay at least a millisecond to delay the data.
			Thread.Sleep(1);
		}

		/// <summary>
		/// Compares two objects to see if they are equal.
		/// </summary>
		/// <typeparam name="T"> The type of the object. </typeparam>
		/// <param name="expected"> The item that is expected. </param>
		/// <param name="actual"> The item that is to be tested. </param>
		/// <param name="membersToIgnore"> Optional members to ignore. </param>
		public static void AreEqual<T>(T[] expected, T[] actual, params string[] membersToIgnore)
		{
			if (expected.Length != actual.Length)
			{
				throw new Exception($"Expected ({expected.Length}) != Actual ({actual.Length}) Length");
			}

			for (var i = 0; i < expected.Length; i++)
			{
				AreEqual(expected[i], actual[i], true, membersToIgnore);
			}
		}

		/// <summary>
		/// Compares two objects to see if they are equal.
		/// </summary>
		/// <typeparam name="T"> The type of the object. </typeparam>
		/// <param name="expected"> The item that is expected. </param>
		/// <param name="actual"> The item that is to be tested. </param>
		/// <param name="membersToIgnore"> Optional members to ignore. </param>
		public static void AreEqual<T>(T expected, T actual, params string[] membersToIgnore)
		{
			AreEqual(expected, actual, true, membersToIgnore);
		}

		/// <summary>
		/// Compares two objects to see if they are equal.
		/// </summary>
		/// <typeparam name="T"> The type of the object. </typeparam>
		/// <param name="expected"> The item that is expected. </param>
		/// <param name="actual"> The item that is to be tested. </param>
		/// <param name="includeChildren"> True to include child complex types. </param>
		/// <param name="membersToIgnore"> Optional members to ignore. </param>
		public static void AreEqual<T>(T expected, T actual, bool includeChildren = true, params string[] membersToIgnore)
		{
			var compareObjects = new CompareLogic
			{
				Config =
				{
					MaxDifferences = int.MaxValue,
					CompareChildren = includeChildren
				}
			};

			if (membersToIgnore.Any())
			{
				compareObjects.Config.MembersToIgnore = membersToIgnore.ToList();
			}

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		public static void Cleanup()
		{
			Directory.SafeDelete();
		}

		public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
		{
			database.Database.ExecuteSqlRaw(ClearDatabaseScript);
			return database;
		}

		public static void Dump(this object item)
		{
			Console.WriteLine(item);
		}

		public static void Dump(this byte[] item)
		{
			foreach (var i in item)
			{
				Debug.Write($"{i:X2},");
			}

			Console.WriteLine("");
		}

		public static void Dump<T>(this T item, string prefix)
		{
			Console.Write(prefix);
			Console.WriteLine(item);
		}

		public static void ExpectedException<T>(Action work, params string[] errorMessage) where T : Exception
		{
			try
			{
				work();
			}
			catch (T ex)
			{
				var details = ex.ToDetailedString();
				if (errorMessage.Any(x => details.Contains(x)))
				{
					return;
				}

				Assert.Fail($"Exception message did not contain expected error. {details}");
			}

			Assert.Fail("The expected exception was not thrown.");
		}

		public static IAuthenticationService GetAuthenticationService()
		{
			var service = new Mock<IAuthenticationService>();
			service.Setup(x => x.LogIn(It.IsAny<Credentials>())).Returns<Credentials>(x => true);
			return service.Object;
		}

		public static ISyncableDatabaseProvider<ContosoClientMemoryDatabase> GetClientProvider()
		{
			var database = new ContosoClientMemoryDatabase();
			return new SyncableDatabaseProvider<ContosoClientMemoryDatabase>((x, y) =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, ContosoClientDatabase.GetDefaultOptions(), null);
		}

		public static ControllerContext GetControllerContext(AccountEntity account)
		{
			var ticket = AuthenticationService.CreateTicket(account, true, CookieAuthenticationDefaults.AuthenticationScheme);
			return new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = ticket.Principal }
			};
		}

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContexts(DatabaseOptions options = null, DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			yield return GetSqlProvider(options, initialize);
			yield return GetSqliteProvider(options, initialize);
			yield return GetMemoryProvider(options, keyCache, initialize);
		}

		public static IDispatcher GetDispatcher()
		{
			var dispatcher = new Mock<IDispatcher>();
			dispatcher.Setup(x => x.HasThreadAccess).Returns(true);
			return dispatcher.Object;
		}

		public static IDatabaseProvider<IContosoDatabase> GetMemoryProvider(DatabaseOptions options = null, DatabaseKeyCache keyCache = null, bool initialized = true)
		{
			var database = new ContosoMemoryDatabase(options, keyCache);

			if (initialized)
			{
				InitializeDatabase(database, keyCache);
			}

			return new DatabaseProvider<IContosoDatabase>(x =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, ContosoDatabase.GetDefaultOptions());
		}

		public static T GetRandomItem<T>(this IEnumerable<T> collection, T exclude = null) where T : class
		{
			var random = new Random();
			var list = collection.ToList();
			if (!list.Any() || exclude != null && list.Count == 1)
			{
				return null;
			}

			var index = random.Next(0, list.Count);

			while (list[index] == exclude)
			{
				index++;

				if (index >= list.Count)
				{
					index = 0;
				}
			}

			return list[index];
		}

		public static IDatabaseProvider<ContosoDatabase> GetSqliteProvider(DatabaseOptions options = null, bool initialized = true, DatabaseKeyCache keyCache = null)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, options, null);
			database.Database.EnsureDeleted();
			database.Database.Migrate();

			if (initialized)
			{
				InitializeDatabase(database, keyCache);
			}

			return new DatabaseProvider<ContosoDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x, keyCache), options);
		}

		public static IDatabaseProvider<ContosoDatabase> GetSqlProvider(DatabaseOptions options = null, bool initialize = true, DatabaseKeyCache keyCache = null)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, options, null);
			database.Database.Migrate();
			database.ClearDatabase();

			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}

			return new DatabaseProvider<ContosoDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x, keyCache), options);
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableMemoryProvider(DatabaseOptions options = null, DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			var database = new ContosoMemoryDatabase(options, keyCache);

			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}

			return new SyncableDatabaseProvider<IContosoDatabase>((x, y) =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, database.Options, keyCache);
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqliteProvider(DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, null, null);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}
			return new SyncableDatabaseProvider<ContosoSqliteDatabase>((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqliteProvider2(DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2, null, null);
			database.Database.EnsureDeleted();
			database.Database.Migrate();
			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}
			return new SyncableDatabaseProvider<ContosoSqliteDatabase>((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqlProvider(DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, null, null);
			database.Database.Migrate();
			database.ClearDatabase();
			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}
			return new SyncableDatabaseProvider<ContosoSqlDatabase>((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
		}

		public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqlProvider2(DatabaseKeyCache keyCache = null, bool initialize = true)
		{
			// Do not use the cache during migration and clearing of the database
			using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection2, null, null);
			database.Database.Migrate();
			database.ClearDatabase();
			if (initialize)
			{
				InitializeDatabase(database, keyCache);
			}
			return new SyncableDatabaseProvider<ContosoSqlDatabase>((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
		}

		public static ISyncClient GetSyncClient(string name, DatabaseType type, bool initializeDatabase, bool useKeyCache, bool useSecondaryConnection)
		{
			switch (type)
			{
				case DatabaseType.Memory:
					return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
						GetSyncableMemoryProvider(null, useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase));

				case DatabaseType.Sql:
					return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
						useSecondaryConnection
							? GetSyncableSqlProvider2(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
							: GetSyncableSqlProvider(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
					);

				case DatabaseType.Sqlite:
					return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
						useSecondaryConnection
							? GetSyncableSqliteProvider2(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
							: GetSyncableSqliteProvider(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
					);

				default:
				case DatabaseType.Unknown:
					throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}

		public static void Initialize()
		{
			Wait(() =>
			{
				try
				{
					Directory.SafeDelete();
					Directory.SafeCreate();
					return true;
				}
				catch
				{
					return false;
				}
			});
		}

		public static void TestServerAndClients(Action<ISyncClient, ISyncClient> action, bool includeWeb = true, bool initializeDatabase = true)
		{
			GetServerClientScenarios(includeWeb, initializeDatabase).ForEach(x =>
			{
				Console.Write($"{x.server.Name} -> {x.client.Name}: ");
				action(x.server, x.client);
				Console.WriteLine($"{x.timer.Elapsed}");
			});
		}

		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.AppendLine(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		private static IEnumerable<(Timer timer, ISyncClient server, ISyncClient client)> GetServerClientScenarios(bool includeWeb, bool initializeDatabase)
		{
			static (Timer timer, ISyncClient server, ISyncClient client) Process(Timer timer, ISyncClient server, ISyncClient client)
			{
				server.Options.IsServerClient = true;
				return (timer, server, client);
			}

			static (Timer timer, ISyncClient server, ISyncClient client) Process2((Timer timer, ISyncClient server, ISyncClient client) scenario)
			{
				return Process(scenario.timer, scenario.server, scenario.client);
			}

			var scenarios = new List<(DatabaseType server, DatabaseType client)>
			{
				new(DatabaseType.Memory, DatabaseType.Memory),
				new(DatabaseType.Memory, DatabaseType.Sql),
				new(DatabaseType.Memory, DatabaseType.Sqlite),
				new(DatabaseType.Sql, DatabaseType.Memory),
				new(DatabaseType.Sql, DatabaseType.Sql),
				new(DatabaseType.Sql, DatabaseType.Sqlite),
				new(DatabaseType.Sqlite, DatabaseType.Memory),
				new(DatabaseType.Sqlite, DatabaseType.Sql),
				new(DatabaseType.Sqlite, DatabaseType.Sqlite)
			};

			foreach (var (server, client) in scenarios)
			{
				yield return Process2(GetSyncClients(server, false, client, false, initializeDatabase));
				yield return Process2(GetSyncClients(server, true, client, false, initializeDatabase));
				yield return Process2(GetSyncClients(server, false, client, true, initializeDatabase));
				yield return Process2(GetSyncClients(server, true, client, true, initializeDatabase));
			}

			if (includeWeb)
			{
				var outgoingConverter = new SyncClientOutgoingConverter(
					new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
					new SyncObjectOutgoingConverter<AccountEntity, int, Account, int>(),
					new SyncObjectOutgoingConverter<LogEventEntity, long, LogEvent, long>(),
					new SyncObjectOutgoingConverter<SettingEntity, long, Setting, long>()
				);

				var incomingConverter = new SyncClientIncomingConverter(
					new SyncObjectIncomingConverter<Address, long, AddressEntity, long>(),
					new SyncObjectIncomingConverter<Account, int, AccountEntity, int>(),
					new SyncObjectIncomingConverter<LogEvent, long, LogEventEntity, long>(),
					new SyncObjectIncomingConverter<Setting, long, SettingEntity, long>()
				);

				var credential = new NetworkCredential("admin@speedy.local", "Password");
				const string serverUri = "https://speedy.local";
				const int timeout = 60000;

				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (MEM, Cached)", GetSyncableMemoryProvider(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (SQL2)", GetSyncableSqlProvider2(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (SQL2, Cached)", GetSyncableSqlProvider2(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return Process(Timer.StartNew(),
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), serverUri, credential: credential, timeout: timeout),
					new SyncClient("Client (Sqlite, Cached)", GetSyncableSqliteProvider(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			}
		}

		private static (Timer timer, ISyncClient server, ISyncClient client) GetSyncClients(DatabaseType scenarioServer, bool cacheServer, DatabaseType scenarioClient, bool cacheClient, bool initializeDatabase)
		{
			var timer = new Timer();
			timer.Start();
			var server = GetSyncClient("Server", scenarioServer, initializeDatabase, cacheServer, false);
			var client = GetSyncClient("Client", scenarioClient, initializeDatabase, cacheClient, scenarioServer == scenarioClient);
			return (timer, server, client);
		}

		private static void InitializeDatabase(IContosoDatabase database, DatabaseKeyCache keyCache)
		{
			var address = new AddressEntity
			{
				City = "City",
				Line1 = "Line1",
				Line2 = "Line2",
				Postal = "12345",
				State = "ST",
				SyncId = Guid.Parse("BDCC2C49-BCE5-49B4-8268-1EFD1E434F79")
			};

			database.Accounts.Add(new AccountEntity
			{
				Address = address,
				Name = "Administrator",
				EmailAddress = AdministratorEmailAddress,
				PasswordHash = AccountService.Hash(AdministratorPassword, AdministratorId.ToString()),
				Roles = BaseService.CombineTags(AccountRole.Administrator),
				SyncId = Guid.Parse("56CF7B5C-4C5A-462C-939D-A1F387A7483C")
			});

			database.SaveChanges();

			keyCache?.Initialize(database);
		}

		private static string ToDetailedString(this Exception ex)
		{
			var builder = new StringBuilder();
			AddExceptionToBuilder(builder, ex);
			return builder.ToString();
		}

		/// <summary>
		/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
		/// time.
		/// </summary>
		/// <param name="action"> The action to call. </param>
		/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
		/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		private static bool Wait(Func<bool> action, double timeout = 1000, int delay = 50)
		{
			var watch = Stopwatch.StartNew();
			var watchTimeout = TimeSpan.FromMilliseconds(timeout);
			var result = false;

			while (!result)
			{
				if (watch.Elapsed > watchTimeout)
				{
					return false;
				}

				result = action();
				if (!result)
				{
					Thread.Sleep(delay);
				}
			}

			return true;
		}

		#endregion
	}
}