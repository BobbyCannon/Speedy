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

			foreach (var provider in TestHelper.RepositoryProviders)
			{
				var name1 = "Repository1";
				var name2 = "Repository2";
				provider.OpenRepository(name1);
				provider.OpenRepository(name2);
				var expected = new List<string> { name1, name2 };
				var actual = provider.AvailableRepositories();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void AvailableRepositoriesShouldReturnRepository()
		{
			Cleanup();

			foreach (var provider in TestHelper.RepositoryProviders)
			{
				var name = "Repository1";
				provider.OpenRepository(name);
				var expected = new List<string> { name };
				var actual = provider.AvailableRepositories();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			TestHelper.Directory.SafeDelete();
		}

		[TestMethod]
		public void DeleteRepositoriesShouldDeleteRepository()
		{
			Cleanup();

			foreach (var provider in TestHelper.RepositoryProviders)
			{
				var name = "Repository1";
				provider.OpenRepository(name);
				var expected = new List<string> { name };
				var actual = provider.AvailableRepositories();
				TestHelper.AreEqual(expected, actual);
				provider.DeleteRepository(name);
				expected.Clear();
				actual = provider.AvailableRepositories();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void GetRepositoryShouldReturnRepository()
		{
			Cleanup();

			foreach (var provider in TestHelper.RepositoryProviders)
			{
				TestHelper.Directory.SafeDelete();
				var name = Guid.NewGuid().ToString();
				var repository = provider.OpenRepository(name);
				Assert.IsNotNull(repository);
				Assert.AreEqual(name, repository.Name);
			}
		}

		#endregion
	}
}