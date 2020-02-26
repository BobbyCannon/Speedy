#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Sync;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class SyncEntityTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void UpdateFromModelToEntity()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");
			var entity = GetTestEntity(date);
			var model = GetTestModel(date2);

			entity.UpdateWith(model, false, false, true);

			var expected = new AddressEntity
			{
				City = "City2",
				CreatedOn = date2,
				Id = 99,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal 2",
				State = "State 2",
				SyncId = Guid.Parse("511EB735-7CE7-4362-B36F-066CD697303A")
			};

			// SyncId changes but that's up to the user of the framework, should not exclude unless they want to
			TestHelper.AreEqual(expected, entity);
		}

		[TestMethod]
		public void UpdateLocalSyncIds()
		{
			var address = new AddressEntity { Id = 1, SyncId = Guid.NewGuid() };
			var person = new AccountEntity { Name = "Bob", Address = address, AddressId = address.Id };

			person.UpdateLocalSyncIds();

			Assert.AreEqual(address.SyncId, person.AddressSyncId);
		}

		[TestMethod]
		public void UpdateWith()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");
			var entity = GetTestEntity(date);
			var model = GetTestModel(date2);

			entity.UpdateWith(model);

			var expected = new AddressEntity
			{
				City = "City2",
				CreatedOn = date2,
				Id = 100,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal 2",
				State = "State 2",
				SyncId = Guid.Parse("511EB735-7CE7-4362-B36F-066CD697303A")
			};

			// We expecting all members to change except virtual members
			TestHelper.AreEqual(expected, entity);
		}

		[TestMethod]
		public void UpdateWithAllowVirtual()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");
			var entity = GetTestEntity(date);
			var model = GetTestModel(date2);

			model.Accounts = null;
			entity.UpdateWith(model, false);

			var expected = new AddressEntity
			{
				Accounts = null,
				City = "City2",
				CreatedOn = date2,
				Id = 100,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal 2",
				State = "State 2",
				SyncId = Guid.Parse("511EB735-7CE7-4362-B36F-066CD697303A")
			};

			// We expecting all members, *including* virtual members!
			TestHelper.AreEqual(expected, entity);
		}

		[TestMethod]
		public void UpdateWithOnly()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");
			var entity = GetTestEntity(date);
			var model = GetTestModel(date2);

			entity.UpdateWithOnly(model, nameof(AddressEntity.Line1), nameof(AddressEntity.Postal));

			var expected = new AddressEntity
			{
				City = "City",
				CreatedOn = date,
				Id = 99,
				Line1 = "Line 1",
				Line2 = "Line2",
				ModifiedOn = date,
				Postal = "Postal 2",
				State = "State",
				SyncId = Guid.Parse("3584456b-cf36-4049-9491-7d83d0fd8255")
			};

			// We expecting all members to change except virtual members
			TestHelper.AreEqual(expected, entity);
		}

		[TestMethod]
		public void UpdateWithSpecificMembers()
		{
			var date = DateTime.Parse("7/1/2019 05:18:30 PM");
			var date2 = DateTime.Parse("7/1/2019 05:18:31 PM");
			var entity = GetTestEntity(date);
			var model = GetTestModel(date2);

			model.Accounts = null;
			entity.UpdateWith(model, false, nameof(AddressEntity.City), nameof(AddressEntity.Postal));

			var expected = new AddressEntity
			{
				Accounts = null,
				City = "City",
				CreatedOn = date2,
				Id = 100,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal",
				State = "State 2",
				SyncId = Guid.Parse("511EB735-7CE7-4362-B36F-066CD697303A")
			};

			// We expecting all members, *including* virtual members!
			TestHelper.AreEqual(expected, entity);
		}

		[TestMethod]
		public void UpdateWithWithExclusions()
		{
			var expectedGuid = Guid.Parse("D3773475-D395-40E1-A230-266A845CB21D");
			var address = new AddressEntity { Id = 1, SyncId = expectedGuid };
			var expected = new AddressEntity();
			var properties = typeof(AddressEntity).GetCachedProperties().Select(x => x.Name).ToList();

			// All properties excluded
			var actual = new AddressEntity();
			actual.UpdateWith(address, properties.ToArray());
			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(0, actual.Id);
			Assert.AreEqual(Guid.Empty, actual.SyncId);

			// Remove Id exclusions
			properties = properties.Except(new[] { nameof(AddressEntity.Id) }).ToList();
			expected.Id = 1;
			actual = new AddressEntity();
			actual.UpdateWith(address, properties.ToArray());
			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual(Guid.Empty, actual.SyncId);

			// Remove SyncId exclusions
			properties = properties.Except(new[] { nameof(AddressEntity.SyncId) }).ToList();
			expected.SyncId = address.SyncId;
			actual = new AddressEntity();
			actual.UpdateWith(address, properties.ToArray());
			TestHelper.AreEqual(expected, actual);
			Assert.AreEqual(1, actual.Id);
			Assert.AreEqual(expectedGuid, actual.SyncId);
		}

		[TestMethod]
		public void VirtualPropertyNames()
		{
			var itemsToTest = new Dictionary<Type, string[]>
			{
				{ typeof(AddressEntity), new[] { nameof(AddressEntity.Accounts), nameof(AddressEntity.LinkedAddress), nameof(AddressEntity.LinkedAddresses) } },
				{ typeof(AccountEntity), new[] { nameof(AccountEntity.Address), nameof(AccountEntity.Groups), nameof(AccountEntity.Pets) } },
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
				Accounts = new List<AccountEntity>
				{
					new AccountEntity { Name = "John" }
				}
			};

			var actual = address.ToSyncObject();
			var expect = "{\"$id\":\"1\",\"City\":\"City\",\"CreatedOn\":\"2017-01-01T01:02:03\",\"Id\":2,\"IsDeleted\":false,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2017-02-02T01:02:03\",\"Postal\":\"29640\",\"State\":\"SC\",\"SyncId\":\"513b9cf1-7596-4e2e-888d-835622a3fb2b\"}";

			actual.Data.FormatDump();

			Assert.AreEqual(expect, actual.Data);
		}

		private static AddressEntity GetTestEntity(DateTime date)
		{
			return new AddressEntity
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
		}

		private static AddressEntity GetTestModel(DateTime date2)
		{
			return new AddressEntity
			{
				Accounts = new List<AccountEntity>(),
				City = "City2",
				CreatedOn = date2,
				Id = 100,
				Line1 = "Line 1",
				Line2 = "Line 2",
				ModifiedOn = date2,
				Postal = "Postal 2",
				State = "State 2",
				SyncId = Guid.Parse("511EB735-7CE7-4362-B36F-066CD697303A")
			};
		}

		#endregion
	}
}