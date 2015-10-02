#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class RepositoryTests
	{
		#region Methods

		[ClassCleanup]
		public static void Cleanup()
		{
			TestHelper.Directory.SafeDelete();
		}

		[TestMethod]
		public void CountShouldReturnCorrectValue()
		{
			TestHelper.ForEachRepository(repository =>
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");
				repository.Save();

				Assert.AreEqual(3, repository.Count);
			});
		}

		[TestMethod]
		public void CountShouldReturnZeroWithoutSave()
		{
			TestHelper.ForEachRepository(repository =>
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");

				Assert.AreEqual(0, repository.Count);
			});
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringMultipleWrites()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringWrite()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void LastActionForKeyShouldWin()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void LastWriteForKeyShouldWin()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void MultithreadedWriteAndReadTest()
		{
			TestHelper.ForEachRepository(repository =>
			{
				var random = new Random();

				var readAction = new Action<IRepository>(repo =>
				{
					Thread.Sleep(random.Next(750, 2000));
					repository.Read().ToList();
				});

				var writeAction = new Action<IRepository, int, int>((repo, min, max) =>
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
					Task.Factory.StartNew(() => writeAction(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => readAction(repository))
				};

				Task.WaitAll(tasks);

				var actual = repository.Read().ToList();
				Assert.AreEqual(4 * 100, actual.Count);
			});
		}

		[TestMethod]
		public void MultithreadedWriteTest()
		{
			TestHelper.ForEachRepository(repository =>
			{
				var action = new Action<IRepository, int, int>((repo, min, max) =>
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
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index)),
					Task.Factory.StartNew(() => action(repository, index++, 100 * index))
				};

				Task.WaitAll(tasks);

				var actual = repository.Read().ToList();
				Assert.AreEqual(tasks.Length * 100, actual.Count);
			});
		}

		[TestMethod]
		public void ReadUsingCondition()
		{
			TestHelper.ForEachRepository(repository =>
			{
				repository.Write(repository.Name, "Root object");
				repository.Write("ChildItem1", "ChildItemValue1");
				repository.Write("ChildItem2", "ChildItemValue2");
				repository.Save();

				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("ChildItem1", "ChildItemValue1"),
					new KeyValuePair<string, string>("ChildItem2", "ChildItemValue2")
				};

				var actual = repository.Read(key => key != repository.Name).ToList();
				Assert.AreEqual(2, actual.Count);
				TestHelper.AreEqual(expected, actual);
			});
		}

		[TestMethod]
		public void RemoveShouldRemoveItem()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
			});
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnFalse()
		{
			TestHelper.ForEachRepository(repository =>
			{
				string value;
				var actual = repository.TryRead("Blah", out value);
				Assert.AreEqual(false, actual);
				Assert.AreEqual(string.Empty, value);
			});
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnTrue()
		{
			TestHelper.ForEachRepository(repository =>
			{
				repository.Write("Blah", "Value");
				repository.Save();

				string value;
				var actual = repository.TryRead("Blah", out value);
				Assert.AreEqual(true, actual);
				Assert.AreEqual("Value", value);
			});
		}

		[TestMethod]
		public void WriteItemValueWithPipeCharacter()
		{
			TestHelper.ForEachRepository(repository =>
			{
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
				TestHelper.AreEqual(expected, actual);
			});
		}

		#endregion
	}
}