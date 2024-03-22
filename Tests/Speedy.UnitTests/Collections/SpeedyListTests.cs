#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Extensions;
using Speedy.Profiling;

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
		((IList) list).Add(2);

		AreEqual(new[] { 1, 2 }, list.ToArray());

		ExpectedException<ArgumentException>(() => ((IList) list).Add("boom"), "The item is the incorrect value type.");
	}

	[TestMethod]
	public void AddRange()
	{
		var list = new SpeedyList<int>();
		list.AddRange(8, 4, 6, 2);
		AreEqual(new[] { 8, 4, 6, 2 }, list.ToArray());

		list.AddRange(5, 7, 3, 1);
		AreEqual(new[] { 8, 4, 6, 2, 5, 7, 3, 1 }, list.ToArray());

		list.Clear();
		list.OrderBy = new[] { new OrderBy<int>() };

		list.AddRange(8, 4, 6, 2);
		AreEqual(new[] { 2, 4, 6, 8 }, list.ToArray());

		list.AddRange(5, 7, 3, 1);
		AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, list.ToArray());
	}

	[TestMethod]
	public void AddRangeShouldOrder()
	{
		var list = new SpeedyList<string>();
		list.OrderBy = new[] { new OrderBy<string>(x => x) };
		list.AddRange("b", "d", "c", "a");
		AreEqual(new[] { "a", "b", "c", "d" }, list.ToArray());
	}

	[TestMethod]
	public void AddWithOrderByShouldAutomaticallyOrder()
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
		var list = new SpeedyList<int>();
		list.InitializeProfiler();
		AreEqual(0, list.Count);
		AreEqual(0, list.Profiler.AddedCount.Value);
		AreEqual(0, list.Profiler.RemovedCount.Value);

		list.AddRange(1, 2, 3, 4, 5, 6, 7, 8);
		AreEqual(8, list.Count);
		AreEqual(8, list.Profiler.AddedCount.Value);
		AreEqual(0, list.Profiler.RemovedCount.Value);

		list.Clear();
		AreEqual(Array.Empty<int>(), list.ToArray());
		AreEqual(8, list.Profiler.AddedCount.Value);
		AreEqual(8, list.Profiler.RemovedCount.Value);

		list.Clear();
		AreEqual(Array.Empty<int>(), list.ToArray());
		AreEqual(8, list.Profiler.AddedCount.Value);
		AreEqual(8, list.Profiler.RemovedCount.Value);
	}

	[TestMethod]
	public void ClearEmptyListShouldNotCalledOnCollectionChangeEvent()
	{
		var changes = 0;
		var list = new SpeedyList<string>();

		void onListOnCollectionChanged(object o, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			changes++;
		}

		try
		{
			list.CollectionChanged += onListOnCollectionChanged;
			list.Clear();
			AreEqual(0, changes);
		}
		finally
		{
			list.CollectionChanged -= onListOnCollectionChanged;
		}
	}

	[TestMethod]
	public void ClearShouldCallOnCollectionChangedEvent()
	{
		var list = new SpeedyList<ClientAccount>();
		var itemsAdded = new List<ClientAccount>();
		var itemsRemoved = new List<ClientAccount>();
		var actual = new List<NotifyCollectionChangedEventArgs>();
		list.Add(new ClientAccount());
		list.Add(new ClientAccount());

		void onListUpdated(object _, SpeedyListUpdatedEventArg args)
		{
			if (args.Added != null)
			{
				itemsAdded.AddRange(args.Added.Cast<ClientAccount>());
			}
			if (args.Removed != null)
			{
				itemsRemoved.AddRange(args.Removed.Cast<ClientAccount>());
			}
		}

		void onListOnCollectionChanged(object _, NotifyCollectionChangedEventArgs args)
		{
			actual.Add(args);
		}

		try
		{
			list.CollectionChanged += onListOnCollectionChanged;
			list.ListUpdated += onListUpdated;
			list.Clear();

			AreEqual(1, actual.Count);
			AreEqual(0, itemsAdded.Count);
			AreEqual(2, itemsRemoved.Count);
		}
		finally
		{
			list.CollectionChanged -= onListOnCollectionChanged;
			list.ListUpdated -= onListUpdated;
		}

		var actualEvent = actual[0];
		AreEqual(NotifyCollectionChangedAction.Reset, actualEvent.Action);
		AreEqual(null, actualEvent.OldItems);
		AreEqual(-1, actualEvent.OldStartingIndex);
		AreEqual(null, actualEvent.NewItems);
		AreEqual(-1, actualEvent.NewStartingIndex);
	}

	[TestMethod]
	public void Constructor()
	{
		var list = new SpeedyList<int>();

		AreEqual(false, list.IsFixedSize);
		AreEqual(false, list.IsReadOnly);
		AreEqual(true, list.IsSynchronized);
		AreEqual(false, list.IsReadLockHeld);
		AreEqual(false, list.IsWriteLockHeld);

		IsNull(list.OrderBy);
		IsNull(list.Profiler);
		IsNull(list.DistinctCheck);
		IsNull(list.FilterCheck);

		IsNotNull(list.Filtered);
		IsNotNull(list.SyncRoot);

		IsTrue(list.IsSynchronized);
		IsFalse(list.IsReadOnly);
	}

	[TestMethod]
	public void Constructors()
	{
		var scenarios = new[]
		{
			new SpeedyList<int>(1, 2, 3, 4),
			//new SpeedyList<int>(new List<int> { 1, 2, 3, 4 }),
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			new SpeedyList<int>(new[] { 1, 2, 3, 4 })
			//new SpeedyList<int>(new[] { 4, 2, 1, 3 }, new OrderBy<int>())
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
			new SpeedyList<int>(null, dispatcher, null, 1, 2, 3, 4),
			//new SpeedyList<int>(dispatcher, new List<int> { 1, 2, 3, 4 }),
			// ReSharper disable once RedundantExplicitParamsArrayCreation
			new SpeedyList<int>(null, dispatcher, null, new[] { 1, 2, 3, 4 })
			//new SpeedyList<int>(dispatcher, new[] { 4, 2, 1, 3 }, new OrderBy<int>())
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
		// ReSharper disable once CollectionNeverUpdated.Local
		var list = new SpeedyList<int>(1, 2, 3, 4);
		IsTrue(list.Contains(2));
		IsFalse(list.Contains(9));

		// Cast as IList
		IsTrue(((IList) list).Contains(2));
		IsFalse(((IList) list).Contains(9));
	}

	[TestMethod]
	public void CopyTo()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		var array = new int[4];
		list.CopyTo(array, 0);
		AreEqual(array, list, false, null,
			nameof(Array.IsFixedSize),
			nameof(Array.IsSynchronized)
		);

		var array2 = (Array) new int[10];
		((IList) list).CopyTo(array2, 4);
		AreEqual(new[] { 0, 0, 0, 0, 1, 2, 3, 4, 0, 0 }, array2);
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
			DistinctCheck = string.Equals
		};
		collection.AddRange("test", "test", "test", "test");
		AreEqual(1, collection.Count);
		AreEqual("test", collection[0]);
	}

	[TestMethod]
	public void DistinctOrderedObservableShouldNotAllowDuplicates()
	{
		var collection = new SpeedyList<ClientAccount>
		{
			DistinctCheck = (x, y) => x.SyncId == y.SyncId,
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
			FilterCheck = x => (x % 3) == 0
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
		var list = new SpeedyList<string>();
		list.FilterCheck = x => x.Contains("Bar");

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

		list.FilterCheck = x => x.Contains("foo");
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
		AreEqual(1, ((IList) list).IndexOf("b"));

		AreEqual(-1, list.IndexOf("z"));
		AreEqual(-1, list.IndexOf(string.Empty));
		AreEqual(-1, list.IndexOf(null));
		AreEqual(-1, ((IList) list).IndexOf(CurrentTime));

		list.DistinctCheck = Equals;

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

		((IList) list).Insert(0, 44);

		AreEqual(2, list.Count);
		AreEqual(44, list[0]);
		AreEqual(42, list[1]);

		// This insert should not duplicate because 44 already in list
		list.DistinctCheck = (x, y) => x.Equals(y);
		((IList) list).Insert(1, 44);

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
	public void LimitedCollectionShouldOrderFirstThenLimit()
	{
		var collection = new SpeedyList<int> { Limit = 3, OrderBy = new[] { new OrderBy<int>(x => x) } };
		var actual = new List<NotifyCollectionChangedEventArgs>();
		collection.CollectionChanged += (_, args) => actual.Add(args);

		collection.Add(5);
		collection.Add(1);
		collection.Add(2);

		var expected = new[] { 1, 2, 5 };
		AreEqual(expected, collection.ToArray());

		expected = new[] { 1, 2, 3 };
		collection.Add(3);
		AreEqual(expected, collection.ToArray());

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

		void onListUpdated(object sender, SpeedyListUpdatedEventArg args)
		{
			itemsCount += args.Added.Count;
		}

		void onListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			changeCount++;
			itemsCount += args.NewItems?.Count ?? 0;
			IsNull(args.OldItems);
		}

		list.CollectionChanged += onListOnCollectionChanged;
		list.ListUpdated += onListUpdated;

		try
		{
			AreEqual(0, list.Count);

			list.Load(1, 2, 3, 4, 5, 6);

			AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.ToArray());
			AreEqual(1, changeCount);
			AreEqual(6, itemsCount);

			list.CollectionChanged -= onListOnCollectionChanged;
			list.ListUpdated -= onListUpdated;
			list.Clear();
			list.CollectionChanged += onListOnCollectionChanged;
			list.ListUpdated += onListUpdated;

			changeCount = 0;
			itemsCount = 0;

			var array = new[] { 1, 2, 3, 4, 5, 6 };
			list.Load(array.AsEnumerable());

			AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.ToArray());
			AreEqual(1, changeCount);
			AreEqual(6, itemsCount);
		}
		finally
		{
			list.CollectionChanged -= onListOnCollectionChanged;
			list.ListUpdated -= onListUpdated;
		}
	}

	[TestMethod]
	public void ManualOrderShouldReorder()
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
		collection.Order();

		// Should be a single move event
		AreEqual(1, changeEvents.Count);
		AreEqual(changeEvents[0], new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, client2, 0, 1));
		AreEqual(new[] { client2, client1 }, collection.ToArray());
	}

	[TestMethod]
	public void OnListUpdated()
	{
		var testList = new TestSpeedyList<int>();
		AreEqual(0, testList.OnListUpdatedCount.Value);
		AreEqual(0, testList.OnListUpdatedAddedCount.Value);
		AreEqual(0, testList.OnListUpdatedRemovedCount.Value);

		testList.Add(99);
		AreEqual(1, testList.OnListUpdatedCount.Value);
		AreEqual(1, testList.OnListUpdatedAddedCount.Value);
		AreEqual(0, testList.OnListUpdatedRemovedCount.Value);

		testList.Load(100, 101, 102, 104);
		// Add, Clear, Load
		AreEqual(3, testList.OnListUpdatedCount.Value);
		// Initial add plus the load, 1+4
		AreEqual(5, testList.OnListUpdatedAddedCount.Value);
		AreEqual(1, testList.OnListUpdatedRemovedCount.Value);

		testList.ResetTestCounts();
		AreEqual(0, testList.OnListUpdatedCount.Value);
		AreEqual(0, testList.OnListUpdatedAddedCount.Value);
		AreEqual(0, testList.OnListUpdatedRemovedCount.Value);

		testList.Remove(101);
		AreEqual(1, testList.OnListUpdatedCount.Value);
		AreEqual(0, testList.OnListUpdatedAddedCount.Value);
		AreEqual(1, testList.OnListUpdatedRemovedCount.Value);
	}

	[TestMethod]
	public void OnPropertyChanged()
	{
		var collection = new TestSpeedyList<int>();
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
			AreEqual(new[] { "Count" }, actual.ToArray(), () => actual.DumpJson());

			// should cause a property change
			actual.Clear();
			collection.FirstName = "aoeu";
			AreEqual(new[] { "FirstName" }, actual.ToArray(), () => actual.DumpJson());
		}
		finally
		{
			collection.PropertyChanged -= onCollectionOnPropertyChanged;
		}
	}

	[TestMethod]
	public void OrderAlpha()
	{
		var collection = new SpeedyList<ClientAccount> { OrderBy = new[] { new OrderBy<ClientAccount>(x => x.Name) } };
		var actual = new List<NotifyCollectionChangedEventArgs>();
		var accounts = new[] { new ClientAccount(), new ClientAccount(), new ClientAccount(), new ClientAccount(), new ClientAccount() };

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

		collection.Order();

		Console.WriteLine(string.Join("", collection.SelectMany(x => x.Name)));

		AreEqual(3, actual.Count);
	}

	[TestMethod]
	public void OrderAlphaNumeric()
	{
		var collection = new SpeedyList<ClientAccount>
		{
			OrderBy = new[]
			{
				new OrderBy<ClientAccount>(x => x.Name),
				new OrderBy<ClientAccount>(x => x.Id)
			}
		};
		var actual = new List<NotifyCollectionChangedEventArgs>();
		var accounts = new[] { new ClientAccount(), new ClientAccount(), new ClientAccount(), new ClientAccount(), new ClientAccount() };

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

		collection.Order();

		var displayNames = string.Join("", collection.SelectMany(x => x.Name));
		var ids = string.Join("", collection.SelectMany(x => x.Id.ToString()));
		Console.WriteLine(displayNames);
		Console.WriteLine(ids);

		AreEqual(2, actual.Count);
		AreEqual("ab2b3b4c", displayNames);
		AreEqual("12340", ids);
	}

	[TestMethod]
	public void OrderShouldAllowDuplicates()
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
	public void OrderUsingManyOrderBy()
	{
		var scenarios = new List<(OrderBy<ClientAccount>[] order, string[] addOrder, string[] expectedOrder)>
		{
			(
				new[]
				{
					new OrderBy<ClientAccount>(x => x.Name == "Zoe", true),
					new OrderBy<ClientAccount>(x => x.Name == "Zane", true),
					new OrderBy<ClientAccount>(x => x.Name)
				},
				new[] { "Bob", "Zane", "Zoe" },
				new[] { "Zoe", "Zane", "Bob" }
			),
			(
				new[]
				{
					new OrderBy<ClientAccount>(x => x.Name == "Zoe"),
					new OrderBy<ClientAccount>(x => x.Name == "Zane"),
					new OrderBy<ClientAccount>(x => x.Name)
				},
				new[] { "Bob", "Zoe", "Zane" },
				new[] { "Bob", "Zane", "Zoe" }
			)
		};

		for (var index = 0; index < scenarios.Count; index++)
		{
			$"Scenario {index}".Dump();

			var scenario = scenarios[index];
			var list = new SpeedyList<ClientAccount>(scenario.order);

			AreEqual(0, list.Count);

			scenario.addOrder.ForEach(x => list.Add(new ClientAccount { Name = x }));
			var expected = scenario.expectedOrder.Select(x => new ClientAccount { Name = x }).ToArray();

			AreEqual(expected, list.ToArray());
		}
	}

	[TestMethod]
	public void OrderWhenAssigningOrderByProperty()
	{
		var list = new SpeedyList<int>(1, 2);
		list.Insert(1, 3);

		AreEqual(new[] { 1, 3, 2 }, list.ToArray());

		list.OrderBy = new[] { new OrderBy<int>() };
		list.Order();

		AreEqual(new[] { 1, 2, 3 }, list.ToArray());
	}

	[TestMethod]
	public void ProcessThenOrderWithAction()
	{
		var list = new SpeedyList<int>();
		list.OrderBy = new[] { new OrderBy<int>(x => x) };

		var expected = Enumerable.Range(1, 99).ToArray();
		list.InitializeProfiler();

		AreEqual(0, list.Profiler.OrderCount.Value);

		list.ProcessThenOrder(() =>
		{
			for (var x = 99; x > 0; x--)
			{
				list.ProcessThenOrder(() => { list.Add(x); });
			}
		});

		AreEqual(expected, list.ToArray());
		AreEqual(1, list.Profiler.OrderCount.Value);
	}

	[TestMethod]
	public void Remove()
	{
		var list = new SpeedyList<int>();
		AreEqual(0, list.Count);

		list.AddRange(1, 2, 3, 4);
		AreEqual(4, list.Count);
		AreEqual(new[] { 1, 2, 3, 4 }, list.ToArray());

		list.Remove(2);
		AreEqual(new[] { 1, 3, 4 }, list.ToArray());

		((IList) list).Remove(3);
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
		var actual = list.ToArray();
		AreEqual(new[] { 1, 2, 3, 6, 7, 8 }, actual);

		ExpectedException<ArgumentException>(() => list.RemoveRange(5, 2),
			"Specified argument was out of the range of valid values.");
		ExpectedException<ArgumentException>(() => list.RemoveRange(5, 20),
			"Specified argument was out of the range of valid values.");

		actual = list.ToArray();
		AreEqual(new[] { 1, 2, 3, 6, 7, 8 }, actual);
	}

	[TestMethod]
	public void RemoveUsingPredicate()
	{
		var list = new SpeedyList<int>(1, 2, 3, 4);
		list.Remove(x => (x % 2) == 0);
		AreEqual(new[] { 1, 3 }, list.ToArray());

		list.Load(1, 2, 3, 4);
		list.Remove(x => x.IsEven());
		AreEqual(new[] { 1, 3 }, list.ToArray());

		list.Load(1, 2, 3, 4);
		list.Remove(x => x.IsOdd());
		AreEqual(new[] { 2, 4 }, list.ToArray());

		list.Load(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
		list.Remove(x => x < 6);
		AreEqual(new[] { 6, 7, 8, 9, 10 }, list.ToArray());

		list.Remove(x => x >= 10);
		AreEqual(new[] { 6, 7, 8, 9 }, list.ToArray());

		list.Remove(x => x <= 6);
		AreEqual(new[] { 7, 8, 9 }, list.ToArray());
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

	private class TestSpeedyList<T> : SpeedyList<T>
	{
		#region Constructors

		public TestSpeedyList()
		{
			OnListUpdatedCount = new Counter();
			OnListUpdatedAddedCount = new Counter();
			OnListUpdatedRemovedCount = new Counter();
		}

		#endregion

		#region Properties

		public string FirstName { get; set; }

		public Counter OnListUpdatedAddedCount { get; }

		public Counter OnListUpdatedCount { get; }

		public Counter OnListUpdatedRemovedCount { get; }

		#endregion

		#region Methods

		public void ResetTestCounts()
		{
			OnListUpdatedAddedCount.Reset();
			OnListUpdatedCount.Reset();
			OnListUpdatedRemovedCount.Reset();
		}

		/// <inheritdoc />
		protected override void OnListUpdated(SpeedyListUpdatedEventArg e)
		{
			OnListUpdatedCount.Increment();
			OnListUpdatedAddedCount.Increment(e.Added?.Count ?? 0);
			OnListUpdatedRemovedCount.Increment(e.Removed?.Count ?? 0);
			base.OnListUpdated(e);
		}

		#endregion
	}

	#endregion
}