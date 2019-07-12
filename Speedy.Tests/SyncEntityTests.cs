#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy.Website.Models;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class SyncEntityTests
	{
		#region Methods

		[TestMethod]
		public void ExcludePropertyForUpdate()
		{
			var source = new AddressEntity();
			var destination = new AddressEntity();

			source.Line1 = "Test";
			
			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);

			destination.UpdateWith(source, false);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual("Test", destination.Line1);
			
			source.Line1 = "Test";
			destination.Line1 = null;
			
			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);

			destination.ExcludePropertiesForUpdate(nameof(AddressEntity.Line1));
			destination.UpdateWith(source, false, true);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);
			
			destination.UpdateWith(source, false, false);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual("Test", destination.Line1);
		}
		
		[TestMethod]
		public void ExcludePropertyForSync()
		{
			var source = new AddressEntity();
			var destination = new AddressEntity();

			source.Line1 = "Test";
			
			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);

			destination.UpdateWith(source, false);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual("Test", destination.Line1);
			
			source.Line1 = "Test";
			destination.Line1 = null;
			
			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);

			destination.ExcludePropertiesForSync(nameof(AddressEntity.Line1));
			destination.UpdateWith(source, true, false);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual(null, destination.Line1);
			
			destination.UpdateWith(source, false, false);

			Assert.AreEqual("Test", source.Line1);
			Assert.AreEqual("Test", destination.Line1);
		}


		[TestMethod]
		public void UpdateFromModelToEntity()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");

			var entity = new AddressEntity
			{
				City = "City",
				CreatedOn = date,
				Id = 99,
				Line1 = "Line1",
				Line2 = "Line2",
				ModifiedOn = date,
				Postal = "Postal",
				State = "State",
				SyncId = Guid.Parse("3584456b-cf36-4049-9491-7d83d0fd8255")
			};

			var model = new Address
			{
				City = "City2",
				CreatedOn = date2,
				Id = 100,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal 2",
				State = "State",
				SyncId = Guid.Parse("3584456b-cf36-4049-9491-7d83d0fd8255")
			};

			entity.UpdateWith(model);

			Assert.AreEqual("City2", entity.City);
		}

		[TestMethod]
		public void UpdateLocalSyncIds()
		{
			var address = new AddressEntity { Id = 1, SyncId = Guid.NewGuid() };
			var person = new PersonEntity { Name = "Bob", Address = address, AddressId = address.Id };

			person.UpdateLocalSyncIds();

			Assert.AreEqual(address.SyncId, person.AddressSyncId);
		}

		[TestMethod]
		public void ResetExclusionsForUpdate()
		{
			var entity = new AddressEntity();
			
			// Defaults and one that should not be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.City)));

			// Add exclusion
			entity.ExcludePropertiesForUpdate(nameof(AddressEntity.City));
			
			// Defaults and one that should now be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.Id)));
			Assert.IsTrue(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.City)));
			
			// Reset exclusion to default
			entity.ResetPropertyUpdateExclusions();
			
			// Defaults and one that should now be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.City)));
			
			// Reset exclusion completely
			entity.ResetPropertyUpdateExclusions(false);
			
			// Defaults and one that should now be excluded
			Assert.IsFalse(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForUpdate(nameof(AddressEntity.City)));
		}
		
		[TestMethod]
		public void ResetExclusionsForSync()
		{
			var entity = new AddressEntity();
			
			// Defaults and one that should not be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForSync(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForSync(nameof(AddressEntity.City)));

			// Add exclusion
			entity.ExcludePropertiesForSync(nameof(AddressEntity.City));
			
			// Defaults and one that should now be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForSync(nameof(AddressEntity.Id)));
			Assert.IsTrue(entity.IsPropertyExcludedForSync(nameof(AddressEntity.City)));
			
			// Reset exclusion to default
			entity.ResetPropertySyncExclusions();
			
			// Defaults and one that should now be excluded
			Assert.IsTrue(entity.IsPropertyExcludedForSync(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForSync(nameof(AddressEntity.City)));
			
			// Reset exclusion completely
			entity.ResetPropertySyncExclusions(false);
			
			// Defaults and one that should now be excluded
			Assert.IsFalse(entity.IsPropertyExcludedForSync(nameof(AddressEntity.Id)));
			Assert.IsFalse(entity.IsPropertyExcludedForSync(nameof(AddressEntity.City)));
		}

		[TestMethod]
		public void VirtualPropertyNames()
		{
			var itemsToTest = new Dictionary<Type, string[]>
			{
				{ typeof(AddressEntity), new[] { nameof(AddressEntity.LinkedAddress), nameof(AddressEntity.LinkedAddresses), nameof(AddressEntity.People) } },
				{ typeof(PersonEntity), new[] { nameof(PersonEntity.Address), nameof(PersonEntity.BillingAddress), nameof(PersonEntity.Groups), nameof(PersonEntity.Owners) } },
				{ typeof(GroupEntity), new[] { nameof(GroupEntity.Members) } },
				{ typeof(SyncRequest), new[] { nameof(SyncRequest.HasChanges) } }
			};

			foreach (var item in itemsToTest)
			{
				var actual = item.Key.GetVirtualPropertyNames().ToArray();
				var expect = item.Value;

				item.Key.FullName.Dump();
				(string.Join(",", expect) + " != " + string.Join(",", actual)).Dump();

				TestHelper.AreEqual(expect, actual);
			}
		}

		[TestMethod]
		public void VirtualPropertyTest1()
		{
			var address = new AddressEntity
			{
				City = "City",
				CreatedOn = new DateTime(2017, 01, 01, 01, 02, 03),
				Id = 2,
				Line1 = "Line1",
				Line2 = "Line2",
				ModifiedOn = new DateTime(2017, 02, 02, 01, 02, 03),
				SyncId = Guid.Parse("513B9CF1-7596-4E2E-888D-835622A3FB2B"),
				Postal = "29640",
				State = "SC",
				People = new List<PersonEntity>
				{
					new PersonEntity { Name = "John" }
				}
			};

			var actual = address.ToSyncObject();
			var expect = "{\"$id\":\"1\",\"City\":\"City\",\"CreatedOn\":\"2017-01-01T01:02:03\",\"Id\":2,\"IsDeleted\":false,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2017-02-02T01:02:03\",\"Postal\":\"29640\",\"State\":\"SC\",\"SyncId\":\"513b9cf1-7596-4e2e-888d-835622a3fb2b\"}";

			actual.Data.FormatDump();

			Assert.AreEqual(expect, actual.Data);
		}

		#endregion
	}
}