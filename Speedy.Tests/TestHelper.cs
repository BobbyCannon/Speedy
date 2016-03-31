#region References

using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Speedy.Samples;
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

		/// <summary>
		/// Compares two objects to see if they are equal.
		/// </summary>
		/// <typeparam name="T"> The type of the object. </typeparam>
		/// <param name="expected"> The item that is expected. </param>
		/// <param name="actual"> The item that is to be tested. </param>
		public static void AreEqual<T>(T expected, T actual)
		{
			var compareObjects = new CompareLogic();
			compareObjects.Config.MaxDifferences = int.MaxValue;
			compareObjects.Config.CompareChildren = false;

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		public static void Cleanup()
		{
			Directory.SafeDelete();
		}

		public static void ExpectedException<T>(Action work, string errorMessage) where T : Exception
		{
			try
			{
				work();
			}
			catch (T ex)
			{
				var details = ex.ToDetailedString();
				if (!details.Contains(errorMessage))
				{
					Assert.Fail("Expected <" + details + "> to contain <" + errorMessage + ">.");
				}
				return;
			}

			Assert.Fail("The expected exception was not thrown.");
		}

		public static IEnumerable<ISampleDatabaseProvider> GetDataContextProviders()
		{
			var context1 = new EntityFrameworkSampleDatabase();
			context1.Database.ExecuteSqlCommand(Resources.ClearDatabase);

			var contextProvider1 = new Mock<ISampleDatabaseProvider>();
			contextProvider1.Setup(x => x.CreateContext()).Returns(() => new EntityFrameworkSampleDatabase());

			var context2 = new SampleDatabase();
			var contextProvider2 = new Mock<ISampleDatabaseProvider>();
			contextProvider2.Setup(x => x.CreateContext()).Returns(context2);

			var contextProvider3 = new Mock<ISampleDatabaseProvider>();
			contextProvider3.Setup(x => x.CreateContext()).Returns(() => new SampleDatabase(Directory.FullName));

			return new[] { contextProvider1.Object, contextProvider2.Object, contextProvider3.Object };
		}

		public static IEnumerable<ISampleDatabase> GetDataContexts()
		{
			var context1 = new EntityFrameworkSampleDatabase();
			context1.Database.ExecuteSqlCommand(Resources.ClearDatabase);

			return new ISampleDatabase[] { context1, new SampleDatabase(), new SampleDatabase(Directory.FullName) };
		}

		public static void Initialize()
		{
			Directory.SafeDelete();
			Directory.SafeCreate();
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