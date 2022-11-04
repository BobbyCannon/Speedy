#region References

using System;
using System.IO;
using Speedy.Samples.Xamarin.iOS.Services;
using Speedy.Samples.Xamarin.Services;
using Xamarin.Forms;

#endregion

[assembly: Dependency(typeof(FileService))]

namespace Speedy.Samples.Xamarin.iOS.Services;

public class FileService : IFileService
{
	#region Methods

	public void WriteFile(string fileName, string data)
	{
		var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var filePath = Path.Combine(documents, fileName);
		File.WriteAllText(filePath, data);
	}

	#endregion
}