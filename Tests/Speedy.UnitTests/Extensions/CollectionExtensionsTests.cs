#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class CollectionExtensionsTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void AddIfMissingTests()
	{
		var dictionary = new Dictionary<string, string>();
		dictionary.AddIfMissing("foo", () => "bar");
		dictionary.AddIfMissing("foo", () => "bar");
		dictionary.AddIfMissing("foo", () => "bar");
		dictionary.AddIfMissing("foo", () => "bar");
		Assert.AreEqual(1, dictionary.Count);
	}

	[TestMethod]
	public void AddOrUpdateTestsForCollections()
	{
		var collection = new Collection<Account>();
		collection.AddOrUpdate(x => x.Name == "John", () => new Account(), x =>
		{
			x.Name = "John";
			x.EmailAddress = "john.doe@domain.com";
		});

		AreEqual(1, collection.Count);
		AreEqual("John", collection[0].Name);
		AreEqual("john.doe@domain.com", collection[0].EmailAddress);

		collection.AddOrUpdate(x => x.Name == "John", () => new Account(), x => x.EmailAddress = "john.doe@hotmail.com");

		AreEqual(1, collection.Count);
		AreEqual("John", collection[0].Name);
		AreEqual("john.doe@hotmail.com", collection[0].EmailAddress);
	}

	[TestMethod]
	public void AddOrUpdateTestsForDictionary()
	{
		var dictionary = new Dictionary<string, string>();
		dictionary.AddOrUpdate("foo", "bar1");
		dictionary.AddOrUpdate("foo", "bar2");
		dictionary.AddOrUpdate("foo", "bar3");
		dictionary.AddOrUpdate("foo", "bar4");
		Assert.AreEqual(1, dictionary.Count);
		Assert.AreEqual("bar4", dictionary["foo"]);
	}

	[TestMethod]
	public void EmptyConcurrentQueue()
	{
		var queue = new ConcurrentQueue<int>();
		queue.Enqueue(0);
		queue.Enqueue(1);
		queue.Enqueue(2);
		queue.Enqueue(3);

		AreEqual(4, queue.Count);

		queue.Empty();

		AreEqual(0, queue.Count);
	}

	[TestMethod]
	public void NaturalSort()
	{
		var data = new[] { "100", "11", "9", "2" };
		var expected = new[] { "2", "9", "11", "100" };
		AreEqual(expected, data.NaturalSort().ToArray());
	}

	[TestMethod]
	public void ReconcileCollectionDefaults()
	{
		var updates = new SpeedyList<Account> { OrderBy = new[] { new OrderBy<Account>(x => x.Id) } };
		var existing = new SpeedyList<Account> { OrderBy = new[] { new OrderBy<Account>(x => x.Id) } };
		var addedAccount = new Account { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com" };
		var updatedAccount = new Account { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com" };
		var removedAccount = new Account { Id = 3, Name = "Foo Bar", EmailAddress = "foo@bar.com" };
		var accountUpdate = new Account { Id = 2, Name = "Jane", EmailAddress = "jane.doe@domain.com" };
		Assert.AreEqual(0, updates.Count);
		Assert.AreEqual(0, existing.Count);

		updates.Add(addedAccount);
		updates.Add(updatedAccount);
		existing.Add(removedAccount);
		existing.Add(accountUpdate);
		Assert.AreEqual(2, updates.Count);
		Assert.AreEqual(2, existing.Count);

		existing.Reconcile(updates);

		Assert.AreEqual(2, updates.Count);
		Assert.AreEqual(2, existing.Count);

		var expectedAccount1 = new Account { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com" };
		var expectedAccount2 = new Account { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com" };

		AreEqual(existing.ToArray(), new[] { expectedAccount1, expectedAccount2 });
	}

	[TestMethod]
	public void ReconcileCollections()
	{
		var updates = new SpeedyList<Account> { OrderBy = new[] { new OrderBy<Account>(x => x.Id) } };
		var existing = new SpeedyList<ClientAccount> { OrderBy = new[] { new OrderBy<ClientAccount>(x => x.Id) } };
		var addedAccount = new Account { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com", SyncId = Guid.Parse("F872F61D-EC7C-40DC-8811-04BB5DBEC6C5") };
		var updatedAccount = new Account { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com", SyncId = Guid.Parse("402516E7-D063-46E9-B1AF-79A24C0B8055") };
		var removedAccount = new ClientAccount { Id = 3, Name = "Foo Bar", EmailAddress = "foo@bar.com", SyncId = Guid.Parse("50BF63A1-098D-44A4-8B69-A85E1CC9172A") };
		var accountUpdate = new ClientAccount { Id = 2, Name = "Jane", EmailAddress = "jane.doe@domain.com", SyncId = Guid.Parse("402516E7-D063-46E9-B1AF-79A24C0B8055") };
		Assert.AreEqual(0, updates.Count);
		Assert.AreEqual(0, existing.Count);

		updates.Add(addedAccount);
		updates.Add(updatedAccount);
		existing.Add(removedAccount);
		existing.Add(accountUpdate);
		Assert.AreEqual(2, updates.Count);
		Assert.AreEqual(2, existing.Count);

		existing.Reconcile(updates, (ca, a) => a.Id == ca.Id, a => new ClientAccount());

		Assert.AreEqual(2, updates.Count);
		Assert.AreEqual(2, existing.Count);

		var expectedAccount1 = new ClientAccount { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com", SyncId = Guid.Parse("F872F61D-EC7C-40DC-8811-04BB5DBEC6C5") };
		var expectedAccount2 = new ClientAccount { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com", SyncId = Guid.Parse("402516E7-D063-46E9-B1AF-79A24C0B8055") };

		AreEqual(existing.ToArray(), new[] { expectedAccount1, expectedAccount2 });
	}

	[TestMethod]
	public void ReconcileCollectionShouldNotFireChanges()
	{
		var collection1 = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
		var collection2 = new List<int> { 1, 2, 3, 4, 5 };
		var count = 0;

		collection1.CollectionChanged += (sender, args) => count++;
		collection1.Reconcile(collection2);

		AreEqual(0, count);

		collection2.Add(6);
		collection1.Reconcile(collection2);

		AreEqual(1, count);
	}

	[TestMethod]
	public void RemoveDuplicateSequentialValues()
	{
		var values = new List<Address>
		{
			new Address { State = "SC" },
			new Address { State = "SC" },
			new Address { State = "NC" },
			new Address { State = "NC" },
			new Address { State = "NC" },
			new Address { State = "GA" },
			new Address { State = "GA" }
		};

		var expected = new List<Address> { values[0], values[2], values[5] };
		var actual = values.ExcludeSequentialDuplicates(x => x.State).ToList();

		AreEqual(expected, actual);
	}

	[TestMethod]
	public void RemoveDuplicateSequentialValuesWithMaximumTIme()
	{
		var values = new List<Address>
		{
			new Address { State = "SC", ModifiedOn = DateTime.Parse("03/24/2022 12:00:00 PM") },
			new Address { State = "SC", ModifiedOn = DateTime.Parse("03/24/2022 12:00:01 PM") },
			new Address { State = "NC", ModifiedOn = DateTime.Parse("03/24/2022 12:00:02 PM") },
			new Address { State = "NC", ModifiedOn = DateTime.Parse("03/24/2022 12:00:03 PM") },
			new Address { State = "NC", ModifiedOn = DateTime.Parse("03/24/2022 12:00:08 PM") },
			new Address { State = "GA", ModifiedOn = DateTime.Parse("03/24/2022 12:00:09 PM") },
			new Address { State = "GA", ModifiedOn = DateTime.Parse("03/24/2022 12:00:10 PM") }
		};

		var expected = new List<Address> { values[0], values[2], values[4], values[5] };
		var actual = values
			.ExcludeSequentialDuplicates(x => x.State,
				(c, n) =>
				{
					var d = n.ModifiedOn - c.ModifiedOn;
					return d.TotalSeconds < 5;
				})
			.ToList();

		AreEqual(expected, actual);
	}

	#endregion
}