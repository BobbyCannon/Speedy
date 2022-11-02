#region References

using System.IO;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for directory info
/// </summary>
public static class DirectoryInfoExtensions
{
	#region Methods

	/// <summary>
	/// Empties a directory of all the files and the directories.
	/// </summary>
	/// <param name="directory"> The directory to empty. </param>
	public static void Empty(this DirectoryInfo directory)
	{
		// See if the directory exists.
		if (!directory.Exists)
		{
			return;
		}

		directory.SafeDelete();
	}

	/// <summary>
	/// Safely create a directory.
	/// </summary>
	/// <param name="info"> The information on the directory to create. </param>
	public static void SafeCreate(this DirectoryInfo info)
	{
		UtilityExtensions.Retry(() =>
		{
			info.Refresh();

			if (!info.Exists)
			{
				info.Create();
			}
		}, 1000, 10);

		UtilityExtensions.Wait(() =>
		{
			info.Refresh();
			return info.Exists;
		}, 1000, 10);
	}

	/// <summary>
	/// Safely delete a directory.
	/// </summary>
	/// <param name="info"> The information of the directory to delete. </param>
	public static void SafeDelete(this DirectoryInfo info)
	{
		UtilityExtensions.Retry(() =>
		{
			info.Refresh();

			if (info.Exists)
			{
				info.Delete(true);
			}
		}, 1000, 10);

		UtilityExtensions.Wait(() =>
		{
			info.Refresh();
			return !info.Exists;
		}, 1000, 10);
	}

	#endregion
}