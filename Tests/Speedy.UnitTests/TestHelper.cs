﻿#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Moq;
using Speedy.Automation;
using Speedy.Automation.Tests;
using Speedy.Automation.Web;
using Speedy.Client.Data;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Core.Services;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Enumerations;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Data.Sync;
using Timer = Speedy.Profiling.Timer;
#if !NET48
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Speedy.Website.Services;
#endif

#endregion

namespace Speedy.UnitTests;

public static class TestHelper
{
	#region Constants

	public const string AdministratorEmailAddress = "admin@speedy.local";
	public const int AdministratorId = 1;
	public const string AdministratorPassword = "Password";
	public const BrowserType BrowsersToTests = BrowserType.Chrome; //BrowserType.Chrome | BrowserType.Edge;

	#endregion

	#region Fields

	public static readonly DirectoryInfo Directory;

	#endregion

	#region Constructors

	static TestHelper()
	{
		Directory = new DirectoryInfo($"{Path.GetTempPath()}\\SpeedyTests");
		DefaultSqlConnection = "server=localhost;database=Speedy;integrated security=true;encrypt=false";
		DefaultSqlConnection2 = "server=localhost;database=Speedy2;integrated security=true;encrypt=false";
		DefaultSqliteConnection = "Data Source=Speedy.db";
		DefaultSqliteConnection2 = "Data Source=Speedy2.db";

		var hostEntries = new List<string> { "speedy.local" };

		var lines = File.ReadAllLines("C:\\Windows\\System32\\drivers\\etc\\hosts")
			.Select(x => x.Trim())
			.Where(x => !x.StartsWith("#") && (x.Length > 0))
			.ToList();

		var missing = hostEntries.Where(x => !lines.Any(y => y.Contains(x))).ToList();
		if (missing.Count > 0)
		{
			Console.WriteLine("Be sure to host file for development mode. Missing entries are:");
			missing.ForEach(x => Console.WriteLine($"\t\t{x}"));
			throw new Exception("Be sure to host file for development mode.");
		}

		var assembly = Assembly.GetExecutingAssembly();
		Version = assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";

		var path = Path.GetDirectoryName(assembly.Location);
		var info = new DirectoryInfo(path ?? "/");

		ApplicationPathForWinFormsX64 = $"{info.FullName.Replace("Speedy.AutomationTests", "Speedy.Winforms.Example")}\\Speedy.Winforms.Example.exe"
			.Replace("net7.0-windows", "net48")
			.Replace("\\bin\\Debug\\", "\\bin\\x64\\Debug\\");

		ApplicationPathForWinFormsX86 = ApplicationPathForWinFormsX64.Replace("\\x64\\", "\\x86\\");

		// Need to convert
		//        C:\Workspaces\GitHub\Speedy\Speedy.AutomationTests\bin\Debug\net7.0-windows
		// - from C:\Workspaces\GitHub\Speedy\Speedy.TestUwp\bin\Debug\net7.0-windows
		// - to   C:\Workspaces\GitHub\Speedy\Samples\Windows\Speedy.Uwp.Example\bin\x64\Debug\AppX\Speedy.Uwp.Example.exe
		ApplicationPathForUwp = info.FullName
				.Replace("Speedy.AutomationTests", "Samples\\Windows\\Speedy.Uwp.Example")
				.Replace("Debug\\net7.0-windows", "x64\\Debug\\AppX")
			+ "\\Speedy.Uwp.Example.exe";

		SpeedyTest.SetClipboardProvider(x => Clipboard.SetText(x ?? "null"));

		Initialize();
	}

	#endregion

	#region Properties

	public static string ApplicationPathForUwp { get; }

	public static string ApplicationPathForWinFormsX64 { get; }

	public static string ApplicationPathForWinFormsX86 { get; }

	public static string ClearDatabaseScript => "EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? DISABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'SET QUOTED_IDENTIFIER ON; IF ''?'' NOT LIKE ''%MigrationHistory%'' AND ''?'' NOT LIKE ''%MigrationsHistory%'' DELETE FROM ?'\r\nEXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'\r\nEXEC sp_MSForEachTable 'ALTER TABLE ? CHECK CONSTRAINT ALL'\r\nEXEC sp_MSForEachTable 'IF OBJECTPROPERTY(object_id(''?''), ''TableHasIdentity'') = 1 DBCC CHECKIDENT (''?'', RESEED, 0)'";

	public static string DefaultSqlConnection { get; }

	public static string DefaultSqlConnection2 { get; }

	public static string DefaultSqliteConnection { get; }

