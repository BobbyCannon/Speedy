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
		#region Methods

		[TestMethod]
		public void ArchiveRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			provider.OpenRepository(name1).Dispose();
			Assert.IsTrue(File.Exists($"{TestHelper.Directory}\\{name1}.speedy"));
			Assert.IsFalse(File.Exists($"{TestHelper.Directory}\\{name1}.speedy.archive"));
			provider.ArchiveRepository(name1);
			Assert.IsFalse(File.Exists($"{TestHelper.Directory}\\{name1}.speedy"));
			Assert.IsTrue(File.Exists($"{TestHelper.Directory}\\{name1}.speedy.archive"));
		}

		[TestMethod]
		public void ArchiveRepositoryInvalidName()
		{
			Cleanup();
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			TestHelper.ExpectedException<FileNotFoundException>(() => provider.ArchiveRepository("Repository1"), "The file could not be found.");
		}

		[TestMethod]
		public void AvailableRepositoriesShouldExclude()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			var name2 = "Repository2";
			var repository1 = provider.OpenRepository(name1);
			var repository2 = provider.OpenRepository(name2);
			var expected = new List<string> { name1 };
			var actual = provider.AvailableRepositories(name2);
			repository1.Dispose();
			repository2.Dispose();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AvailableRepositoriesShouldReturnMultipleRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			var name2 = "Repository2";
			var repository1 = provider.OpenRepository(name1);
			var repository2 = provider.OpenRepository(name2);
			var expected = new List<string> { name1, name2 };
			var actual = provider.AvailableRepositories();
			repository1.Dispose();
			repository2.Dispose();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AvailableRepositoriesShouldReturnRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name = "Repository1";
			provider.OpenRepository(name).Dispose();
			var expected = new List<string> { name };
			var actual = provider.AvailableRepositories();
			TestHelper.AreEqual(expected, actual);
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			TestHelper.Cleanup();
		}

		[TestMethod]
		public void DeleteRepositoriesShouldDeleteRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name = "Repository1";
			provider.OpenRepository(name).Dispose();
			var expected = new List<string> { name };
			var actual = provider.AvailableRepositories();
			TestHelper.AreEqual(expected, actual);
			provider.DeleteRepository(name);
			expected.Clear();
			actual = provider.AvailableRepositories();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void GetRepositoryShouldReturnRepository()
		{
			Cleanup();
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory.FullName);
			var name = Guid.NewGuid().ToString();

			using (var repository = provider.OpenRepository(name))
			{
				Assert.IsNotNull(repository);
				Assert.AreEqual(name, repository.Name);
			}
		}

		[TestMethod]
		public void OpenAvailableRepositoriesShouldReturnFirstRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			var name2 = "Repository2";
			provider.OpenRepository(name1).Dispose();
			var repository = provider.OpenRepository(name2);

			using (repository)
			{
				using (var repository2 = provider.OpenAvailableRepository())
				{
					Assert.AreEqual(repository2.Name, "Repository1");
				}
			}
		}

		[TestMethod]
		public void OpenAvailableRepositoryShouldExclude()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			var name2 = "Repository2";
			var repository1 = provider.OpenRepository(name1);
			provider.OpenRepository(name2).Dispose();
			var actual = provider.OpenAvailableRepository(name1);
			repository1.Dispose();
			TestHelper.AreEqual(name2, actual.Name);
			actual.Dispose();
		}

		[TestMethod]
		public void RepositoryDeleteShouldDeleteRepository()
		{
			Cleanup();

			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name = "Repository1";
			provider.OpenRepository(name).Dispose();
			var expected = new List<string> { name };
			var actual = provider.AvailableRepositories();
			TestHelper.AreEqual(expected, actual);

			using (var repository = provider.OpenRepository(name))
			{
				repository.Delete();
			}

			expected.Clear();
			actual = provider.AvailableRepositories();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void UnarchiveRepository()
		{
			Cleanup();

			TestHelper.Directory.SafeCreate();
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			var name1 = "Repository1";
			File.CreateText($"{TestHelper.Directory}\\{name1}.speedy.archive").Dispose();
			Assert.IsFalse(File.Exists($"{TestHelper.Directory}\\{name1}.speedy"));
			Assert.IsTrue(File.Exists($"{TestHelper.Directory}\\{name1}.speedy.archive"));
			provider.UnarchiveRepository(name1);
			Assert.IsTrue(File.Exists($"{TestHelper.Directory}\\{name1}.speedy"));
			Assert.IsFalse(File.Exists($"{TestHelper.Directory}\\{name1}.speedy.archive"));
		}

		[TestMethod]
		public void UnarchiveRepositoryInvalidName()
		{
			Cleanup();
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory);
			TestHelper.ExpectedException<FileNotFoundException>(() => provider.UnarchiveRepository("Repository1"), "The file could not be found.");
		}

		#endregion
	}
}