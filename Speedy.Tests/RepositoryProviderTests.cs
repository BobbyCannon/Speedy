#region References

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class RepositoryProviderTests
	{
		#region Fields

		private static readonly DirectoryInfo _directory;

		#endregion

		#region Constructors

		static RepositoryProviderTests()
		{
			_directory = new DirectoryInfo(@"C:\SpeedyTest");
		}

		#endregion

		#region Methods

		[TestMethod]
		public void AvailableRepositoriesShouldReturnMultipleRepository()
		{
			var name1 = "Repository1";
			var name2 = "Repository2";
			_directory.SafeDelete();
			_directory.SafeCreate();
			File.WriteAllText(_directory.FullName + $"\\{name1}.speedy", string.Empty);
			File.WriteAllText(_directory.FullName + $"\\{name2}.speedy", string.Empty);
			var expected = new List<string> { name1, name2 };
			var provider = new RepositoryProvider(_directory);
			var actual = provider.AvailableRepositories();

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AvailableRepositoriesShouldReturnRepository()
		{
			var name = "Repository1";
			_directory.SafeDelete();
			_directory.SafeCreate();
			File.WriteAllText(_directory.FullName + $"\\{name}.speedy", string.Empty);
			var expected = new List<string> { name };
			var provider = new RepositoryProvider(_directory);
			var actual = provider.AvailableRepositories();

			TestHelper.AreEqual(expected, actual);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			_directory.SafeDelete();
		}

		[TestMethod]
		public void GetRepositoryShouldReturnRepository()
		{
			Cleanup();

			var name = Guid.NewGuid().ToString();
			var provider = new RepositoryProvider(_directory);
			var repository = provider.GetRepository(name);
			Assert.IsNotNull(repository);
			Assert.AreEqual(name, repository.Name);
		}

		#endregion
	}
}