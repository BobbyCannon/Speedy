#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class RepositoryTests
	{
		#region Fields

		private static readonly string _directory;

		#endregion

		#region Constructors

		static RepositoryTests()
		{
			_directory = "C:\\SpeedyTest";
		}

		#endregion

		#region Methods

		[TestMethod]
		public void ClearShouldDeleteFile()
		{
			var name = Guid.NewGuid().ToString();
			var repository = new Repository(_directory, name);
			repository.Write("foo", "bar");
			repository.Save();

			Assert.IsTrue(File.Exists(_directory + "\\" + name + ".data"));

			repository.Clear();

			Assert.IsFalse(File.Exists(_directory + "\\" + name + ".data"));
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringWrite()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item1");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Item1", "Item1"),
				new KeyValuePair<string, string>("Item2", "Item2"),
				new KeyValuePair<string, string>("Item3", "Item3"),
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}
		
		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringMultipleWrites()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Save();

			repository.Write("Item1", "Item1");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Item1", "Item1"),
				new KeyValuePair<string, string>("Item2", "Item2"),
				new KeyValuePair<string, string>("Item3", "Item3"),
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		[TestMethod]
		public void LastActionForKeyShouldWin()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());

			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Remove("Item1");
			repository.Write("Item1", "Item10");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Item1", "Item10"),
				new KeyValuePair<string, string>("Item2", "Item2"),
				new KeyValuePair<string, string>("Item3", "Item3"),
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		[TestMethod]
		public void LastWriteForKeyShouldWin()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());

			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item10");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Item1", "Item10"),
				new KeyValuePair<string, string>("Item2", "Item2"),
				new KeyValuePair<string, string>("Item3", "Item3"),
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		[TestMethod]
		public void WriteItemValueWithPipeCharacter()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());

			repository.Write("Item1", "Item1|Item2");
			repository.Write("Item2", "Item2|Boo");
			repository.Write("Item3", "Item3|Foo|Bar|Again");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("Item1", "Item1|Item2"),
				new KeyValuePair<string, string>("Item2", "Item2|Boo"),
				new KeyValuePair<string, string>("Item3", "Item3|Foo|Bar|Again"),
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		#endregion

		#region Static Methods

		public static void AreEqual<T>(T expected, T actual)
		{
			var compareObjects = new CompareLogic();
			compareObjects.Config.MaxDifferences = int.MaxValue;

			var result = compareObjects.Compare(expected, actual);
			Assert.IsTrue(result.AreEqual, result.DifferencesString);
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			if (Directory.Exists(_directory))
			{
				Directory.Delete(_directory, true);
			}
		}

		#endregion
	}
}