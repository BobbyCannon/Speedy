#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Client.Samples.Models;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Tests
{
	[TestClass]
	public class SyncObjectTests
	{
		#region Methods

		[TestMethod]
		public void ConvertModelsToEntities()
		{
			var createdOn = new DateTime(2019, 01, 01, 02, 03, 04, DateTimeKind.Utc);
			var modifiedOn = new DateTime(2019, 02, 03, 04, 05, 06, DateTimeKind.Utc);

			var address = new Address
			{
				City = "City",
				Id = 99,
				Line1 = "Line1",
				Line2 = "Line2",
				Postal = "Postal",
				State = "State",
				SyncId = Guid.Parse("efc2c530-37b6-4fa5-ab71-bd38a3b4d277"),
				CreatedOn = createdOn,
				ModifiedOn = modifiedOn
			};

			var person = new Person
			{
				Address = address,
				AddressId = address.Id,
				AddressSyncId = address.SyncId,
				Id = 100,
				Name = "John",
				SyncId = Guid.Parse("7d880bb3-183f-4f75-86d8-9834ffe8fad2"),
				CreatedOn = createdOn,
				ModifiedOn = modifiedOn
			};

			var addressSyncObject = address.ToSyncObject();
			var personSyncObject = person.ToSyncObject();
			var changes = new[] { addressSyncObject, personSyncObject };
			var actual = SyncObjectConverter.Convert(changes, SyncConversionType.Converting, true, true,
					new SyncObjectConverter<Address, long, AddressEntity, long>(),
					new SyncObjectConverter<Person, int, PersonEntity, int>()
				)
				.ToList();

			var expectedAddress = "{\"$id\":\"1\",\"City\":\"City\",\"CreatedOn\":\"2019-01-01T02:03:04Z\",\"Id\":0,\"IsDeleted\":false,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2019-02-03T04:05:06Z\",\"Postal\":\"Postal\",\"State\":\"State\",\"SyncId\":\"efc2c530-37b6-4fa5-ab71-bd38a3b4d277\"}";
			var expectedPerson = "{\"$id\":\"1\",\"AddressId\":0,\"AddressSyncId\":\"efc2c530-37b6-4fa5-ab71-bd38a3b4d277\",\"CreatedOn\":\"2019-01-01T02:03:04Z\",\"Id\":0,\"IsDeleted\":false,\"ModifiedOn\":\"2019-02-03T04:05:06Z\",\"Name\":\"John\",\"SyncId\":\"7d880bb3-183f-4f75-86d8-9834ffe8fad2\"}";

			Assert.AreEqual(address.SyncId, actual[0].SyncId);
			Assert.AreEqual(expectedAddress, actual[0].Data);
			Assert.AreEqual(SyncObjectStatus.Modified, actual[0].Status);

			Assert.AreEqual(person.SyncId, actual[1].SyncId);
			Assert.AreEqual(expectedPerson, actual[1].Data);
			Assert.AreEqual(SyncObjectStatus.Modified, actual[1].Status);
		}

		#endregion
	}
}