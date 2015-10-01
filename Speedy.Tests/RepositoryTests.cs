#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
				new KeyValuePair<string, string>("Item3", "Item3")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
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
				new KeyValuePair<string, string>("Item3", "Item3")
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
				new KeyValuePair<string, string>("Item3", "Item3")
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
				new KeyValuePair<string, string>("Item3", "Item3")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		[TestMethod]
		public void MultithreadedWriteTest()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());

			var action = new Action<Repository, int, int>((repo, min, max) =>
			{
				for (var i = min; i < max; i++)
				{
					repository.Write("Key" + i, "Value" + i);
					repository.Save();
				}
			});

			var index = 0;
			var tasks = new[]
			{
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index)),
				Task.Run(() => action(repository, index++, 100 * index))
			};

			Task.WaitAll(tasks);

			var actual = repository.Read().ToList();
			Assert.AreEqual(tasks.Length * 100, actual.Count);
		}

		[TestMethod]
		public void MultithreadedWriteAndReadTest()
		{
			var repository = new Repository(_directory, Guid.NewGuid().ToString());
			var random = new Random();

			var readAction = new Action<Repository>((repo) =>
			{
				Thread.Sleep(random.Next(750, 2000));
				repository.Read().ToList();
			});

			var writeAction = new Action<Repository, int, int>((repo, min, max) =>
			{
				for (var i = min; i < max; i++)
				{
					repository.Write("Key" + i, "Value" + i);
					repository.Save();
				}
			});
			
			var index = 0;
			var tasks = new[]
			{
				Task.Run(() => writeAction(repository, index++, 100 * index)),
				Task.Run(() => readAction(repository)),
				Task.Run(() => writeAction(repository, index++, 100 * index)),
				Task.Run(() => readAction(repository)),
				Task.Run(() => writeAction(repository, index++, 100 * index)),
				Task.Run(() => readAction(repository)),
				Task.Run(() => writeAction(repository, index++, 100 * index)),
				Task.Run(() => readAction(repository))
			};

			Task.WaitAll(tasks);

			var actual = repository.Read().ToList();
			Assert.AreEqual(4 * 100, actual.Count);
		}

		[TestMethod]
		public void RemoveShouldRemoveItem()
		{
			var name = Guid.NewGuid().ToString();
			var repository = new Repository(_directory, name);

			repository.Write("Item1", "Value1");
			repository.Write("Item2", "Value2");
			repository.Write("Item3", "Value3");
			repository.Save();

			var actual = repository.Read().ToList();
			Assert.AreEqual(actual.Count, 3);

			repository.Remove(new HashSet<string>(new[] { "Item2" }));
			repository.Save();

			actual = repository.Read().ToList();
			Assert.AreEqual(actual.Count, 2);
			Assert.AreEqual("Item1", actual[0].Key);
			Assert.AreEqual("Value1", actual[0].Value);
			Assert.AreEqual("Item3", actual[1].Key);
			Assert.AreEqual("Value3", actual[1].Value);
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
				new KeyValuePair<string, string>("Item3", "Item3|Foo|Bar|Again")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			AreEqual(expected, actual);
		}

		#endregion
	}
}