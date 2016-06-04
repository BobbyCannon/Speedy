#region References

using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.EntityFramework;
using Speedy.Samples;
using Speedy.Samples.EntityFramework;
using Speedy.Samples.EntityFrameworkCore;
using Speedy.Samples.Sync;
using Speedy.Sync;
using Speedy.Tests.Properties;

#endregion

namespace Speedy.Tests
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
		/// <param name="includeChildren"> True to include child complex types. </param>
		public static void AreEqual<T>(T expected, T actual, bool includeChildren = true)
		{
			var compareObjects = new CompareLogic();
			compareObjects.Config.MaxDifferences = int.MaxValue;
			compareObjects.Config.CompareChildren = includeChildren;

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

		public static IEnumerable<IContosoDatabaseProvider> GetDataContextProviders()
		{
			return GetDataContexts();
		}

		public static IEnumerable<IContosoDatabaseProvider> GetDataContexts(DatabaseOptions options = null)
		{
			var database1 = new EntityFrameworkContosoDatabase("DefaultConnection", options);
			database1.Database.ExecuteSqlCommand(Resources.ClearDatabase);
			yield return new EntityFrameworkContosoDatabaseProvider("DefaultConnection", options);

			var database2 = new EntityFrameworkCoreContosoDatabase("DefaultConnection2", options);
			database2.Database.Migrate();
			database2.Database.ExecuteSqlCommand(Resources.ClearDatabase);
			yield return new EntityFrameworkCoreContosoDatabaseProvider("DefaultConnection2", options);

			yield return new ContosoDatabaseProvider(null, options);
			yield return new ContosoDatabaseProvider(Directory.FullName, options);
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

		public static void TestServerAndClients(Action<IContosoSyncClient, IContosoSyncClient> action)
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

		private static IContosoDatabaseProvider GetEntityFrameworkProvider()
		{
			using (var database = new EntityFrameworkContosoDatabase())
			{
				database.ClearDatabase();
				return new EntityFrameworkContosoDatabaseProvider(database.Database.Connection.ConnectionString);
			}
		}

		private static IEnumerable<Tuple<IContosoSyncClient, IContosoSyncClient>> GetServerClientScenerios()
		{
			yield return new Tuple<IContosoSyncClient, IContosoSyncClient>(new ContosoSyncClient("Server (MEM)", new ContosoDatabaseProvider()), new ContosoSyncClient("Client (EF)", GetEntityFrameworkProvider()));
			yield return new Tuple<IContosoSyncClient, IContosoSyncClient>(new ContosoSyncClient("Server (MEM)", new ContosoDatabaseProvider()), new ContosoWebSyncClient("Client (WEB)", GetEntityFrameworkProvider()));
			yield return new Tuple<IContosoSyncClient, IContosoSyncClient>(new ContosoSyncClient("Server (EF)", GetEntityFrameworkProvider()), new ContosoSyncClient("Client (MEM)", new ContosoDatabaseProvider()));
			yield return new Tuple<IContosoSyncClient, IContosoSyncClient>(new ContosoWebSyncClient("Server (WEB)", GetEntityFrameworkProvider()), new ContosoSyncClient("Client (MEM)", new ContosoDatabaseProvider()));
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