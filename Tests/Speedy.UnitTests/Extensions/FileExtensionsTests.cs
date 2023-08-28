#region References

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class FileExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void MoveAndReplaceWithExistingFile()
	{
		var sourceDirectory = new DirectoryInfo(Path.Combine(AssemblyDirectory.FullName, "SourceFiles"));
		sourceDirectory.SafeCreate();
		var destinationDirectory = new DirectoryInfo(Path.Combine(AssemblyDirectory.FullName, "DestinationFiles"));
		destinationDirectory.SafeCreate();

		var sourceFileInfo = new FileInfo(Path.Combine(sourceDirectory.FullName, "Test.txt"));
		File.WriteAllText(sourceFileInfo.FullName, "aoeu");

		var destinationFileInfo = new FileInfo(Path.Combine(destinationDirectory.FullName, "Test.txt"));
		File.WriteAllText(sourceFileInfo.FullName, "snth");

		sourceFileInfo.SafeMove(destinationFileInfo, true, 20, 10);
		IsTrue(destinationFileInfo.Exists);
	}

	[TestMethod]
	public void MoveWithoutReplacingExistingFile()
	{
		var sourceDirectory = new DirectoryInfo(Path.Combine(AssemblyDirectory.FullName, "SourceFiles"));
		sourceDirectory.SafeCreate();
		var destinationDirectory = new DirectoryInfo(Path.Combine(AssemblyDirectory.FullName, "DestinationFiles"));
		destinationDirectory.SafeCreate();

		var sourceFileInfo = new FileInfo(Path.Combine(sourceDirectory.FullName, "Test.txt"));
		File.WriteAllText(sourceFileInfo.FullName, "aoeu");

		var destinationFileInfo = new FileInfo(Path.Combine(destinationDirectory.FullName, "Test.txt"));
		File.WriteAllText(sourceFileInfo.FullName, "snth");

		ExpectedException<IOException>(
			() => sourceFileInfo.SafeMove(destinationFileInfo, false, 20, 10),
			"Cannot create a file when that file already exists."
		);
	}

	#endregion
}