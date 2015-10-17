#region References

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class RepositoryProviderTests
	{
		#region Methods

		[TestMethod]
		public void AvailableRepositoriesShouldReturnMultipleRepository()
		{
			Cleanup();

			var provider = new RepositoryProvider(TestHelper.Directory);
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

			var provider = new RepositoryProvider(TestHelper.Directory);
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

			var provider = new RepositoryProvider(TestHelper.Directory);
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
			var provider = new RepositoryProvider(TestHelper.Directory);
			Cleanup();

			var name = Guid.NewGuid().ToString();
			using (var repository = provider.OpenRepository(name))
			{
				Assert.IsNotNull(repository);
				Assert.AreEqual(name, repository.Name);
			}
		}

		[TestMethod]
		public void RepositoryDeleteShouldDeleteRepository()
		{
			Cleanup();

			var provider = new RepositoryProvider(TestHelper.Directory);
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

		#endregion
	}
}