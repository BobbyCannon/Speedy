#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class RepositoryTests
	{
		#region Methods

		[TestMethod]
		public void Archive()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var archiveFile = new FileInfo($"{TestHelper.Directory}\\{name}.speedy.archive");
				var repositoryFile = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsFalse(archiveFile.Exists);
				Assert.IsTrue(repositoryFile.Exists);

				repository.Archive();
				archiveFile.Refresh();
				repositoryFile.Refresh();

				Assert.IsTrue(archiveFile.Exists);
				Assert.IsFalse(repositoryFile.Exists);

				var expected = string.Format("Foo1|\"Bar1\"{0}Foo2|\"Bar2\"{0}Foo3|\"Bar3\"{0}", Environment.NewLine);
				var actual = archiveFile.ReadAllText();
				Assert.AreEqual(expected, actual);
			}
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			TestHelper.Cleanup();
		}

		[TestMethod]
		public void ClearItemFromDisk()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var expected = string.Format("Foo1|\"Bar1\"{0}Foo2|\"Bar2\"{0}Foo3|\"Bar3\"{0}", Environment.NewLine);
				var actual = info.ReadAllText();
				Assert.AreEqual(expected, actual);
				Assert.IsTrue(info.Length > 0);

				repository.Clear();
				info.Refresh();
				Assert.AreEqual(0, info.Length);
			}
		}

		[TestMethod]
		public void CountCorrectWithCachedAdd()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			repository.Write("Foo3", "Bar3");
			repository.Save();

			Assert.AreEqual(3, repository.Count);

			repository.Dispose();
		}

		[TestMethod]
		public void CountCorrectWithCachedRemoves()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			var data = repository.Read().Take(1).ToList();
			var keys = new HashSet<string>(data.Select(x => x.Key));
			repository.Remove(keys);
			repository.Save();

			Assert.AreEqual(1, repository.Count);

			repository.Dispose();
		}

		[TestMethod]
		public void CountCorrectWithCachedUpdates()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			repository.Write("Foo1", "Bar");
			repository.Save();

			Assert.AreEqual(2, repository.Count);

			repository.Dispose();
		}

		[TestMethod]
		public void CountFromCorruptRepository()
		{
			var name = Guid.NewGuid().ToString();
			var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			info.Directory.SafeCreate();
			var badData = string.Format("Foo3{0}Bar2|\"Foo2\"{0}{0}|{0}Foo1|\"Bar1\"{0}", Environment.NewLine);
			File.WriteAllText(info.FullName, badData, Encoding.UTF8);

			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				Assert.AreEqual(2, repository.Count);
			}
		}

		[TestMethod]
		public void CountShouldReturnCorrectValue()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Remove("Item2");
				repository.Write("Item3", "Item3");
				repository.Save();

				Assert.AreEqual(2, repository.Count);
			}
		}

		[TestMethod]
		public void CountShouldReturnZero()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Save();
				Assert.AreEqual(1, repository.Count);

				repository.Remove("Item1");
				repository.Save();
				Assert.AreEqual(0, repository.Count);
			}
		}

		[TestMethod]
		public void CountShouldReturnZeroWithoutSave()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");

				Assert.AreEqual(0, repository.Count);
			}
		}

		[TestMethod]
		public void Delete()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var repositoryFile = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(repositoryFile.Exists);

				repository.Delete();
				repositoryFile.Refresh();
				Assert.IsFalse(repositoryFile.Exists);
			}
		}

		[TestMethod]
		public void DisposeShouldFlush()
		{
			var name = Guid.NewGuid().ToString();
			var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");

			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 10))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Item2", "Item2");
				repository.Write("Item3", "Item3");
				repository.Save();

				info.Refresh();
				Assert.AreEqual(0, info.Length);
			}

			info.Refresh();
			Assert.IsTrue(info.Length > 0);
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringMultipleWrites()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				repository.Write("Item1", "Item1");
				repository.Write("Bar2", "Foo2");
				repository.Write("Item3", "Item3");
				repository.Save();
				Assert.AreEqual(3, repository.Count);

				repository.Write("Item1", "Item1");
				repository.Save();
				Assert.AreEqual(3, repository.Count);

				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Bar2", "Foo2"),
					new KeyValuePair<string, string>("Item3", "Item3"),
					new KeyValuePair<string, string>("Item1", "Item1")
				};

				var actual = repository.Read().ToList();
				Assert.AreEqual(3, actual.Count);
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringWrite()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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

		//[TestMethod]
		//public void FindMissingKeys()
		//{
		//	var name = Guid.NewGuid().ToString();
		//	using (var repository = Repository.Create(TestHelper.Directory, name))
		//	{
		//		repository.Write("Foo1", "Bar1");
		//		repository.Write("Foo2", "Bar2");
		//		repository.Write("Foo3", "Bar3");
		//		repository.Save();

		//		var expected = new HashSet<string> { "Foo4" };
		//		var actual = repository.FindMissingKeys(new HashSet<string> { "Foo1", "Foo2", "Foo3", "Foo4" });
		//		TestHelper.AreEqual(expected, actual);
		//	}
		//}

		[TestMethod]
		public void FlushShouldWriteAllItems()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 20))
			{
				repository.Write("Foo1", "Bar1");
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(0, info.Length);

				repository.Flush();
				info.Refresh();
				Assert.IsTrue(info.Length > 0);
			}
		}

		[TestMethod]
		public void LastActionForKeyShouldWin()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
		public void LoadUsingDictionary()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1)))
			{
				repository.Load(new Dictionary<string, string> { { "Foo1", "Bar1" }, { "Foo2", "Bar2" } });
				Assert.AreEqual(2, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo1|\"Bar1\"" + Environment.NewLine + "Foo2|\"Bar2\"" + Environment.NewLine, actual);
			}
		}

		[TestMethod]
		public void MultithreadedWriteAndReadTest()
		{
			using (var context = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 10))
			{
				var repository = context;
				var random = new Random();

				var readAction = new Action<IKeyValueRepository<string>>(repo =>
				{
					Thread.Sleep(random.Next(10, 50));
					repository.Read().ToList();
				});

				var writeAction = new Action<IKeyValueRepository<string>, int, int>((repo, min, max) =>
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
			using (var context = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
			{
				var repository = context;
				var action = new Action<IKeyValueRepository<string>, int, int>((repo, min, max) =>
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
			using (var context = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
			{
				var repository = context;
				var action = new Action<IKeyValueRepository<string>, int, int>((repo, min, max) =>
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
		public void ReadCorrectWithCachedAdd()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			repository.Write("Foo3", "Bar3");
			repository.Save();

			var expected = new[]
			{
				new KeyValuePair<string, string>("Foo1", "Bar1"),
				new KeyValuePair<string, string>("Foo2", "Bar2"),
				new KeyValuePair<string, string>("Foo3", "Bar3")
			};

			var actual = repository.Read().ToArray();
			TestHelper.AreEqual(expected, actual);

			repository.Dispose();
		}

		[TestMethod]
		public void ReadCorrectWithCachedRemoves()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			var data = repository.Read().Take(1).ToList();
			var keys = new HashSet<string>(data.Select(x => x.Key));
			repository.Remove(keys);
			repository.Save();

			Assert.AreEqual(1, repository.Count);

			var actual = repository.Read().Take(1).First();
			Assert.AreEqual("Foo2", actual.Key);
			Assert.AreEqual("Bar2", actual.Value);

			repository.Dispose();
		}

		[TestMethod]
		public void ReadCorrectWithCachedUpdate()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			repository.Write("Foo1", "1Bar");
			repository.Save();

			var expected = new[]
			{
				new KeyValuePair<string, string>("Foo1", "1Bar"),
				new KeyValuePair<string, string>("Foo2", "Bar2")
			};

			var actual = repository.Read().ToArray();
			TestHelper.AreEqual(expected, actual);

			repository.Dispose();
		}

		[TestMethod]
		public void ReadFromCorruptRepository()
		{
			var name = Guid.NewGuid().ToString();
			var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			info.Directory.SafeCreate();
			var badData = string.Format("Foo3{0}Bar2|\"Foo2\"{0}{0}|{0}Foo1|\"Bar1\"{0}", Environment.NewLine);
			File.WriteAllText(info.FullName, badData, Encoding.UTF8);

			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Bar2", "Foo2"),
					new KeyValuePair<string, string>("Foo1", "Bar1")
				};

				var actual = repository.Read().ToList();
				Assert.AreEqual(2, actual.Count);
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void ReadInvalidKeyShouldThrowException()
		{
			var name = Guid.NewGuid().ToString();
			using (var context = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				var repository = context;
				TestHelper.ExpectedException<KeyNotFoundException>(() => repository.Read("Blah"), "Could not find the entry with the key.");
			}
		}

		[TestMethod]
		public void ReadItemFromCache()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Bar2", "Foo2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);

				// Only Foo1 should be in the file.
				var expected = "Foo1|\"Bar1\"" + Environment.NewLine;
				var actual = info.ReadAllText();
				Assert.AreEqual(expected, actual);
				actual = repository.Read("Bar2");
				Assert.AreEqual("Foo2", actual);
			}
		}

		[TestMethod]
		public void ReadItemsFromDisk()
		{
			var provider = new KeyValueRepositoryProvider(TestHelper.Directory, TimeSpan.FromDays(1), 10000);
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
			tempInfo.Directory.SafeCreate();
			var rawData = $"Foo1|\"Bar1\"{Environment.NewLine}Foo2|\"Bar2\"";
			File.WriteAllText(tempInfo.FullName, rawData, Encoding.UTF8);

			var repository = provider.OpenRepository(name);
			repository.Write("Foo3", "Bar3");
			repository.Save();

			Assert.AreEqual(3, repository.Count);

			repository.Dispose();
		}

		[TestMethod]
		public void ReadOnlyWrittenItemsFromDisk()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Foo2", "Bar2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);

				var expected = "Foo1|\"Bar1\"" + Environment.NewLine;
				var actual = info.ReadAllText();
				Assert.AreEqual(expected, actual);
				actual = repository.Read("Foo1");
				Assert.AreEqual("Bar1", actual);
			}
		}

		[TestMethod]
		public void ReadOrderWithCaching()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromMinutes(1), 10))
			{
				repository.Write("Foo4", "Bar4");
				repository.Write("Bar4", "Foo4");
				repository.Write("Yo", "Nope");
				repository.Remove("Yo");
				repository.Write("Foo1", "Bar1");
				repository.Write("Bar1", "Foo1");
				repository.Save();

				Assert.AreEqual(4, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.AreEqual(0, info.Length);

				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Foo4", "Bar4"),
					new KeyValuePair<string, string>("Bar4", "Foo4"),
					new KeyValuePair<string, string>("Foo1", "Bar1"),
					new KeyValuePair<string, string>("Bar1", "Foo1")
				};

				var actual = repository.Read().ToList();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void ReadOrderWithNoCaching()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo4", "Bar4");
				repository.Write("Bar4", "Foo4");
				repository.Write("Foo1", "Bar1");
				repository.Write("Bar1", "Foo1");
				repository.Save();

				Assert.AreEqual(4, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actualText = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo4|\"Bar4\"{0}Bar4|\"Foo4\"{0}Foo1|\"Bar1\"{0}Bar1|\"Foo1\"{0}", Environment.NewLine), actualText);

				var expected = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("Foo4", "Bar4"),
					new KeyValuePair<string, string>("Bar4", "Foo4"),
					new KeyValuePair<string, string>("Foo1", "Bar1"),
					new KeyValuePair<string, string>("Bar1", "Foo1")
				};

				var actual = repository.Read().ToList();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void ReadUsingCondition()
		{
			using (var context = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
		public void RepositoryWithComplexRelationship()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository<Person>.Create(TestHelper.Directory, name))
			{
				var address = new Address { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
				var expected = new Person { Address = address, Name = "Bob Smith" };
				var id = Guid.NewGuid().ToString();

				repository.Write(id, expected);
				repository.Save();
				repository.Flush();

				var actual = repository.Read(id);

				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void RestoreFromCorruptPartialSave()
		{
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy.temp");
			tempInfo.Directory.SafeCreate();
			var badData = string.Format("Foo3{0}Bar2|\"Foo2\"{0}{0}|{0}Foo1|\"Bar1\"{0}", Environment.NewLine);
			File.WriteAllText(tempInfo.FullName, badData, Encoding.UTF8);

			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				Assert.AreEqual(2, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var expected = string.Format("Bar2|\"Foo2\"{0}Foo1|\"Bar1\"{0}", Environment.NewLine);
				var actual = info.ReadAllText();
				TestHelper.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void RestoreFromPartialSave()
		{
			var name = Guid.NewGuid().ToString();
			var tempInfo = new FileInfo($"{TestHelper.Directory}\\{name}.speedy.temp");
			tempInfo.Directory.SafeCreate();
			var expected = string.Format("Foo3|\"Bar3\"{0}Bar2|\"Foo2\"{0}Foo1|\"Bar1\"{0}", Environment.NewLine);
			File.WriteAllText(tempInfo.FullName, expected);

			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				Assert.AreEqual(3, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var actual = info.ReadAllText();
				Assert.AreEqual(expected.Length, actual.Length);
				Assert.AreEqual(expected, actual);
			}
		}

		[TestMethod]
		public void SaveShouldNotWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 10))
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
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1), 2))
			{
				repository.Write("Foo3", "Bar3");
				repository.Write("Bar2", "Foo2");
				repository.Write("Foo1", "Bar1");
				repository.Save();

				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo3|\"Bar3\"" + Environment.NewLine, actual);

				repository.Flush();
				info.Refresh();
				Assert.IsTrue(info.Length > 0);
				actual = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo3|\"Bar3\"{0}Bar2|\"Foo2\"{0}Foo1|\"Bar1\"{0}", Environment.NewLine), actual);
			}
		}

		[TestMethod]
		public void SaveShouldOnlyWriteItemsOverTimeoutToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromSeconds(1), 10))
			{
				repository.Write("Foo1", "Bar1");
				Thread.Sleep(1500);
				repository.Write("Bar2", "Foo2");
				repository.Write("Foo3", "Bar3");
				repository.Save();

				// Only Foo1 should be in the file.
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo1|\"Bar1\"" + Environment.NewLine, actual);

				repository.Flush();
				info.Refresh();
				Assert.IsTrue(info.Length > 0);
				actual = info.ReadAllText();
				Assert.AreEqual(39, actual.Length);
				actual = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo1|\"Bar1\"{0}Bar2|\"Foo2\"{0}Foo3|\"Bar3\"{0}", Environment.NewLine), actual);
			}
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnFalse()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
			{
				string value;
				var actual = repository.TryRead("Blah", out value);
				Assert.AreEqual(false, actual);
				Assert.AreEqual(null, value);
			}
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnTrue()
		{
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, Guid.NewGuid().ToString()))
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
		public void WriteOrderWithCachingLimit()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromMinutes(1), 2))
			{
				repository.Write("Foo4", "Bar4");
				repository.Write("Bar4", "Foo4");
				repository.Write("Foo1", "Bar1");
				repository.Write("Bar1", "Foo1");
				repository.Save();

				Assert.AreEqual(4, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var actual = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo4|\"Bar4\"{0}Bar4|\"Foo4\"{0}", Environment.NewLine), actual);

				repository.Flush();
				info.Refresh();
				Assert.IsTrue(info.Length > 0);
				actual = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo4|\"Bar4\"{0}Bar4|\"Foo4\"{0}Foo1|\"Bar1\"{0}Bar1|\"Foo1\"{0}", Environment.NewLine), actual);
			}
		}

		[TestMethod]
		public void WriteOrderWithNoCaching()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name))
			{
				repository.Write("Foo4", "Bar4");
				repository.Write("Bar4", "Foo4");
				repository.Write("Foo1", "Bar1");
				repository.Write("Bar1", "Foo1");
				repository.Save();

				Assert.AreEqual(4, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actual = info.ReadAllText();
				Assert.AreEqual(string.Format("Foo4|\"Bar4\"{0}Bar4|\"Foo4\"{0}Foo1|\"Bar1\"{0}Bar1|\"Foo1\"{0}", Environment.NewLine), actual);
			}
		}

		[TestMethod]
		public void WriteUsingDictionary()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1)))
			{
				repository.Write(new Dictionary<string, string> { { "Foo1", "Bar1" }, { "Foo2", "Bar2" } });
				repository.Save();

				Assert.AreEqual(2, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				Assert.IsTrue(info.Length > 0);
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo1|\"Bar1\"" + Environment.NewLine + "Foo2|\"Bar2\"" + Environment.NewLine, actual);
			}
		}

		[TestMethod]
		public void ZeroLimitShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.FromDays(1)))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo|\"Bar\"" + Environment.NewLine, actual);
			}
		}

		[TestMethod]
		public void ZeroTimeoutShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, TimeSpan.Zero, 10))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo|\"Bar\"" + Environment.NewLine, actual);
			}
		}

		[TestMethod]
		public void ZeroTimeoutViaNullShouldSaveShouldWriteToFile()
		{
			var name = Guid.NewGuid().ToString();
			using (var repository = KeyValueRepository.Create(TestHelper.Directory, name, null, 10))
			{
				repository.Write("Foo", "Bar");
				repository.Save();

				Assert.AreEqual(1, repository.Count);
				var info = new FileInfo($"{TestHelper.Directory}\\{name}.speedy");
				var actual = info.ReadAllText();
				Assert.AreEqual("Foo|\"Bar\"" + Environment.NewLine, actual);
			}
		}

		#endregion
	}
}