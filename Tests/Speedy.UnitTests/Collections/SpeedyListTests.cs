#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Collections;

[TestClass]
public class SpeedyListTests : BaseCollectionTests
{
	#region Methods

	[TestMethod]
	public void Add()
	{
		var list = new SpeedyList<int>();

		AreEqual(0, list.Count);

		list.Add(1);
		list.Add((object) 2);

		AreEqual(new[] { 1, 2 }, list.ToArray());

		ExpectedException<ArgumentException>(() => list.Add("boom"), "The item is the incorrect value type.");
	}

	[TestMethod]
	public void AddRange()
	{
		var list = new SpeedyList<int>();
		list.AddRange(8, 4, 6, 2);
		AreEqual(new[] { 8, 4, 6, 2 }, list.ToArray());

		list.AddRange(5, 7, 3, 1);
		AreEqual(new[] { 8, 4, 6, 2, 5, 7, 3, 1 }, list.ToArray());

		list = new SpeedyList<int>(new OrderBy<int>());
		list.AddRange(8, 4, 6, 2);
		AreEqual(new[] { 2, 4, 6, 8 }, list.ToArray());

		list.AddRange(5, 7, 3, 1);
		AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, list.ToArray());
	}

	[TestMethod]
	public void AddWithOrderByShouldAutomaticallySort()
	{
		var list = new SpeedyList<ClientAccount>
		{
			OrderBy = new[]
			{
				new OrderBy<ClientAccount>(x => x.IsDeleted),
				new OrderBy<ClientAccount>(x => x.CreatedOn, true)
			}
		};

		var actual = new List<NotifyCollectionChangedEventArgs>();
		list.CollectionChanged += (_, args) => actual.Add(args);

		list.Add(new ClientAccount { Name = "John", IsDeleted = true, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) });
		list.Add(new ClientAccount { Name = "Jane", IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) });

		AreEqual(2, actual.Count);
		AreEqual(NotifyCollectionChangedAction.Add, actual[0].Action);
		AreEqual(0, actual[0].NewStartingIndex);
		AreEqual(-1, actual[0].OldStartingIndex);
		AreEqual(NotifyCollectionChangedAction.Add, actual[1].Action);
		AreEqual(0, actual[1].NewStartingIndex);
		AreEqual(-1, actual[1].OldStartingIndex);

		AreEqual("Jane", list[0].Name);
		AreEqual("John", list[1].Name);
	}

	[TestMethod]
	public void Clear()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4, 5, 6, 7, 8);
		list.Clear();
		AreEqual(Array.Empty<int>(), list.ToArray());

		list.Clear();
		AreEqual(Array.Empty<int>(), list.ToArray());
	}

	[TestMethod]
	public void ClearEmptyCollection()
	{
		var changes = 0;
		var collection = new SpeedyList<string>();
		collection.CollectionChanged += (_, _) => changes++;
		collection.Clear();
		AreEqual(0, changes);
	}

	[TestMethod]
	public void ClearShouldCallOnChangedEvent()
	{
		var collection = new SpeedyList<ClientAccount>();
		var actual = new List<NotifyCollectionChangedEventArgs>();
		collection.Add(new ClientAccount());
		collection.Add(new ClientAccount());

		collection.CollectionChanged += (_, args) => actual.Add(args);
		collection.Clear();
		AreEqual(2, actual.Count);

		var actualEvent = actual[0];
		AreEqual(NotifyCollectionChangedAction.Remove, actualEvent.Action);
		Assert.IsNotNull(actualEvent.OldItems);
		AreEqual(1, actualEvent.OldItems.Count);
		AreEqual(0, actualEvent.OldStartingIndex);
		AreEqual(null, actualEvent.NewItems);
		AreEqual(-1, actualEvent.NewStartingIndex);
		
		actualEvent = actual[1];
		AreEqual(NotifyCollectionChangedAction.Remove, actualEvent.Action);
		Assert.IsNotNull(actualEvent.OldItems);
		AreEqual(1, actualEvent.OldItems.Count);
		AreEqual(0, actualEvent.OldStartingIndex);
		AreEqual(null, actualEvent.NewItems);
		AreEqual(-1, actualEvent.NewStartingIndex);
	}

	[TestMethod]
	public void CollectionShouldSort()
	{
		var collection = new SpeedyList<string> { OrderBy = new[] { new OrderBy<string>(x => x) } };
		collection.AddRange("b", "d", "c", "a");
		TestHelper.AreEqual(new[] { "a", "b", "c", "d" }, collection.ToArray());
	}

	[TestMethod]
	public void Constructors()
	{
		var scenarios = new[]
		{
			new SpeedyList<int>(1, 2, 3, 4),
			new SpeedyList<int>(new List<int> { 1, 2, 3, 4 }),
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			new SpeedyList<int>(new[] { 1, 2, 3, 4 }),
			new SpeedyList<int>(new[] { 4, 2, 1, 3 }, new OrderBy<int>())
		};

		foreach (var scenario in scenarios)
		{
			AreEqual(4, scenario.Count);
			AreEqual(new[] { 1, 2, 3, 4 }, scenario.ToArray());
		}

		var list = new SpeedyList<int>();
		IsNull(list.GetDispatcher());

		var dispatcher = new TestDispatcher();
		scenarios = new[]
		{
			new SpeedyList<int>(dispatcher, 1, 2, 3, 4),
			new SpeedyList<int>(dispatcher, new List<int> { 1, 2, 3, 4 }),
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			new SpeedyList<int>(dispatcher, new[] { 1, 2, 3, 4 }),
			new SpeedyList<int>(dispatcher, new[] { 4, 2, 1, 3 }, new OrderBy<int>())
		};

		foreach (var scenario in scenarios)
		{
			AreEqual(dispatcher, scenario.GetDispatcher());
			AreEqual(4, scenario.Count);
			AreEqual(new[] { 1, 2, 3, 4 }, scenario.ToArray());
		}
	}

	[TestMethod]
	public void Contains()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		IsTrue(list.Contains(2));
		IsFalse(list.Contains(9));

		// Cast as object
		IsTrue(list.Contains((object) 2));
		IsFalse(list.Contains((object) 9));
	}

	[TestMethod]
	public void CopyTo()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		var array = new int[4];
		list.CopyTo(array, 0);
		AreEqual(array, list, false, null,
			nameof(list.IsFixedSize),
			nameof(list.IsSynchronized)
		);

		var array2 = (Array) new int[10];
		list.CopyTo(array2, 4);
		AreEqual(new[] { 0, 0, 0, 0, 1, 2, 3, 4, 0, 0 }, array2);
	}

	[TestMethod]
	public void Defaults()
	{
		var list = new SpeedyList<int>();
		AreEqual(false, list.IsFixedSize);
		AreEqual(false, list.IsReadOnly);
		AreEqual(true, list.IsSynchronized);
		AreEqual(false, list.IsReadLocked);
		AreEqual(false, list.IsUpgradeableReadLockHeld);
		IsNotNull(list.SyncRoot);
	}

	[TestMethod]
	public void Dispatcher()
	{
		var dispatcher = new TestDispatcher();
		var list = new SpeedyList<int>(dispatcher);
		AreEqual(dispatcher, list.GetDispatcher());
	}

	[TestMethod]
	public void DistinctCollection()
	{
		var collection = new SpeedyList<string>
		{
			ComparerFunction = string.Equals
		};
		collection.AddRange("test", "test", "test", "test");
		AreEqual(1, collection.Count);
		AreEqual("test", collection[0]);
	}

	[TestMethod]
	public void DistinctSortedObservableShouldNotAllowDuplicates()
	{
		var collection = new SpeedyList<ClientAccount>
		{
			ComparerFunction = (x, y) => x.SyncId == y.SyncId,
			OrderBy = new[] { new OrderBy<ClientAccount>(x => x.SyncId) }
		};
		var account1 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
		var account2 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
		collection.Add(account1);
		collection.Add(account2);
		AreEqual(1, collection.Count);
	}

	[TestMethod]
	public void Enumerator()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		var enumerable = (IEnumerable) list;
		var enumerator = enumerable.GetEnumerator();
		var expected = 1;

		while (enumerator.MoveNext())
		{
			AreEqual(expected++, enumerator.Current);
		}
	}

	[TestMethod]
	public void FilterCollectionShouldFilter()
	{
		var list = new SpeedyList<int>
		{
			OrderBy = new[] { new OrderBy<int>(x => x) },
			IncludeInFilter = x => (x % 3) == 0
		};

		list.Add(5);
		list.Add(1);
		list.Add(4);
		list.Add(2);
		list.Add(3);

		list.Insert(1, 7);
		list.Insert(4, 6);
		list.Insert(3, 9);
		list.Insert(7, 8);

		// We should have only been alerted with anything divisible by 3
		AreEqual(3, list.Filtered.Count);
		AreEqual(new[] { 3, 6, 9 }, list.Filtered.ToArray());

		list.Remove(3);

		// Filter collection should have changed
		AreEqual(2, list.Filtered.Count);
		AreEqual(new[] { 6, 9 }, list.Filtered.ToArray());

		list.Remove(1);
		list.Remove(2);
		list.Remove(7);
		list.Remove(8);

		// Nothing should have changed
		AreEqual(2, list.Filtered.Count);
		AreEqual(new[] { 6, 9 }, list.Filtered.ToArray());

		list.Add(3);

		// 3 should have been re-added
		AreEqual(3, list.Filtered.Count);
		AreEqual(new[] { 3, 6, 9 }, list.Filtered.ToArray());
	}

	[TestMethod]
	public void FilterCollectionShouldRefreshOnFilterChange()
	{
		var list = new SpeedyList<string>
		{
			IncludeInFilter = x => x.Contains("Bar")
		};

		// First entry should not be included in filtered collection
		list.Add("Hello");
		AreEqual(0, list.Filtered.Count);
		AreEqual(1, list.Count);

		// Second entry should be included in filtered collection
		list.Add("Foo Bar");
		AreEqual(1, list.Filtered.Count);
		AreEqual(2, list.Count);

		// Changing the second entry should not change anything until after filter.
		list[1] = "foo";
		AreEqual("Foo Bar", list.Filtered[0]);
		AreEqual(1, list.Filtered.Count);
		AreEqual(2, list.Count);

		// After refresh the filter collection should be empty
		list.RefreshFilter();
		AreEqual(0, list.Filtered.Count);
		AreEqual(2, list.Count);

		list.IncludeInFilter = x => x.Contains("foo");
		AreEqual("foo", list.Filtered[0]);
		AreEqual(1, list.Filtered.Count);
		AreEqual(2, list.Count);
	}

	[TestMethod]
	public void First()
	{
		var list = new SpeedyList<string>("a", "b", "c");
		var actual = list.First();
		AreEqual("a", actual);

		list.Clear();
		AreEqual(0, list.Count);

		ExpectedException<InvalidOperationException>(() => list.First(), "Sequence contains no elements");
	}

	[TestMethod]
	public void FirstOrDefault()
	{
		var list = new SpeedyList<string>("a", "b", "c");
		var actual = list.FirstOrDefault();
		AreEqual("a", actual);
	}

	[TestMethod]
	public void Indexer()
	{
		var list = new SpeedyList<string>("a", "b", "c");
		AreEqual("a", list[0]);
		AreEqual("b", list[1]);
		AreEqual("c", list[2]);

		list[1] = "d";
		AreEqual("d", list[1]);

		ExpectedException<ArgumentOutOfRangeException>(() => _ = list[-1], "Specified argument was out of the range of valid values.");
		ExpectedException<ArgumentOutOfRangeException>(() => _ = list[3], "Specified argument was out of the range of valid values.");

		var list2 = (IList) list;
		AreEqual("a", list2[0]);
		AreEqual("d", list2[1]);
		AreEqual("c", list2[2]);

		list2[1] = "b";
		AreEqual("b", list2[1]);

		ExpectedException<ArgumentOutOfRangeException>(() => _ = list2[-1], "Specified argument was out of the range of valid values.");
		ExpectedException<ArgumentOutOfRangeException>(() => _ = list2[3], "Specified argument was out of the range of valid values.");
	}

	[TestMethod]
	public void IndexOf()
	{
		var list = new SpeedyList<string>("a", "b", "c");

		AreEqual(1, list.IndexOf("b"));
		AreEqual(1, list.IndexOf((object) "b"));

		AreEqual(-1, list.IndexOf("z"));
		AreEqual(-1, list.IndexOf(string.Empty));
		AreEqual(-1, list.IndexOf(null));
		AreEqual(-1, list.IndexOf(TimeService.UtcNow));

		list.ComparerFunction = Equals;

		AreEqual(1, list.IndexOf("b"));
		AreEqual(-1, list.IndexOf("z"));
	}

	[TestMethod]
	public void Insert()
	{
		var list = new SpeedyList<int>();
		list.Insert(0, 42);

		AreEqual(1, list.Count);
		AreEqual(42, list[0]);

		list.Insert(0, (object) 44);

		AreEqual(2, list.Count);
		AreEqual(44, list[0]);
		AreEqual(42, list[1]);

		// This insert should not duplicate because 44 already in list
		list.ComparerFunction = (x, y) => x.Equals(y);
		list.Insert(1, (object) 44);

		AreEqual(2, list.Count);
		AreEqual(44, list[0]);
		AreEqual(42, list[1]);

		list.Insert(1, 46);
		AreEqual(3, list.Count);
		AreEqual(44, list[0]);
		AreEqual(46, list[1]);
		AreEqual(42, list[2]);
	}

	[TestMethod]
	public void Last()
	{
		var list = new SpeedyList<string>("a", "b", "c");
		var actual = list.Last();
		AreEqual("c", actual);

		list.Clear();
		AreEqual(0, list.Count);

		ExpectedException<InvalidOperationException>(() => list.Last(), "Sequence contains no elements");
	}

	[TestMethod]
	public void LastOrDefault()
	{
		var list = new SpeedyList<string>("a", "b", "c");
		var actual = list.LastOrDefault();
		AreEqual("c", actual);
	}

	[TestMethod]
	public void Limit()
	{
		var list = new SpeedyList<int> { Limit = 5 };

		list.Add(1);
		list.Add(2);
		list.Add(3);
		list.Add(4);
		list.Add(5);
		list.Add(6);
		list.Insert(1, 7);

		AreEqual(5, list.Count);
		AreEqual(new[] { 2, 7, 3, 4, 5 }, list.ToArray());

		list.Load(9, 8, 7, 6, 5, 4, 3, 2, 1);

		AreEqual(5, list.Count);
		AreEqual(new[] { 9, 8, 7, 6, 5 }, list.ToArray());
	}

	[TestMethod]
	public void LimitedCollectionShouldSortFirstThenLimit()
	{
		var collection = new SpeedyList<int> { Limit = 3, OrderBy = new[] { new OrderBy<int>(x => x) } };
		var actual = new List<NotifyCollectionChangedEventArgs>();
		collection.CollectionChanged += (_, args) => actual.Add(args);

		collection.Add(5);
		collection.Add(1);
		collection.Add(2);

		var expected = new[] { 1, 2, 5 };
		TestHelper.AreEqual(expected, collection.ToArray());

		expected = new[] { 1, 2, 3 };
		collection.Add(3);
		TestHelper.AreEqual(expected, collection.ToArray());

		actual.Select(x => x.Action.ToString()).DumpJson();

		AreEqual(5, actual.Count);
		AreEqual(NotifyCollectionChangedAction.Add, actual[0].Action); // Add 5 = 5
		AreEqual(NotifyCollectionChangedAction.Add, actual[1].Action); // Add 1 = 1,5
		AreEqual(NotifyCollectionChangedAction.Add, actual[2].Action); // Add 2 = 1,2,5
		AreEqual(NotifyCollectionChangedAction.Add, actual[3].Action); // Add 3 = 1,2,3,5
		AreEqual(NotifyCollectionChangedAction.Remove, actual[4].Action); // Remove 5 = 1,2,3
	}

	[TestMethod]
	public void Load()
	{
		var list = new SpeedyList<int>();
		var changeCount = 0;
		var itemsCount = 0;

		void onListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			changeCount++;
			itemsCount += args.NewItems?.Count ?? 0;
			IsNull(args.OldItems);
		}

		list.CollectionChanged += onListOnCollectionChanged;

		try
		{
			AreEqual(0, list.Count);

			list.Load(1, 2, 3, 4, 5, 6);

			AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.ToArray());
			AreEqual(6, changeCount);
			AreEqual(6, itemsCount);

			list.CollectionChanged -= onListOnCollectionChanged;
			list.Clear();
			list.CollectionChanged += onListOnCollectionChanged;

			changeCount = 0;
			itemsCount = 0;

			var array = new[] { 1, 2, 3, 4, 5, 6 };
			list.Load(array.AsEnumerable());

			AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.ToArray());
			AreEqual(6, changeCount);
			AreEqual(6, itemsCount);
		}
		finally
		{
			list.CollectionChanged -= onListOnCollectionChanged;
		}
	}

	[TestMethod]
	public void ManualSortShouldReorder()
	{
		var collection = new SpeedyList<ClientAccount>
		{
			OrderBy = new[]
			{
				new OrderBy<ClientAccount>(x => x.IsDeleted),
				new OrderBy<ClientAccount>(x => x.CreatedOn, true)
			}
		};

		var changeEvents = new List<NotifyCollectionChangedEventArgs>();
		collection.CollectionChanged += (_, args) => changeEvents.Add(args);

		var client1 = new ClientAccount { Id = 1, IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) };
		var client2 = new ClientAccount { Id = 2, IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 42) };
		collection.Add(client1);
		collection.Add(client2);

		AreEqual(new[] { client1, client2 }, collection.ToArray());

		// Should be standard events because no sorting should take place
		AreEqual(2, changeEvents.Count);
		AreEqual(changeEvents[0], new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, client1, 0));
		AreEqual(NotifyCollectionChangedAction.Add, changeEvents[1].Action);
		AreEqual(1, changeEvents[1].NewStartingIndex);
		AreEqual(-1, changeEvents[1].OldStartingIndex);
		AreEqual(client1.Id, collection[0].Id);
		AreEqual(client2.Id, collection[1].Id);

		// This should push the items into the second position on sort
		changeEvents.Clear();
		client1.IsDeleted = true;
		collection.Sort();

		// Should be a single move event
		AreEqual(1, changeEvents.Count);
		AreEqual(changeEvents[0], new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, client2, 0, 1));
		AreEqual(new[] { client2, client1 }, collection.ToArray());
	}

	[TestMethod]
	public void OnPropertyChanged()
	{
		var collection = new TestCollection();
		var actual = new List<string>();

		void onCollectionOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			actual.Add(args.PropertyName);
		}

		try
		{
			// will cause default property changes
			collection.PropertyChanged += onCollectionOnPropertyChanged;
			collection.Add(1);
			AreEqual(new[] { "Count" }, actual.ToArray(), actual.DumpJson());

			// should cause a property change
			actual.Clear();
			collection.Name = "aoeu";
			AreEqual(new[] { "Name" }, actual.ToArray(), actual.DumpJson());
		}
		finally
		{
			collection.PropertyChanged -= onCollectionOnPropertyChanged;
		}
	}

	[TestMethod]
	public virtual void ParallelShouldWork()
	{
		var list = new SpeedyList<int>();
		TestParallelOperations(list);
	}

	[TestMethod]
	public void ProcessThenSortWithAction()
	{
		var list = new SpeedyList<int> { OrderBy = new[] { new OrderBy<int>(x => x) } };
		var expected = Enumerable.Range(1, 99).ToArray();

		list.ProcessThenSort(() =>
		{
			for (var x = 99; x > 0; x--)
			{
				list.Add(x);
			}
		});

		AreEqual(expected, list.ToArray());
	}

	[TestMethod]
	public void ProcessThenSortWithParallelAction()
	{
		var list = new SpeedyList<int> { OrderBy = new[] { new OrderBy<int>(x => x) } };
		var expected = Enumerable.Range(1, 99).ToArray();

		list.ProcessThenSort(() => { Parallel.For(1, 100, x => { list.Add(x); }); });

		AreEqual(expected, list.ToArray());
	}

	[TestMethod]
	public void Remove()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		list.Remove(2);
		AreEqual(new[] { 1, 3, 4 }, list.ToArray());

		list.Remove((object) 3);
		AreEqual(new[] { 1, 4 }, list.ToArray());

		list.Remove(3);
		AreEqual(new[] { 1, 4 }, list.ToArray());
	}

	[TestMethod]
	public void RemoveAt()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4, 5, 6, 7, 8);
		list.RemoveAt(0);
		AreEqual(new[] { 2, 3, 4, 5, 6, 7, 8 }, list.ToArray());

		// All of these are out of range.
		ExpectedException<ArgumentOutOfRangeException>(() => list.RemoveAt(list.Count),
			"Index was out of range. Must be non-negative and less than the size of the collection.");
		ExpectedException<ArgumentOutOfRangeException>(() => list.RemoveAt(int.MaxValue),
			"Index was out of range. Must be non-negative and less than the size of the collection.");
		ExpectedException<ArgumentOutOfRangeException>(() => list.RemoveAt(int.MinValue),
			"Index was out of range. Must be non-negative and less than the size of the collection.");

		AreEqual(new[] { 2, 3, 4, 5, 6, 7, 8 }, list.ToArray());
	}

	[TestMethod]
	public void RemoveRange()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4, 5, 6, 7, 8);
		list.RemoveRange(3, 2);
		AreEqual(new[] { 1, 2, 3, 6, 7, 8 }, list.ToArray());

		ExpectedException<ArgumentException>(() => list.RemoveRange(5, 2),
			"Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
		ExpectedException<ArgumentException>(() => list.RemoveRange(5, 20),
			"Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");

		AreEqual(new[] { 1, 2, 3, 6, 7, 8 }, list.ToArray());
	}

	[TestMethod]
	public void RemoveUsingPredicate()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		list.Remove(x => (x % 2) == 0);
		AreEqual(new[] { 1, 3 }, list.ToArray());

		list = new SpeedyList<int>(1, 2, 3, 4);
		list.Remove(x => x.IsEven());
		AreEqual(new[] { 1, 3 }, list.ToArray());

		list = new SpeedyList<int>(1, 2, 3, 4);
		list.Remove(x => x.IsOdd());
		AreEqual(new[] { 2, 4 }, list.ToArray());

		list = new SpeedyList<int>(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
		list.Remove(x => x < 6);
		AreEqual(new[] { 6, 7, 8, 9, 10 }, list.ToArray());

		list.Remove(x => x >= 10);
		AreEqual(new[] { 6, 7, 8, 9 }, list.ToArray());

		list.Remove(x => x <= 6);
		AreEqual(new[] { 7, 8, 9 }, list.ToArray());
	}

	[TestMethod]
	public void SortAlpha()
	{
		var collection = new SpeedyList<Account> { OrderBy = new[] { new OrderBy<Account>(x => x.Name) } };
		var actual = new List<NotifyCollectionChangedEventArgs>();
		var accounts = new[] { new Account(), new Account(), new Account(), new Account(), new Account() };

		collection.Add(accounts[0]);
		collection.Add(accounts[1]);
		collection.Add(accounts[2]);
		collection.Add(accounts[3]);
		collection.Add(accounts[4]);

		collection.CollectionChanged += (_, args) => actual.Add(args);

		accounts[0].Name = "c";
		accounts[1].Name = "b";
		accounts[2].Name = "e";
		accounts[3].Name = "a";
		accounts[4].Name = "d";

		collection.Sort();

		Console.WriteLine(string.Join("", collection.SelectMany(x => x.Name)));

		AreEqual(3, actual.Count);
	}

	[TestMethod]
	public void SortAlphaNumeric()
	{
		var collection = new SpeedyList<Account>
		{
			OrderBy = new[]
			{
				new OrderBy<Account>(x => x.Name),
				new OrderBy<Account>(x => x.Id)
			}
		};
		var actual = new List<NotifyCollectionChangedEventArgs>();
		var accounts = new[] { new Account(), new Account(), new Account(), new Account(), new Account() };

		collection.Add(accounts[0]);
		collection.Add(accounts[1]);
		collection.Add(accounts[2]);
		collection.Add(accounts[3]);
		collection.Add(accounts[4]);

		collection.CollectionChanged += (_, args) => actual.Add(args);

		accounts[0].Name = "c";
		accounts[0].Id = 0;
		accounts[1].Name = "b3";
		accounts[1].Id = 3;
		accounts[2].Name = "a";
		accounts[2].Id = 1;
		accounts[3].Name = "b4";
		accounts[3].Id = 4;
		accounts[4].Name = "b2";
		accounts[4].Id = 2;

		collection.Sort();

		var displayNames = string.Join("", collection.SelectMany(x => x.Name));
		var ids = string.Join("", collection.SelectMany(x => x.Id.ToString()));
		Console.WriteLine(displayNames);
		Console.WriteLine(ids);

		AreEqual(4, actual.Count);
		AreEqual("ab2b3b4c", displayNames);
		AreEqual("12340", ids);
	}

	[TestMethod]
	public void SortedObservableShouldAllowDuplicates()
	{
		var collection = new SpeedyList<ClientAccount>
		{
			OrderBy = new[] { new OrderBy<ClientAccount>(x => x.SyncId) }
		};
		var account1 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
		var account2 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
		collection.Add(account1);
		collection.Add(account2);
		AreEqual(2, collection.Count);
	}

	[TestMethod]
	public void ThreadSafeSortedCollection()
	{
		var count = 1000;
		var collection = new SpeedyList<int> { OrderBy = new[] { new OrderBy<int>(x => x) } };
		var options = new ParallelOptions { MaxDegreeOfParallelism = 1 };

		Parallel.For(0, 1000, options, (x, _) => { collection.Add(x); });

		var expected = Enumerable.Range(0, count).ToArray();
		var actual = collection.ToArray();

		TestHelper.AreEqual(expected, actual);
	}

	[TestMethod]
	public void TryGetAndRemoveAt()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		IsTrue(list.TryGetAndRemoveAt(1, out var value));
		AreEqual(2, value);
		AreEqual(new[] { 1, 3, 4 }, list.ToArray());

		IsFalse(list.TryGetAndRemoveAt(3, out value));
		AreEqual(0, value);
		AreEqual(new[] { 1, 3, 4 }, list.ToArray());
	}

	#endregion

	#region Classes

	private class TestCollection : SpeedyList<int>
	{
		#region Properties

		public string Name { get; set; }

		#endregion
	}

	#endregion
}