#region References

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
			Directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\SpeedyTest");
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

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		public static void Cleanup()
		{
			Directory.SafeDelete();
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
				var data = new byte[stream.Length];
				var read = stream.Read(data, 0, data.Length);
				return Encoding.UTF8.GetString(data, 0, read);
			}
		}

		/// <summary>
		/// Safely delete a file.
		/// </summary>
		/// <param name="directory"> The information of the file to delete. </param>
		public static void SafeDelete(this DirectoryInfo directory)
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

		/// <summary>
		/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
		/// time.
		/// </summary>
		/// <param name="action"> The action to call. </param>
		/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
		/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		internal static bool Wait(Func<bool> action, double timeout = 1000, int delay = 50)
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