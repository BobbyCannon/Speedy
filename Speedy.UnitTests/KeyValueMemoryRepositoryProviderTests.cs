#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Exceptions;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class KeyValueMemoryRepositoryProviderTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void ArchiveRepository()
		{
			Cleanup();

			var provider = new KeyValueMemoryRepositoryProvider();
			var name1 = "Repository1";
			provider.OpenRepository(name1).Dispose();
			Assert.AreEqual(1, provider.OpenedRepositories.Count);
			Assert.AreEqual(0, provider.ArchivedRepositories.Count);
			provider.ArchiveRepository(name1);
			Assert.AreEqual(0, provider.OpenedRepositories.Count);
			Assert.AreEqual(1, provider.ArchivedRepositories.Count);
		}

		[TestMethod]
		public void ArchiveRepositoryInvalidName()
		{
			Cleanup();
			var provider = new KeyValueMemoryRepositoryProvider();
			TestHelper.ExpectedException<SpeedyException>(() => provider.ArchiveRepository("Repository1"), SpeedyException.RepositoryNotFound);
		}

		[TestMethod]
		public void AvailableRepositoriesShouldExclude()
		{
			Cleanup();

			var provider = new KeyValueMemoryRepositoryProvider();
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

			var provider = new KeyValueMemoryRepositoryProvider();
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

			var provider = new KeyValueMemoryRepositoryProvider();
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

			var provider = new KeyValueMemoryRepositoryProvider();
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

			var provider = new KeyValueMemoryRepositoryProvider();
			var name = Guid.NewGuid().ToString();
			using var repository = provider.OpenRepository(name);

			Assert.IsNotNull(repository);
			Assert.AreEqual(name, repository.Name);
		}

		[TestMethod]
		public void OpenAvailableRepositoriesShouldReturnFirstRepository()
		{
			Cleanup();

			var provider = new KeyValueMemoryRepositoryProvider();
			var name1 = "Repository1";
			var name2 = "Repository2";
			provider.OpenRepository(name1).Dispose();
			var repository = provider.OpenRepository(name2);

			using (repository)
			{
				using var repository2 = provider.OpenAvailableRepository();
				Assert.AreEqual(repository2.Name, "Repository1");
			}
		}

		[TestMethod]
		public void OpenAvailableRepositoryShouldExclude()
		{
			Cleanup();

			var provider = new KeyValueMemoryRepositoryProvider();
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

			var provider = new KeyValueMemoryRepositoryProvider();
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
			var provider = new KeyValueMemoryRepositoryProvider();
			var name1 = "Repository1";
			provider.OpenRepository(name1).Dispose();
			Assert.AreEqual(1, provider.OpenedRepositories.Count);
			Assert.AreEqual(0, provider.ArchivedRepositories.Count);
			provider.ArchiveRepository(name1);
			Assert.AreEqual(0, provider.OpenedRepositories.Count);
			Assert.AreEqual(1, provider.ArchivedRepositories.Count);
			provider.UnarchiveRepository(name1);
			Assert.AreEqual(1, provider.OpenedRepositories.Count);
			Assert.AreEqual(0, provider.ArchivedRepositories.Count);
		}

		[TestMethod]
		public void UnarchiveRepositoryInvalidName()
		{
			Cleanup();
			var provider = new KeyValueMemoryRepositoryProvider();
			TestHelper.ExpectedException<SpeedyException>(() => provider.UnarchiveRepository("Repository1"), SpeedyException.RepositoryNotFound);
		}

		#endregion
	}
}