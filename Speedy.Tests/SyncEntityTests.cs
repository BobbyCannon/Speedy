#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class SyncEntityTests
	{
		#region Methods

		[TestMethod]
		public void UpdateLocalSyncIds()
		{
			var address = new Address { Id = 1, SyncId = Guid.NewGuid() };
			var person = new Person { Name = "Bob", Address = address, AddressId = address.Id };

			person.UpdateLocalSyncIds();

			Assert.AreEqual(address.SyncId, person.AddressSyncId);
		}

		[TestMethod]
		public void VirtualPropertyNames()
		{
			var itemsToTest = new Dictionary<Type, string[]>
			{
				{ typeof(Address), new[] { nameof(Address.BillingPeople), nameof(Address.LinkedAddress), nameof(Address.LinkedAddresses), nameof(Address.People) } },
				{ typeof(Person), new[] { nameof(Person.Address), nameof(Person.BillingAddress), nameof(Person.Groups), nameof(Person.Owners) } },
				{ typeof(Group), new[] { nameof(Group.Members) } },
				{ typeof(SyncRequest), new string[0] }
			};

			foreach (var item in itemsToTest)
			{
				var actual = item.Key.GetVirtualPropertyNames().ToArray();
				var expect = item.Value;

				TestHelper.AreEqual(expect, actual);
			}
		}

		[TestMethod]
		public void VirtualPropertyTest1()
		{
			var address = new Address
			{
				City = "City",
				CreatedOn = new DateTime(2017, 01, 01, 01, 02, 03),
				Id = 2,
				Line1 = "Line1",
				Line2 = "Line2",
				ModifiedOn = new DateTime(2017, 02, 02, 01, 02, 03),
				SyncId = Guid.Parse("513B9CF1-7596-4E2E-888D-835622A3FB2B"),
				Postal = "296540",
				State = "SC"
			};

			var actual = address.ToSyncObject();
			var expect = "{\"$id\":\"1\",\"City\":\"City\",\"FullAddress\":\"Line1\\r\\nCity, SC  296540\",\"Id\":2,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"Postal\":\"296540\",\"State\":\"SC\",\"SyncId\":\"513b9cf1-7596-4e2e-888d-835622a3fb2b\",\"ModifiedOn\":\"2017-02-02T01:02:03\",\"CreatedOn\":\"2017-01-01T01:02:03\"}";

			actual.Data.FormatDump();

			Assert.AreEqual(expect, actual.Data);
		}

		#endregion
	}
}