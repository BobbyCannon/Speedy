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

		public static IEnumerable<IRepository> Repositories
		{
			get
			{
				var guid = Guid.NewGuid().ToString();
				return new IRepository[] { new MemoryRepository(guid), new Repository(Directory, guid) };
			}
		}

		public static IEnumerable<IRepositoryProvider> RepositoryProviders => new IRepositoryProvider[] { new MemoryRepositoryProvider(), new RepositoryProvider(Directory) };

		#endregion

		#region Methods

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