	public static string DefaultSqliteConnection2 { get; }

	public static string Version { get; }

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

	public static void Cleanup()
	{
		Directory.SafeDelete();
	}

	public static T ClearDatabase<T>(this T database) where T : EntityFrameworkDatabase
	{
		database.Database.ExecuteSqlRaw(ClearDatabaseScript);
		return database;
	}

	/// <summary>
	/// Compares two objects to see if they are equal.
	/// </summary>
	/// <typeparam name="T"> The type of the object. </typeparam>
	/// <param name="expected"> The item that is expected. </param>
	/// <param name="actual"> The item that is to be tested. </param>
	/// <param name="includeChildren"> True to include child complex types. </param>
	/// <param name="membersToIgnore"> Optional members to ignore. </param>
	public static ComparisonResult Compare<T>(T expected, T actual, bool includeChildren = true, params string[] membersToIgnore)
	{
		return Compare(expected, actual, includeChildren, membersToIgnore.ToList());
	}

	/// <summary>
	/// Compares two objects to see if they are equal.
	/// </summary>
	/// <typeparam name="T"> The type of the object. </typeparam>
	/// <param name="expected"> The item that is expected. </param>
	/// <param name="actual"> The item that is to be tested. </param>
	/// <param name="includeChildren"> True to include child complex types. </param>
	/// <param name="membersToIgnore"> Optional members to ignore. </param>
	public static ComparisonResult Compare<T>(T expected, T actual, bool includeChildren, IEnumerable<string> membersToIgnore)
	{
		return Compare(expected, actual, includeChildren, membersToIgnore.ToList());
	}

	/// <summary>
	/// Compares two objects to see if they are equal.
	/// </summary>
	/// <typeparam name="T"> The type of the object. </typeparam>
	/// <param name="expected"> The item that is expected. </param>
	/// <param name="actual"> The item that is to be tested. </param>
	/// <param name="includeChildren"> True to include child complex types. </param>
	/// <param name="membersToIgnore"> Optional members to ignore. </param>
	public static ComparisonResult Compare<T>(T expected, T actual, bool includeChildren, List<string> membersToIgnore)
	{
		var compareObjects = new CompareLogic
		{
			Config =
			{
				CompareChildren = includeChildren,
				IgnoreObjectTypes = true,
				MaxDifferences = int.MaxValue,
				MaxStructDepth = 3
			}
		};

		if (membersToIgnore.Any())
		{
			compareObjects.Config.MembersToIgnore = membersToIgnore;
		}

		return compareObjects.Compare(expected, actual);
	}

