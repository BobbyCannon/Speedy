#region References

using System;
using System.Collections.Generic;
using System.IO;
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
			TestHelper.Cleanup();
		}

		[TestMethod]
		public void ClearItemFromDisk()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(33, info.Length);

				repository.Clear();
				info.Refresh();
				Assert.AreEqual(0, info.Length);
			}
		}

		[TestMethod]
		public void CountShouldReturnCorrectValue()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");
				repository.Save();

				Assert.AreEqual(3, repository.Count);
			}
		}

		[TestMethod]
		public void CountShouldReturnZeroWithoutSave()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");

				Assert.AreEqual(0, repository.Count);
			}
		}

		[TestMethod]
		public void DisposeShouldFlush()
		{
			var name = Guid.NewGuid().ToString();
			var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");

			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 10))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");
				repository.Save();

				info.Refresh();
				Assert.AreEqual(0, info.Length);
			}

			info.Refresh();
			Assert.AreEqual(39, info.Length);
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringMultipleWrites()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");
				repository.Save();
				Assert.AreEqual(3, repository.Count);

				repository.Write("Item1", "Item1");
				repository.Save();
				Assert.AreEqual(3, repository.Count);

				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Item1", "Item1"),
					new KeyValuePair<string, string>("Item2", "Item2"),
					new KeyValuePair<string, string>("Item3", "Item3")
				};

				var actual = repository.Read().ToList();
				Assert.AreEqual(3, actual.Count);
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringWrite()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			}
		}

		[TestMethod]
		public void FlushShouldWriteAllItems()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 20))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(0, info.Length);

				repository.Flush();
				info.Refresh();
				Assert.AreEqual(33, info.Length);
			}
		}

		[TestMethod]
		public void LastActionForKeyShouldWin()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			}
		}

		[TestMethod]
		public void LastWriteForKeyShouldWin()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			}
		}

		[TestMethod]
		public void MultithreadedWriteAndReadTest()
		{
			using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 10))
			{
				var repository = context;
				var random = new Random();

				var readAction = new Action<IRepository>(repo =>
				{
					Thread.Sleep(random.Next(10, 50));
					repository.Read().ToList();
				});

				var writeAction = new Action<IRepository, int, int>((repo, min, max) =>
				{
					for (var i = min; i < max; i++)
					{
						repository.Write("Key" + i, "Value" + i);
						repository.Save();
					}

					repository.Flush();
				});

				var size = 20;

				var tasks = new[]
				{
					Task.Factory.StartNew(() => writeAction(repository, 0, size)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, 1 * size, 1 * size + size)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, 2 * size, 2 * size + size)),
					Task.Factory.StartNew(() => readAction(repository)),
					Task.Factory.StartNew(() => writeAction(repository, 3 * size, 3 * size + size)),
					Task.Factory.StartNew(() => readAction(repository))
				};

				Task.WaitAll(tasks);

				var actual = repository.Read().ToList();
				Assert.AreEqual(tasks.Length / 2 * size, actual.Count);
			}
		}

		[TestMethod]
		public void MultithreadedWriteTest()
		{
			using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
			{
				var repository = context;
				var action = new Action<IRepository, int, int>((repo, min, max) =>
				{
					for (var i = min; i < max; i++)
					{
						repository.Write("Key" + i, "Value" + i);
						repository.Save();
					}

					repository.Flush();
				});

				var size = 10;

				var tasks = new[]
				{
					Task.Factory.StartNew(() => action(repository, 0, size)),
					Task.Factory.StartNew(() => action(repository, 1 * size, 1 * size + size)),
					Task.Factory.StartNew(() => action(repository, 2 * size, 2 * size + size)),
					Task.Factory.StartNew(() => action(repository, 3 * size, 3 * size + size)),
					Task.Factory.StartNew(() => action(repository, 4 * size, 4 * size + size)),
					Task.Factory.StartNew(() => action(repository, 5 * size, 5 * size + size)),
					Task.Factory.StartNew(() => action(repository, 6 * size, 6 * size + size)),
					Task.Factory.StartNew(() => action(repository, 7 * size, 7 * size + size))
				};

				Task.WaitAll(tasks);
				repository.Flush();

				var actual = repository.Read().ToList();
				Assert.AreEqual(tasks.Length * size, actual.Count);
			}
		}

		/// <summary>
		/// Overlapping item writes should be acceptable, not efficient but acceptable.
		/// </summary>
		[TestMethod]
		public void MultithreadedWriteTestOverlappingEvents()
		{
			using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
			{
				var repository = context;
				var action = new Action<IRepository, int, int>((repo, min, max) =>
				{
					for (var i = min; i < max; i++)
					{
						repository.Write("Key" + i, "Value" + i);
						repository.Save();
					}

					repository.Flush();
				});

				var size = 10;

				var tasks = new[]
				{
					Task.Factory.StartNew(() => action(repository, 0, size)),
					Task.Factory.StartNew(() => action(repository, 1, 1 + size)),
					Task.Factory.StartNew(() => action(repository, 2, 2 + size)),
					Task.Factory.StartNew(() => action(repository, 3, 3 + size)),
					Task.Factory.StartNew(() => action(repository, 4, 4 + size)),
					Task.Factory.StartNew(() => action(repository, 5, 5 + size)),
					Task.Factory.StartNew(() => action(repository, 6, 6 + size)),
					Task.Factory.StartNew(() => action(repository, 7, 7 + size))
				};

				Task.WaitAll(tasks);
				repository.Flush();

				var actual = repository.Read().ToList();
				Assert.AreEqual(tasks.Length + size - 1, actual.Count);
			}
		}

		[TestMethod]
		public void ReadItemFromCache()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(11, info.Length);

				// Only Foo1 should be in the file.
				var expected = "Foo1|Bar1" + Environment.NewLine;
				var actual = info.ReadAllText();
				Assert.AreEqual(expected, actual);

				actual = repository.Read("Foo2");
				Assert.AreEqual("Bar2", actual);
			}
		}

		[TestMethod]
		public void ReadItemFromDisk()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(11, info.Length);
				var expected = "Foo1|Bar1" + Environment.NewLine;
				var actual = info.ReadAllText();
				Assert.AreEqual(expected, actual);

				actual = repository.Read("Foo1");
				Assert.AreEqual("Bar1", actual);
			}
		}

		[TestMethod]
		public void ReadUsingCondition()
		{
			using (var context = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				var repository = context;

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
			}
		}

		[TestMethod]
		public void RemoveShouldRemoveItem()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			}
		}

		[TestMethod]
		public void SaveShouldNotWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 10))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(0, info.Length);
			}
		}

		[TestMethod]
		public void SaveShouldOnlyWriteItemsOverLimitToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 2))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(11, info.Length);

				repository.Flush();
				info.Refresh();
				Assert.AreEqual(33, info.Length);
			}
		}

		[TestMethod]
		public void SaveShouldOnlyWriteItemsOverTimeoutToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(11, info.Length);
				var actual = info.ReadAllText();
				Assert.AreEqual(11, actual.Length);

				repository.Flush();
				info.Refresh();
				Assert.AreEqual(33, info.Length);
				actual = info.ReadAllText();
				Assert.AreEqual(33, actual.Length);
			}
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnFalse()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				string value;
				var actual = repository.TryRead("Blah", out value);
				Assert.AreEqual(false, actual);
				Assert.AreEqual(string.Empty, value);
			}
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnTrue()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Blah", "Value");
				repository.Save();

				string value;
				var actual = repository.TryRead("Blah", out value);
				Assert.AreEqual(true, actual);
				Assert.AreEqual("Value", value);
			}
		}

		[TestMethod]
		public void WriteItemValueWithPipeCharacter()
		{
			using (var repository = Repository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			}
		}

		[TestMethod]
		public void WriteUsingDictionary()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1)))
			{
				repository.Write(new Dictionary<string, string> { { "Foo1", "Bar1" }, { "Foo2", "Bar2" } });
				repository.Save();

				Assert.AreEqual(2, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(22, info.Length);
			}
		}

		[TestMethod]
		public void ZeroLimitShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1)))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(9, info.Length);
			}
		}

		[TestMethod]
		public void ZeroTimeoutShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, TimeSpan.Zero, 10))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(9, info.Length);
			}
		}

		[TestMethod]
		public void ZeroTimeoutViaNullShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = Repository.Create(TestHelper.Directory, name, null, 10))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(9, info.Length);
			}
		}

		#endregion
	}
}