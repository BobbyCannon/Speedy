﻿#region References

using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.EntityFramework;
using Speedy.IntegrationTests.Properties;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Samples.EntityFramework;
using Speedy.Sync;

#endregion

namespace Speedy.IntegrationTests
{
	public static class TestHelper
	{
		#region Fields

		public static readonly DirectoryInfo Directory;

		#endregion

		#region Constructors

		static TestHelper()
		{
			Directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Speedy");

			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersionByContext<ContosoDatabase, Samples.EntityFramework.Migrations.Configuration>());
		}

		#endregion

		#region Methods

		public static void AddAndSaveChanges<T>(this IContosoDatabase database, T item) where T : SyncEntity
		{
			var repository = database.GetSyncableRepository(item.GetType());
			repository.Add(item);
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
			Debug.WriteLine(item);
		}

		public static void Dump(this byte[] item)
		{
			foreach (var i in item)
			{
				Debug.Write($"{i:X2},");
			}

			Debug.WriteLine("");
		}

		public static void Dump<T>(this T item, Func<T, object> action)
		{
			Debug.WriteLine(action(item));
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
				if (!errorMessage.Any(x => details.Contains(x)))
				{
					Assert.Fail("Exception message did not contain expected error.");
				}
				return;
			}

			Assert.Fail("The expected exception was not thrown.");
		}

		public static void FormatDump(this string item)
		{
			Console.WriteLine(item.Replace("\r\n", "\\r\\n").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\"", "\\\""));
		}

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContextProviders()
		{
			return GetDataContexts();
		}

		public static IEnumerable<IDatabaseProvider<IContosoDatabase>> GetDataContexts(DatabaseOptions options = null)
		{
			yield return GetEntityFrameworkProvider();
			yield return GetMemoryProvider(null, options);
			yield return GetMemoryProvider(Directory.FullName, options);
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

		/// <summary>
		/// Safely delete a directory.
		/// </summary>
		/// <param name="directory"> The information of the directory to create. </param>
		public static void SafeCreate(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (directory.Exists)
			{
				return;
			}

			directory.Create();

			Wait(() =>
			{
				directory.Refresh();
				return directory.Exists;
			});
		}

		public static void TestServerAndClients(Action<ISyncClient, ISyncClient> action)
		{
			GetServerClientScenerios().ForEach(x =>
			{
				Console.WriteLine(x.Item1.Name + " -> " + x.Item2.Name);
				action(x.Item1, x.Item2);
			});
		}

		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.AppendLine(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			var entityException = ex as DbEntityValidationException;
			if (entityException != null)
			{
				foreach (var details in entityException.EntityValidationErrors)
				{
					foreach (var error in details.ValidationErrors)
					{
						builder.AppendLine(details.Entry.Entity.GetType().Name + ": " + error.ErrorMessage);
					}

					builder.AppendLine();
				}
			}

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		private static IDatabaseProvider<IContosoDatabase> GetEntityFrameworkProvider()
		{
			using (var database = new ContosoDatabase())
			{
				database.ClearDatabase();
				var connectionString = database.Database.Connection.ConnectionString;
				return new DatabaseProvider<IContosoDatabase>(x => new ContosoDatabase(connectionString, x));
			}
		}

		private static IDatabaseProvider<IContosoDatabase> GetMemoryProvider(string directory = null, DatabaseOptions options = null)
		{
			var database = new ContosoMemoryDatabase(directory, options);
			// todo: support new options?
			return new DatabaseProvider<IContosoDatabase>(x => database);
		}

		private static IEnumerable<Tuple<ISyncClient, ISyncClient>> GetServerClientScenerios()
		{
			yield return new Tuple<ISyncClient, ISyncClient>(new SyncClient("Server (MEM)", GetSyncableMemoryProvider()), new SyncClient("Client (EF)", GetSyncableEntityFrameworkProvider()));
			yield return new Tuple<ISyncClient, ISyncClient>(new SyncClient("Server (MEM)", GetSyncableMemoryProvider()), new WebSyncClient("Client (WEB)", GetSyncableEntityFrameworkProvider(), "http://speedy.local"));
			yield return new Tuple<ISyncClient, ISyncClient>(new SyncClient("Server (EF)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
			yield return new Tuple<ISyncClient, ISyncClient>(new WebSyncClient("Server (WEB)", GetSyncableEntityFrameworkProvider(), "http://speedy.local"), new SyncClient("Client (MEM)", GetSyncableMemoryProvider()));
			yield return new Tuple<ISyncClient, ISyncClient>(new SyncClient("Server (EF)", GetSyncableEntityFrameworkProvider()), new SyncClient("Client (EF2)", GetSyncableEntityFrameworkProvider2()));
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider()
		{
			using (var database = new ContosoDatabase())
			{
				database.ClearDatabase();
				var connectionString = database.Database.Connection.ConnectionString;
				return new SyncDatabaseProvider(x => new ContosoDatabase(connectionString, x));
			}
		}

		private static ISyncableDatabaseProvider GetSyncableEntityFrameworkProvider2()
		{
			using (var database = new ContosoDatabase("DefaultConnection2"))
			{
				database.ClearDatabase();
				return new SyncDatabaseProvider(x => new ContosoDatabase("DefaultConnection2", x));
			}
		}

		private static ISyncableDatabaseProvider GetSyncableMemoryProvider(string directory = null)
		{
			var database = new ContosoMemoryDatabase(directory);
			// todo: support new options?
			return new SyncDatabaseProvider(x => database);
		}

		/// <summary>
		/// Safely delete a directory.
		/// </summary>
		/// <param name="directory"> The information of the directory to delete. </param>
		private static void SafeDelete(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (!directory.Exists)
			{
				return;
			}

			directory.Delete(true);

			Wait(() =>
			{
				directory.Refresh();
				return !directory.Exists;
			});
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