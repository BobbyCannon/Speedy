#region References

using System.IO;
using Android.OS;
using Speedy.Samples.Xamarin.Services;

#endregion

namespace Speedy.Samples.Xamarin.Droid.Services;

#pragma warning disable CS0618

public class FileService : IFileService
{
	#region Methods

	public void WriteFile(string fileName, string data)
	{
		var directoryPath = Path.Combine(
			Environment.ExternalStorageDirectory.AbsolutePath,
			Environment.DirectoryDownloads);

		var filePath = Path.Combine(directoryPath, fileName);
		File.WriteAllText(filePath, data);
	}

	#endregion
}