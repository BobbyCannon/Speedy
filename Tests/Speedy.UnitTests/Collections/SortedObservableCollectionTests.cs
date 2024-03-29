﻿#region References

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Collections
{
	[TestClass]
	public class SortedObservableCollectionTests : SpeedyUnitTest
	{
		#region Methods

		[TestMethod]
		public void AddShouldAutomaticallyOrder()
		{
			var collection = new SortedObservableCollection<ClientAccount>(
				new OrderBy<ClientAccount>(x => x.IsDeleted),
				new OrderBy<ClientAccount>(x => x.CreatedOn, true)
			);

			var actual = new List<NotifyCollectionChangedEventArgs>();
			collection.CollectionChanged += (sender, args) => actual.Add(args);

			collection.Add(new ClientAccount { IsDeleted = true, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) });
			collection.Add(new ClientAccount { IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) });

			Assert.AreEqual(3, actual.Count);
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[0].Action);
			Assert.AreEqual(0, actual[0].NewStartingIndex);
			Assert.AreEqual(-1, actual[0].OldStartingIndex);
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[1].Action);
			Assert.AreEqual(1, actual[1].NewStartingIndex);
			Assert.AreEqual(-1, actual[1].OldStartingIndex);
			Assert.AreEqual(NotifyCollectionChangedAction.Move, actual[2].Action);
			Assert.AreEqual(0, actual[2].NewStartingIndex);
			Assert.AreEqual(1, actual[2].OldStartingIndex);
		}

		[TestMethod]
		public void CollectionShouldSort()
		{
			var collection = new SortedObservableCollection<string>(new OrderBy<string>(x => x));
			collection.AddRange("b", "d", "c", "a");
			AreEqual(new[] { "a", "b", "c", "d" }, collection.ToArray());
		}

		[TestMethod]
		public void DistinctSortedObservableShouldNotAllowDuplicates()
		{
			var collection = new SortedObservableCollection<ClientAccount>(new OrderBy<ClientAccount>(x => x.SyncId))
			{
				DistinctCheck = (x, y) => x.SyncId == y.SyncId
			};
			var account1 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
			var account2 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
			collection.Add(account1);
			collection.Add(account2);
			Assert.AreEqual(1, collection.Count);
		}

		[TestMethod]
		public void LimitedCollectionShouldSortFirstThenLimit()
		{
			var collection = new SortedObservableCollection<int>(new OrderBy<int>(x => x)) { Limit = 3 };
			var actual = new List<NotifyCollectionChangedEventArgs>();
			collection.CollectionChanged += (sender, args) => actual.Add(args);

			collection.Add(5);
			collection.Add(1);
			collection.Add(2);

			var expected = new[] { 1, 2, 5 };
			AreEqual(expected, collection.ToArray());

			expected = new[] { 2, 3, 5 };
			collection.Add(3);
			AreEqual(expected, collection.ToArray());

			Assert.AreEqual(8, actual.Count);
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[0].Action); // Add 5
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[1].Action); // Add 1
			Assert.AreEqual(NotifyCollectionChangedAction.Move, actual[2].Action); // Move 1
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[3].Action); // Add 2
			Assert.AreEqual(NotifyCollectionChangedAction.Move, actual[4].Action); // Move 2
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[5].Action); // Add 3
			Assert.AreEqual(NotifyCollectionChangedAction.Move, actual[6].Action); // Move 3
			Assert.AreEqual(NotifyCollectionChangedAction.Remove, actual[7].Action); // Remove 1
		}

		[TestMethod]
		public void ManualSortShouldReorder()
		{
			var collection = new SortedObservableCollection<ClientAccount>(
				new OrderBy<ClientAccount>(x => x.IsDeleted),
				new OrderBy<ClientAccount>(x => x.CreatedOn, true)
			);

			var actual = new List<NotifyCollectionChangedEventArgs>();
			collection.CollectionChanged += (sender, args) => actual.Add(args);

			collection.Add(new ClientAccount { Id = 1, IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 43) });
			collection.Add(new ClientAccount { Id = 2, IsDeleted = false, CreatedOn = new DateTime(2019, 8, 28, 9, 36, 42) });

			// Should be standard events because no sorting should take place
			Assert.AreEqual(2, actual.Count);
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[0].Action);
			Assert.AreEqual(0, actual[0].NewStartingIndex);
			Assert.AreEqual(-1, actual[0].OldStartingIndex);
			Assert.AreEqual(NotifyCollectionChangedAction.Add, actual[1].Action);
			Assert.AreEqual(1, actual[1].NewStartingIndex);
			Assert.AreEqual(-1, actual[1].OldStartingIndex);
			Assert.AreEqual(1, collection[0].Id);
			Assert.AreEqual(2, collection[1].Id);

			// This should push the items into the second position on sort
			actual.Clear();
			collection[0].IsDeleted = true;
			collection.Sort();

			// Should be a single move event
			Assert.AreEqual(1, actual.Count);
			Assert.AreEqual(2, collection[0].Id);
		}

		[TestMethod]
		public void SortAlpha()
		{
			var collection = new SortedObservableCollection<Account>(new OrderBy<Account>(x => x.Name));
			var actual = new List<NotifyCollectionChangedEventArgs>();
			var accounts = new[] { new Account(), new Account(), new Account(), new Account(), new Account() };

			collection.Add(accounts[0]);
			collection.Add(accounts[1]);
			collection.Add(accounts[2]);
			collection.Add(accounts[3]);
			collection.Add(accounts[4]);

			collection.CollectionChanged += (sender, args) => actual.Add(args);

			accounts[0].Name = "c";
			accounts[1].Name = "b";
			accounts[2].Name = "e";
			accounts[3].Name = "a";
			accounts[4].Name = "d";

			collection.Sort();

			Console.WriteLine(string.Join("", collection.SelectMany(x => x.Name)));

			Assert.AreEqual(3, actual.Count);
		}

		[TestMethod]
		public void SortAlphaNumeric()
		{
			var collection = new SortedObservableCollection<Account>(
				new OrderBy<Account>(x => x.Name),
				new OrderBy<Account>(x => x.Id)
			);
			var actual = new List<NotifyCollectionChangedEventArgs>();
			var accounts = new[] { new Account(), new Account(), new Account(), new Account(), new Account() };

			collection.Add(accounts[0]);
			collection.Add(accounts[1]);
			collection.Add(accounts[2]);
			collection.Add(accounts[3]);
			collection.Add(accounts[4]);

			collection.CollectionChanged += (sender, args) => actual.Add(args);

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

			Assert.AreEqual(4, actual.Count);
			Assert.AreEqual("ab2b3b4c", displayNames);
			Assert.AreEqual("12340", ids);
		}

		[TestMethod]
		public void SortedObservableShouldAllowDuplicates()
		{
			var collection = new SortedObservableCollection<ClientAccount>(new OrderBy<ClientAccount>(x => x.SyncId));
			var account1 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
			var account2 = new ClientAccount { SyncId = Guid.Parse("CE0CBFAD-504C-47BB-B7CD-0919E9A327C0") };
			collection.Add(account1);
			collection.Add(account2);
			Assert.AreEqual(2, collection.Count);
		}

		[TestMethod]
		public void ThreadSafeSortedCollection()
		{
			var count = 1000;
			var collection = new SortedObservableCollection<int>(new OrderBy<int>(x => x)) { Limit = count };

			Parallel.For(0, count, (x, s) => { collection.Add(x); });

			var expected = Enumerable.Range(0, count).ToArray();
			AreEqual(expected, collection.ToArray());
		}

		#endregion
	}
}