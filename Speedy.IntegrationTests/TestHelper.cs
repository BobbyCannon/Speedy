#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Client.Data;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.EntityFramework;
using Speedy.Extensions;
using Speedy.IntegrationTests.Properties;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Data.Sql;
using Speedy.Website.Data.Sqlite;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;
using Speedy.Website.Services;

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
			database.Database.ExecuteSqlRaw(Resources.ClearDatabase);
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
			return new SyncDatabaseProvider<ContosoClientMemoryDatabase>(x =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, ContosoClientMemoryDatabase.GetDefaultOptions());
		}

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContexts(DatabaseOptions options = null, bool initialized = true)
		{
			yield return GetSqlProvider(options, initialized);
			yield return GetSqliteProvider(options, initialized);
			yield return GetMemoryProvider(options, initialized);
		}

		public static IDispatcher GetDispatcher()
		{
			var dispatcher = new Mock<IDispatcher>();
			dispatcher.Setup(x => x.HasThreadAccess).Returns(true);
			return dispatcher.Object;
		}

		public static IPrincipal GetIdentity(int id, string name)
		{
			var response = new Mock<IPrincipal>();
			response.SetupGet(x => x.Identity).Returns(() => new GenericIdentity($"{id};{name}"));
			return response.Object;
		}

		public static IDatabaseProvider<IContosoDatabase> GetMemoryProvider(DatabaseOptions options = null, bool initialized = true)
		{
			var database = new ContosoMemoryDatabase(options);

			if (initialized)
			{
				InitializeDatabase(database);
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

		public static IDatabaseProvider<IContosoDatabase> GetSqliteProvider(DatabaseOptions options = null, bool initialized = true)
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, options))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();

				if (initialized)
				{
					InitializeDatabase(database);
				}

				return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x), options);
			}
		}

		public static IDatabaseProvider<IContosoDatabase> GetSqlProvider(DatabaseOptions options = null, bool initialized = true)
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, options))
			{
				database.Database.Migrate();
				database.ClearDatabase();

				if (initialized)
				{
					InitializeDatabase(database);
				}

				return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x), options);
			}
		}

		public static ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseOptions options = null, bool initialize = true)
		{
			var database = new ContosoMemoryDatabase(options);

			if (initialize)
			{
				InitializeDatabase(database);
			}

			return new SyncDatabaseProvider(x =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, database.Options);
		}

		public static ISyncableDatabaseProvider GetSyncableSqliteProvider(bool initialize = true)
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				if (initialize)
				{
					InitializeDatabase(database);
				}
				return new SyncDatabaseProvider<ContosoSqliteDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		public static ISyncableDatabaseProvider GetSyncableSqliteProvider2(bool initialize = true)
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				if (initialize)
				{
					InitializeDatabase(database);
				}
				return new SyncDatabaseProvider<ContosoSqliteDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		public static ISyncableDatabaseProvider GetSyncableSqlProvider(bool initialize = true)
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				if (initialize)
				{
					InitializeDatabase(database);
				}
				return new SyncDatabaseProvider<ContosoSqlDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		public static ISyncableDatabaseProvider GetSyncableSqlProvider2(bool initialize = true)
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection2))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				if (initialize)
				{
					InitializeDatabase(database);
				}
				return new SyncDatabaseProvider<ContosoSqlDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x), database.Options);
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
				Console.WriteLine(x.server.Name + " -> " + x.client.Name);
				action(x.server, x.client);
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

		private static IEnumerable<(ISyncClient server, ISyncClient client)> GetServerClientScenarios(bool includeWeb, bool initializeDatabase)
		{
			(ISyncClient server, ISyncClient client) process(ISyncClient server, ISyncClient client)
			{
				server.Options.MaintainModifiedOn = true;
				return (server, client);
			}

			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)), new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)));
			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)), new SyncClient("Client (SQL)", GetSyncableSqlProvider(initializeDatabase)));
			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)), new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider(initializeDatabase)), new SyncClient("Client (SQL2)", GetSyncableSqlProvider2(initializeDatabase)));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider(initializeDatabase)), new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider(initializeDatabase)), new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)), new SyncClient("Client (Sqlite2)", GetSyncableSqliteProvider2(initializeDatabase)));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)), new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)), new SyncClient("Client (SQL)", GetSyncableSqlProvider(initializeDatabase)));

			if (includeWeb)
			{
				var outgoingConverter = new SyncClientOutgoingConverter(
					new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
					new SyncObjectOutgoingConverter<AccountEntity, int, Account, int>()
				);

				var incomingConverter = new SyncClientIncomingConverter(
					new SyncObjectIncomingConverter<Address, long, AddressEntity, long>(),
					new SyncObjectIncomingConverter<Account, int, AccountEntity, int>()
				);

				var credential = new NetworkCredential("admin@speedy.local", "Password");
				yield return process(
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initializeDatabase), "https://speedy.local", credential: credential, timeout: 60000),
					new SyncClient("Client (MEM)", GetSyncableMemoryProvider(initialize: initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return process(
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initializeDatabase), "https://speedy.local", credential: credential, timeout: 60000),
					new SyncClient("Client (SQL2)", GetSyncableSqlProvider2(initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
				yield return process(
					new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(initializeDatabase), "https://speedy.local", credential: credential, timeout: 60000),
					new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider(initializeDatabase)) { IncomingConverter = incomingConverter, OutgoingConverter = outgoingConverter });
			}
		}

		private static void InitializeDatabase(IContosoDatabase database)
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
				Roles = BaseService.CombineRoles(AccountRole.Administrator),
				SyncId = Guid.Parse("56CF7B5C-4C5A-462C-939D-A1F387A7483C")
			});

			database.SaveChanges();
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