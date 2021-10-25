#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Client;
using Speedy.Exceptions;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class KeyValueMemoryRepositoryTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void Archive()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name);
			var eventCalled = false;

			void onRepositoryOnArchived(object sender, EventArgs args)
			{
				eventCalled = true;
			}

			repository.Archived += onRepositoryOnArchived;

			repository.Write("Foo1", "Bar1");
			repository.Write("Foo2", "Bar2");
			repository.Write("Foo3", "Bar3");
			repository.Save();
			Assert.AreEqual(3, repository.Count);
			Assert.IsFalse(eventCalled, "Archived event should not have been called yet.");
			repository.Archive();
			repository.Archived -= onRepositoryOnArchived;
			Assert.AreEqual(0, repository.Count);
			Assert.IsTrue(eventCalled, "Archived event was not called.");
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			TestHelper.Cleanup();
		}

		[TestMethod]
		public void Clear()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item1");
			repository.Save();

			var actual = repository.ToList();
			Assert.AreEqual(3, actual.Count);

			repository.Clear();
			Assert.AreEqual(0, repository.Count);
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
		public void CountShouldReturnCorrectValue()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Remove("Item2");
			repository.Write("Item3", "Item3");
			repository.Save();

			Assert.AreEqual(2, repository.Count);
		}

		[TestMethod]
		public void CountShouldReturnZero()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Save();
			Assert.AreEqual(1, repository.Count);

			repository.Remove("Item1");
			repository.Save();
			Assert.AreEqual(0, repository.Count);
		}

		[TestMethod]
		public void CountShouldReturnZeroWithoutSave()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");

			Assert.AreEqual(0, repository.Count);
		}

		[TestMethod]
		public void Delete()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name);
			var eventCalled = false;

			void onRepositoryOnDeleted(object sender, EventArgs args)
			{
				eventCalled = true;
			}

			repository.Deleted += onRepositoryOnDeleted;

			repository.Write("Foo1", "Bar1");
			repository.Write("Foo2", "Bar2");
			repository.Write("Foo3", "Bar3");
			repository.Save();
			Assert.AreEqual(3, repository.Count);
			Assert.IsFalse(eventCalled, "Deleted event should not have been called yet.");
			repository.Delete();
			repository.Deleted -= onRepositoryOnDeleted;
			Assert.AreEqual(0, repository.Count);
			Assert.IsTrue(eventCalled, "Deleted event was not called.");
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringMultipleWrites()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
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
				new("Bar2", "Foo2"),
				new("Item3", "Item3"),
				new("Item1", "Item1")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void DuplicateKeysShouldNotHappenDuringWrite()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item1");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Item2", "Item2"),
				new("Item3", "Item3"),
				new("Item1", "Item1")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void Enumeration()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item1");
			repository.Save();

			var expected = new List<string> { "Item2", "Item3", "Item1" };
			var actual = repository.ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LastActionForKeyShouldWin()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Remove("Item1");
			repository.Write("Item1", "Item10");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Item2", "Item2"),
				new("Item3", "Item3"),
				new("Item1", "Item10")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LastWriteForKeyShouldWin()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1");
			repository.Write("Item2", "Item2");
			repository.Write("Item3", "Item3");
			repository.Write("Item1", "Item10");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Item2", "Item2"),
				new("Item3", "Item3"),
				new("Item1", "Item10")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void LoadUsingDictionary()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name, TimeSpan.FromDays(1));
			repository.Load(new Dictionary<string, string> { { "Foo1", "Bar1" }, { "Foo2", "Bar2" } });
			Assert.AreEqual(2, repository.Count);
		}

		[TestMethod]
		public void MultiThreadedWriteAndReadTest()
		{
			using var context = new KeyValueMemoryRepository(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 10);
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
				Task.Factory.StartNew(() => writeAction(repository, 1 * size, (1 * size) + size)),
				Task.Factory.StartNew(() => readAction(repository)),
				Task.Factory.StartNew(() => writeAction(repository, 2 * size, (2 * size) + size)),
				Task.Factory.StartNew(() => readAction(repository)),
				Task.Factory.StartNew(() => writeAction(repository, 3 * size, (3 * size) + size)),
				Task.Factory.StartNew(() => readAction(repository))
			};

			Task.WaitAll(tasks);

			var actual = repository.Read().ToList();
			Assert.AreEqual((tasks.Length / 2) * size, actual.Count);
		}

		[TestMethod]
		public void MultiThreadedWriteTest()
		{
			using (var context = new KeyValueMemoryRepository(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5))
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
					Task.Factory.StartNew(() => action(repository, 1 * size, (1 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 2 * size, (2 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 3 * size, (3 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 4 * size, (4 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 5 * size, (5 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 6 * size, (6 * size) + size)),
					Task.Factory.StartNew(() => action(repository, 7 * size, (7 * size) + size))
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
		public void MultiThreadedWriteTestOverlappingEvents()
		{
			using var context = new KeyValueMemoryRepository(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1), 5);
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
			Assert.AreEqual((tasks.Length + size) - 1, actual.Count);
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
				new KeyValuePair<string, string>("Foo2", "Bar2"),
				new KeyValuePair<string, string>("Foo1", "1Bar")
			};

			var actual = repository.Read().ToArray();
			TestHelper.AreEqual(expected, actual);

			repository.Dispose();
		}

		[TestMethod]
		public void ReadInvalidKeyShouldThrowException()
		{
			var name = Guid.NewGuid().ToString();
			using var context = new KeyValueMemoryRepository(name, TimeSpan.FromSeconds(1), 10);
			var repository = context;
			TestHelper.ExpectedException<KeyNotFoundException>(() => repository.Read("Blah"), SpeedyException.KeyNotFound);
		}

		[TestMethod]
		public void ReadItemFromCache()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name, TimeSpan.FromSeconds(1), 10);
			repository.Write("Foo1", "Bar1");
			Thread.Sleep(1500);
			repository.Write("Bar2", "Foo2");
			repository.Write("Foo3", "Bar3");
			repository.Save();

			// Only Foo1 should be in the file.
			var actual = repository.Read("Bar2");
			Assert.AreEqual("Foo2", actual);
		}

		[TestMethod]
		public void ReadKeys()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Key1", "Item1");
			repository.Write("Key2", "Item2");
			repository.Write("Key3", "Item3");
			repository.Write("Key1", "Item1");
			repository.Save();

			var expected = new List<string> { "Key2", "Key3", "Key1" };
			var actual = repository.ReadKeys().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ReadOrderWithCaching()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name, TimeSpan.FromMinutes(1), 10);
			repository.Write("Foo4", "Bar4");
			repository.Write("Bar4", "Foo4");
			repository.Write("Yo", "Nope");
			repository.Remove("Yo");
			repository.Write("Foo1", "Bar1");
			repository.Write("Bar1", "Foo1");
			repository.Save();

			Assert.AreEqual(4, repository.Count);

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Foo4", "Bar4"),
				new("Bar4", "Foo4"),
				new("Foo1", "Bar1"),
				new("Bar1", "Foo1")
			};

			var actual = repository.Read().ToList();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ReadOrderWithNoCaching()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository(name);
			repository.Write("Foo4", "Bar4");
			repository.Write("Bar4", "Foo4");
			repository.Write("Foo1", "Bar1");
			repository.Write("Bar1", "Foo1");
			repository.Save();

			Assert.AreEqual(4, repository.Count);

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Foo4", "Bar4"),
				new("Bar4", "Foo4"),
				new("Foo1", "Bar1"),
				new("Bar1", "Foo1")
			};

			var actual = repository.Read().ToList();
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void ReadUsingCondition()
		{
			using var context = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			var repository = context;

			repository.Write(repository.Name, "Root object");
			repository.Write("ChildItem1", "ChildItemValue1");
			repository.Write("ChildItem2", "ChildItemValue2");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new("ChildItem1", "ChildItemValue1"),
				new("ChildItem2", "ChildItemValue2")
			};

			var actual = repository.Read(key => key != repository.Name).ToList();
			Assert.AreEqual(2, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void RemoveShouldRemoveItem()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
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
		public void RepositoryWithComplexRelationship()
		{
			var name = Guid.NewGuid().ToString();
			using var repository = new KeyValueMemoryRepository<ClientAccount>(name);
			var address = new ClientAddress { City = "City", Line1 = "Line1", Line2 = "Line2", Postal = "Postal", State = "State" };
			var expected = new ClientAccount { Address = address, Name = "Bob Smith" };
			var id = Guid.NewGuid().ToString();

			repository.Write(id, expected);
			repository.Save();
			repository.Flush();

			var actual = repository.Read(id);

			TestHelper.AreEqual(expected, actual);
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnFalse()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			var actual = repository.TryRead("Blah", out var value);
			Assert.AreEqual(false, actual);
			Assert.AreEqual(null, value);
		}

		[TestMethod]
		public void TryReadInvalidKeyShouldReturnTrue()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Blah", "Value");
			repository.Save();

			var actual = repository.TryRead("Blah", out var value);
			Assert.AreEqual(true, actual);
			Assert.AreEqual("Value", value);
		}

		[TestMethod]
		public void WriteItemValueWithPipeCharacter()
		{
			using var repository = new KeyValueMemoryRepository(Guid.NewGuid().ToString());
			repository.Write("Item1", "Item1|Item2");
			repository.Write("Item2", "Item2|Boo");
			repository.Write("Item3", "Item3|Foo|Bar|Again");
			repository.Save();

			var expected = new List<KeyValuePair<string, string>>
			{
				new("Item1", "Item1|Item2"),
				new("Item2", "Item2|Boo"),
				new("Item3", "Item3|Foo|Bar|Again")
			};

			var actual = repository.Read().ToList();
			Assert.AreEqual(3, actual.Count);
			TestHelper.AreEqual(expected, actual);
		}

		#endregion
	}
}