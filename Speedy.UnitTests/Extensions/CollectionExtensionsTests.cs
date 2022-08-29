#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Collections;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions
{
	[TestClass]
	public class CollectionExtensionsTests
	{
		#region Methods

		[TestMethod]
		public void AddIfMissingTests()
		{
			var dictionary = new Dictionary<string, string>();
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			dictionary.AddIfMissing("foo", "bar");
			Assert.AreEqual(1, dictionary.Count);
		}

		[TestMethod]
		public void AddOrUpdateTests()
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
		public void NaturalSort()
		{
			var data = new[] { "100", "11", "9", "2" };
			var expected = new[] { "2", "9", "11", "100" };
			TestHelper.AreEqual(expected, data.NaturalSort().ToArray());
		}

		[TestMethod]
		public void ReconcileCollections()
		{
			var updates = new SortedObservableCollection<Account>(new OrderBy<Account>(x => x.Id));
			var existing = new SortedObservableCollection<ClientAccount>(new OrderBy<ClientAccount>(x => x.Id));
			var addedAccount = new Account { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com" };
			var updatedAccount = new Account { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com" };
			var removedAccount = new ClientAccount { Id = 3, Name = "Foo Bar", EmailAddress = "foo@bar.com" };
			var accountUpdate = new ClientAccount { Id = 2, Name = "Jane", EmailAddress = "jane.doe@domain.com" };
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

			var expectedAccount1 = new ClientAccount { Id = 1, Name = "John Doe", EmailAddress = "john@domain.com" };
			var expectedAccount2 = new ClientAccount { Id = 2, Name = "Jane Doe", EmailAddress = "jane@domain.com" };

			TestHelper.AreEqual(existing.ToArray(), new[] { expectedAccount1, expectedAccount2 });
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

			TestHelper.AreEqual(expected, actual);
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

			TestHelper.AreEqual(expected, actual);
		}

		#endregion
	}
}