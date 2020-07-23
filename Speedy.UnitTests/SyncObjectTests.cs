#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Data.Client;
using Speedy.Sync;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.UnitTests
{
	[TestClass]
	public class SyncObjectTests : BaseTests
	{
		#region Methods

		[TestMethod]
		public void ConvertModelsToEntities()
		{
			var createdOn = new DateTime(2019, 01, 01, 02, 03, 04, DateTimeKind.Utc);
			var modifiedOn = new DateTime(2019, 02, 03, 04, 05, 06, DateTimeKind.Utc);

			var address = new ClientAddress
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

			var person = new ClientAccount
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
			var converter = new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<ClientAddress, long, AddressEntity, long>(),
				new SyncObjectIncomingConverter<ClientAccount, int, AccountEntity, int>()
			);
			
			var actual = converter.Convert(changes).ToList();
			var expectedAddress = "{\"$id\":\"1\",\"City\":\"City\",\"CreatedOn\":\"2019-01-01T02:03:04Z\",\"Id\":0,\"IsDeleted\":false,\"Line1\":\"Line1\",\"Line2\":\"Line2\",\"LinkedAddressId\":null,\"LinkedAddressSyncId\":null,\"ModifiedOn\":\"2019-02-03T04:05:06Z\",\"Postal\":\"Postal\",\"State\":\"State\",\"SyncId\":\"efc2c530-37b6-4fa5-ab71-bd38a3b4d277\"}";
			var expectedAccount = "{\"$id\":\"1\",\"AddressId\":0,\"AddressSyncId\":\"efc2c530-37b6-4fa5-ab71-bd38a3b4d277\",\"CreatedOn\":\"2019-01-01T02:03:04Z\",\"EmailAddress\":null,\"ExternalId\":null,\"Id\":0,\"IsDeleted\":false,\"LastLoginDate\":\"0001-01-01T00:00:00\",\"ModifiedOn\":\"2019-02-03T04:05:06Z\",\"Name\":\"John\",\"Nickname\":null,\"PasswordHash\":null,\"Roles\":null,\"SyncId\":\"7d880bb3-183f-4f75-86d8-9834ffe8fad2\"}";

			Assert.AreEqual(address.SyncId, actual[0].SyncId);
			Assert.AreEqual(expectedAddress, actual[0].Data);
			Assert.AreEqual(SyncObjectStatus.Modified, actual[0].Status);

			actual[1].Data.Dump();

			Assert.AreEqual(person.SyncId, actual[1].SyncId);
			Assert.AreEqual(expectedAccount, actual[1].Data);
			Assert.AreEqual(SyncObjectStatus.Modified, actual[1].Status);
		}

		#endregion
	}
}