	public static T CopyToClipboard<T>(this T value)
	{
		var thread = new Thread(() =>
		{
			try
			{
				Clipboard.SetText(value?.ToString() ?? "null");
			}
			catch
			{
				// Ignore the clipboard set issue...
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		return value;
	}

	public static ISyncableDatabaseProvider<ContosoClientMemoryDatabase> GetClientProvider()
	{
		var database = new ContosoClientMemoryDatabase();
		return new SyncableDatabaseProvider<ContosoClientMemoryDatabase>((x, y) =>
		{
			database.Options.UpdateWith(x);
			return database;
		}, ContosoClientMemoryDatabase.GetDefaultOptions(), null);
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
		dispatcher.Setup(x => x.IsDispatcherThread).Returns(true);
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

	public static Automation.Application GetOrStartApplication(bool x86 = false)
	{
		var path = x86 ? ApplicationPathForWinFormsX86 : ApplicationPathForWinFormsX64;
		var response = Automation.Application.AttachOrCreate(path);
		response.Timeout = TimeSpan.FromSeconds(5);
		response.AutoClose = true;
		return response;
	}

	public static T GetRandomItem<T>(this IEnumerable<T> collection, T exclude = null) where T : class
	{
		var random = new Random();
		var list = collection.ToList();
		if (!list.Any() || ((exclude != null) && (list.Count == 1)))
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

	public static IDatabaseProvider<IContosoDatabase> GetSqliteProvider(DatabaseOptions options = null, bool initialized = true, DatabaseKeyCache keyCache = null)
	{
		// Do not use the cache during migration and clearing of the database
		using var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, options, null);
		database.Database.EnsureDeleted();
		database.Database.Migrate();

		if (initialized)
		{
			InitializeDatabase(database, keyCache);
		}

		return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x, keyCache), options);
	}

	public static IDatabaseProvider<IContosoDatabase> GetSqlProvider(DatabaseOptions options = null, bool initialize = true, DatabaseKeyCache keyCache = null)
	{
		// Do not use the cache during migration and clearing of the database
		using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, options, null);
		//database.Database.Migrate();
		database.ClearDatabase();

		if (initialize)
		{
			InitializeDatabase(database, keyCache);
		}

		return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x, keyCache), options);
	}

	public static ISyncableDatabaseProvider<IContosoClientDatabase> GetSyncableClientMemoryProvider(DatabaseOptions options = null, DatabaseKeyCache keyCache = null, bool initialize = true)
	{
		var database = new ContosoClientMemoryDatabase(options, keyCache);

		if (initialize)
		{
			//InitializeDatabase(database, keyCache);
		}

		return new SyncableDatabaseProvider<IContosoClientDatabase>((x, y) =>
		{
			database.Options.UpdateWith(x);
			return database;
		}, database.Options, keyCache);
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
		return new SyncableDatabaseProvider<IContosoDatabase>((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
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
		return new SyncableDatabaseProvider<IContosoDatabase>((x, y) => new ContosoSqliteDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
	}

	public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqlProvider(DatabaseKeyCache keyCache = null, bool initialize = true)
	{
		// Do not use the cache during migration and clearing of the database
		using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, null, null);
		// do not migrate because we have Old EF and New EF migrations
		//database.Database.Migrate();
		database.ClearDatabase();
		if (initialize)
		{
			InitializeDatabase(database, keyCache);
		}
		return new SyncableDatabaseProvider<IContosoDatabase>((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
	}

	public static ISyncableDatabaseProvider<IContosoDatabase> GetSyncableSqlProvider2(DatabaseKeyCache keyCache = null, bool initialize = true)
	{
		// Do not use the cache during migration and clearing of the database
		using var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection2, null, null);
		// do not migrate because we have Old EF and New EF migrations
		//database.Database.Migrate();
		database.ClearDatabase();
		if (initialize)
		{
			InitializeDatabase(database, keyCache);
		}
		return new SyncableDatabaseProvider<IContosoDatabase>((x, y) => new ContosoSqlDatabase(database.DbContextOptions, x, y), database.Options, keyCache);
	}

	public static ISyncClient GetSyncClient(string name, DatabaseType type, bool initializeDatabase, bool useKeyCache, bool useSecondaryConnection,
		SyncClientIncomingConverter incomingConverter, SyncClientOutgoingConverter outgoingConverter)
	{
		switch (type)
		{
			case DatabaseType.Memory:
			{
				return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
						GetSyncableMemoryProvider(null, useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase))
					{ IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter };
			}
			case DatabaseType.Sql:
			{
				return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
					useSecondaryConnection
						? GetSyncableSqlProvider2(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
						: GetSyncableSqlProvider(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
				) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter };
			}
			case DatabaseType.Sqlite:
			{
				return new SyncClient($"{name}: ({type}{(useKeyCache ? ", cached" : "")})",
					useSecondaryConnection
						? GetSyncableSqliteProvider2(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
						: GetSyncableSqliteProvider(useKeyCache ? new DatabaseKeyCache() : null, initializeDatabase)
				) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter };
			}
			default:
			case DatabaseType.Unknown:
			{
				throw new ArgumentOutOfRangeException(nameof(type), type, null);
			}
		}
	}

	public static void Initialize()
	{
		UtilityExtensions.WaitUntil(() =>
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
		}, 1000, 10);
	}

	public static void PrintChildren(Element parent, string prefix = "")
	{
		var element = parent;
		if (element != null)
		{
			Console.WriteLine(prefix + element.ToDetailString().Replace(Environment.NewLine, ", "));
			prefix += "  ";
		}

		foreach (var child in parent.Children)
		{
			PrintChildren(child, prefix);
		}
	}

	/// <summary>
	/// Reads all text from the file info. Uses read access and read/write sharing.
	/// </summary>
	/// <param name="info"> The file info to read all text from. </param>
	/// <returns> The text from the file. </returns>
	public static string ReadAllText(this FileInfo info)
	{
		using (var stream = info.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			var reader = new StreamReader(stream, Encoding.UTF8);
			return reader.ReadToEnd();
		}
	}

	public static void RemoveSaveAndCleanup<T, T2>(this IContosoDatabase database, T item) where T : Entity<T2>
	{
		database.Remove<T, T2>(item);
		database.SaveChanges();
		database.Dispose();

		// Because sync is based on "until" (less than, not equal) then we must delay at least a millisecond to delay the data.
		Thread.Sleep(1);
	}

	public static Automation.Application StartApplication(bool x86 = false)
	{
		var path = x86 ? ApplicationPathForWinFormsX86 : ApplicationPathForWinFormsX64;
		path.Dump();
		Automation.Application.CloseAll(path);
		var response = Automation.Application.AttachOrCreate(path);
		response.Timeout = TimeSpan.FromSeconds(5);
		response.AutoClose = true;
		return response;
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

		//if (ex is DbEntityValidationException entityException)
		//{
		//	foreach (var details in entityException.EntityValidationErrors)
		//	{
		//		foreach (var error in details.ValidationErrors)
		//		{
		//			builder.AppendLine(details.Entry.Entity.GetType().Name + ": " + error.ErrorMessage);
		//		}

		//		builder.AppendLine();
		//	}
		//}

		if (ex.InnerException != null)
		{
			AddExceptionToBuilder(builder, ex.InnerException);
		}
	}

	private static IEnumerable<(Timer timer, ISyncClient server, ISyncClient client)> GetServerClientScenarios(bool includeWeb, bool initializeDatabase)
	{
		var account = new AccountEntity
		{
			Name = "Administrator",
			EmailAddress = AdministratorEmailAddress,
			PasswordHash = AccountService.Hash(AdministratorPassword, AdministratorId.ToString()),
			Roles = BaseService.CombineTags(AccountRole.Administrator),
			SyncId = Guid.Parse("56CF7B5C-4C5A-462C-939D-A1F387A7483C")
		};

		var incomingConverter = ServerSyncClient.GetIncomingConverter(account);
		var outgoingConverter = ServerSyncClient.GetOutgoingConverter();

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
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Memory, DatabaseType.Memory),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Memory, DatabaseType.Sql),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Memory, DatabaseType.Sqlite),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sql, DatabaseType.Memory),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sql, DatabaseType.Sql),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sql, DatabaseType.Sqlite),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sqlite, DatabaseType.Memory),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sqlite, DatabaseType.Sql),
			new ValueTuple<DatabaseType, DatabaseType>(DatabaseType.Sqlite, DatabaseType.Sqlite)
		};

		foreach (var (server, client) in scenarios)
		{
			yield return Process2(GetSyncClients(server, false, client, false, initializeDatabase, incomingConverter, outgoingConverter));
			yield return Process2(GetSyncClients(server, true, client, false, initializeDatabase, incomingConverter, outgoingConverter));
			yield return Process2(GetSyncClients(server, false, client, true, initializeDatabase, incomingConverter, outgoingConverter));
			yield return Process2(GetSyncClients(server, true, client, true, initializeDatabase, incomingConverter, outgoingConverter));
		}

		if (includeWeb)
		{
			const string serverUri = "https://speedy.local";
			const int timeout = 60000;

			var credential = new Credential("admin@speedy.local", "Password");
			var webClient = new WebClient(serverUri, timeout, credential);

			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (MEM, Cached)", GetSyncableMemoryProvider(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (SQL2)", GetSyncableSqlProvider2(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (SQL2, Cached)", GetSyncableSqlProvider2(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			yield return Process(Timer.StartNew(),
				new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initialize: initializeDatabase), webClient),
				new SyncClient("Client (Sqlite, Cached)", GetSyncableSqliteProvider(initialize: initializeDatabase, keyCache: new DatabaseKeyCache())) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
		}
	}

	private static (Timer timer, ISyncClient server, ISyncClient client) GetSyncClients(DatabaseType scenarioServer, bool cacheServer, DatabaseType scenarioClient,
		bool cacheClient, bool initializeDatabase, SyncClientIncomingConverter incomingConverter, SyncClientOutgoingConverter outgoingConverter)
	{
		var timer = new Timer();
		timer.Start();
		var server = GetSyncClient("Server", scenarioServer, initializeDatabase, cacheServer, false, incomingConverter, outgoingConverter);
		var client = GetSyncClient("Client", scenarioClient, initializeDatabase, cacheClient, scenarioServer == scenarioClient, incomingConverter, outgoingConverter);
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

		keyCache?.InitializeAndLoad(database);
	}

	#endregion

	#if !NET48
	public static ControllerContext GetControllerContext(AccountEntity account)
	{
		var ticket = AuthenticationService.CreateTicket(account, true, CookieAuthenticationDefaults.AuthenticationScheme);
		return new ControllerContext
		{
			HttpContext = new DefaultHttpContext { User = ticket.Principal }
		};
	}

	public static IAuthenticationService GetAuthenticationService()
	{
		var service = new Mock<IAuthenticationService>();
		service.Setup(x => x.LogIn(It.IsAny<WebCredential>())).Returns<WebCredential>(x => true);
		return service.Object;
	}
	#endif
}