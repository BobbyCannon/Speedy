#region References

using System;
using System.Collections.Generic;
using System.IO;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	public static class TestHelper
	{
		#region Constructors

		static TestHelper()
		{
			Directory = new DirectoryInfo(@"C:\SpeedyTest");
		}

		#endregion

		#region Properties

		public static DirectoryInfo Directory { get; set; }

		private static IEnumerable<IRepository> Repositories
		{
			get
			{
				var guid = Guid.NewGuid().ToString();
				return new[] { new MemoryRepository(guid), Repository.Create(Directory, guid) };
			}
		}

		public static IEnumerable<IRepositoryProvider> RepositoryProviders => new IRepositoryProvider[] { new MemoryRepositoryProvider(), new RepositoryProvider(Directory) };

		#endregion

		#region Methods

		/// <summary>
		/// Process an action against a new instance of each repository.
		/// </summary>
		/// <param name="action"> The action to perform against each repository. </param>
		public static void ForEachRepository(Action<IRepository> action)
		{
			var browsers = Repositories;
			foreach (var browser in browsers)
			{
				using (browser)
				{
					action(browser);
				}
			}
		}
		
		public static void AreEqual<T>(T expected, T actual)
		{
			var compareObjects = new CompareLogic();
			compareObjects.Config.MaxDifferences = int.MaxValue;

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		#endregion
	}
}