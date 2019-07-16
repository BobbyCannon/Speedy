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
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.EntityFramework;
using Speedy.Net;
using Speedy.Samples.Sql;
using Speedy.Samples.Sqlite;
using Speedy.Samples.Tests.Properties;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Tests
{
	public static class TestHelper
	{
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
			database.Database.ExecuteSqlCommand(Resources.ClearDatabase);
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

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContextProviders()
		{
			return GetDataContexts();
		}

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContexts(DatabaseOptions options = null)
		{
			yield return GetSqlProvider(options);
			yield return GetSqliteProvider(options);
			yield return GetMemoryProvider(options);
		}

		public static IDatabaseProvider<IContosoDatabase> GetMemoryProvider(DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options);
			return new DatabaseProvider<IContosoDatabase>(x => database);
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

		public static IDatabaseProvider<IContosoDatabase> GetSqliteProvider(DatabaseOptions options = null)
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection, options))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqliteDatabase(database.DbContextOptions, x), options);
			}
		}

		public static IDatabaseProvider<IContosoDatabase> GetSqlProvider(DatabaseOptions options = null)
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection, options))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new DatabaseProvider<IContosoDatabase>(x => new ContosoSqlDatabase(database.DbContextOptions, x), options);
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

		public static void TestServerAndClients(Action<ISyncClient, ISyncClient> action, bool includeWeb = true)
		{
			GetServerClientScenarios(includeWeb).ForEach(x =>
			{
				Console.WriteLine(x.Item1.Name + " -> " + x.Item2.Name);
				action(x.Item1, x.Item2);
			});
		}

		internal static ISyncableDatabaseProvider GetSyncableMemoryProvider(DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(options);
			return new SyncDatabaseProvider(x =>
			{
				database.Options.UpdateWith(x);
				return database;
			}, database.Options);
		}

		internal static ISyncableDatabaseProvider GetSyncableSqliteProvider()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableSqliteProvider2()
		{
			using (var database = ContosoSqliteDatabase.UseSqlite(DefaultSqliteConnection2))
			{
				database.Database.EnsureDeleted();
				database.Database.Migrate();
				return new SyncDatabaseProvider(x => new ContosoSqliteDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableSqlProvider()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		internal static ISyncableDatabaseProvider GetSyncableSqlProvider2()
		{
			using (var database = ContosoSqlDatabase.UseSql(DefaultSqlConnection2))
			{
				database.Database.Migrate();
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoSqlDatabase(database.DbContextOptions, x), database.Options);
			}
		}

		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.AppendLine(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		private static IEnumerable<(ISyncClient server, ISyncClient client)> GetServerClientScenarios(bool includeWeb)
		{
			(ISyncClient server, ISyncClient client) process(ISyncClient server, ISyncClient client)
			{
				server.Options.MaintainModifiedOn = true;
				return (server, client);
			}

			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider()), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider()), new SyncClient("Client (SQL)", GetSyncableSqlProvider()));
			yield return process(new SyncClient("Server (MEM)", GetSyncableMemoryProvider()), new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider()));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider()), new SyncClient("Client (SQL2)", GetSyncableSqlProvider2()));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider()), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
			yield return process(new SyncClient("Server (SQL)", GetSyncableSqlProvider()), new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider()));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider()), new SyncClient("Client (Sqlite2)", GetSyncableSqliteProvider2()));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider()), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
			yield return process(new SyncClient("Server (Sqlite)", GetSyncableSqliteProvider()), new SyncClient("Client (SQL)", GetSyncableSqlProvider()));

			if (includeWeb)
			{
				var credential = new NetworkCredential(string.Empty, "Password");
				yield return process(new WebSyncClient("Server (WEB Secure)", GetSyncableSqlProvider(), "https://speedy.local", "api/SecureSync", credential), new SyncClient("Client (SQL2)", GetSyncableSqlProvider2()));
				yield return process(new WebSyncClient("Server (WEB)", GetSyncableSqlProvider(), "https://speedy.local"), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
				yield return process(new WebSyncClient("Server (WEB Secure)", GetSyncableSqlProvider(), "https://speedy.local", "api/SecureSync", credential), new SyncClient("Client (Sqlite)", GetSyncableSqliteProvider()));
			}
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