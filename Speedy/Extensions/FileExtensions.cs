#region References

using System.IO;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// File system extensions (file / directories)
	/// </summary>
	public static class FileExtensions
	{
		#region Methods

		/// <summary>
		/// Safely create a file.
		/// </summary>
		/// <param name="file"> The information of the file to create. </param>
		public static void SafeCreate(this FileInfo file)
		{
			file.Refresh();
			if (file.Exists)
			{
				return;
			}

			UtilityExtensions.Retry(() =>
			{
				if (file.Exists)
				{
					return;
				}

				File.Create(file.FullName).Dispose();
			}, 1000, 10);

			UtilityExtensions.Wait(() =>
			{
				file.Refresh();
				return file.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely delete a file.
		/// </summary>
		/// <param name="info"> The information of the file to delete. </param>
		public static void SafeDelete(this FileInfo info)
		{
			UtilityExtensions.Retry(() =>
			{
				info.Refresh();

				if (info.Exists)
				{
					info.Delete();
				}
			}, 1000, 10);

			UtilityExtensions.Wait(() =>
			{
				info.Refresh();
				return !info.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely move a file.
		/// </summary>
		/// <param name="fileLocation"> The information of the file to move. </param>
		/// <param name="newLocation"> The location to move the file to. </param>
		public static void SafeMove(this FileInfo fileLocation, FileInfo newLocation)
		{
			fileLocation.Refresh();
			if (!fileLocation.Exists)
			{
				throw new FileNotFoundException("The file could not be found.", fileLocation.FullName);
			}

			UtilityExtensions.Retry(() => fileLocation.MoveTo(newLocation.FullName), 1000, 10);

			UtilityExtensions.Wait(() =>
			{
				fileLocation.Refresh();
				newLocation.Refresh();
				return !fileLocation.Exists && newLocation.Exists;
			}, 1000, 10);
		}

		internal static FileStream OpenAndCopyTo(this FileStream from, FileInfo to, int timeout)
		{
			lock (from)
			{
				var response = UtilityExtensions.Retry(() => File.Open(to.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None), timeout, 50);
				from.Position = 0;
				from.CopyTo(response);
				response.Flush(true);
				response.Position = 0;
				return response;
			}
		}

		/// <summary>
		/// Open the file with read/write permission with file read share.
		/// </summary>
		/// <param name="info"> The information for the file. </param>
		/// <returns> The stream for the file. </returns>
		internal static FileStream OpenFile(this FileInfo info)
		{
			return UtilityExtensions.Retry(() => File.Open(info.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read), 1000, 50);
		}

		#endregion
	}
}