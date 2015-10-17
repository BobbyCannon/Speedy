#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

#endregion

namespace Speedy
{
	/// <summary>
	/// Extensions for all the things.
	/// </summary>
	internal static class Extensions
	{
		#region Methods

		/// <summary>
		/// Add or update a dictionary entry.
		/// </summary>
		/// <typeparam name="T1"> The type of the key. </typeparam>
		/// <typeparam name="T2"> The type of the value. </typeparam>
		/// <param name="dictionary"> The dictionary to update. </param>
		/// <param name="key"> The value of the key. </param>
		/// <param name="value"> The value of the value. </param>
		internal static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
				return;
			}

			dictionary.Add(key, value);
		}

		/// <summary>
		/// Open the file with read/write permission with file read share.
		/// </summary>
		/// <param name="info"> The information for the file. </param>
		/// <returns> The stream for the file. </returns>
		internal static FileStream OpenFile(this FileInfo info)
		{
			return Retry(() => File.Open(info.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read), TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50));
		}

		/// <summary>
		/// Safely create a file.
		/// </summary>
		/// <param name="file"> The information of the file to create. </param>
		internal static void SafeCreate(this FileInfo file)
		{
			file.Refresh();
			if (file.Exists)
			{
				return;
			}

			Retry(() => File.Create(file.FullName).Dispose(), TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50));

			Wait(() =>
			{
				file.Refresh();
				return !file.Exists;
			});
		}

		internal static void SafeCreate(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (directory.Exists)
			{
				return;
			}

			Retry(directory.Create, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50));

			Wait(() =>
			{
				directory.Refresh();
				return directory.Exists;
			});
		}

		/// <summary>
		/// Safely delete a file.
		/// </summary>
		/// <param name="file"> The information of the file to delete. </param>
		internal static void SafeDelete(this FileInfo file)
		{
			file.Refresh();
			if (!file.Exists)
			{
				return;
			}

			Retry(file.Delete, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50));

			Wait(() =>
			{
				file.Refresh();
				return !file.Exists;
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

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <typeparam name="T"> The type for this retry. </typeparam>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		private static T Retry<T>(Func<T> action, TimeSpan timeout, TimeSpan delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				return action();
			}
			catch (Exception)
			{
				Thread.Sleep((int) delay.TotalMilliseconds);

				var remaining = timeout - watch.Elapsed;
				if (remaining.Ticks >= 0)
				{
					return Retry(action, remaining, delay);
				}

				throw;
			}
		}

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		private static void Retry(Action action, TimeSpan timeout, TimeSpan delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				action();
			}
			catch (Exception)
			{
				Thread.Sleep((int) delay.TotalMilliseconds);

				var remaining = timeout - watch.Elapsed;
				if (remaining.Ticks >= 0)
				{
					Retry(action, remaining, delay);
				}

				throw;
			}
		}

		#endregion
	}
}