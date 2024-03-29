#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.SyncApi;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.UnitTests.Factories
{
	[ExcludeFromCodeCoverage]
	public static class EntityFactory
	{
		#region Methods

		public static AccountEntity GetAccount(Action<AccountEntity> update = null, string name = null, AddressEntity address = null)
		{
			var time = TimeService.UtcNow;
			var personAddress = address ?? GetAddress();

			var result = new AccountEntity
			{
				Address = personAddress,
				AddressSyncId = personAddress.SyncId,
				Id = default,
				Name = name ?? Guid.NewGuid().ToString(),
				Nickname = null,
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static AddressEntity GetAddress(Action<AddressEntity> update = null, string line1 = null, string postal = null, string state = null)
		{
			var time = TimeService.UtcNow;
			var result = new AddressEntity
			{
				City = "City",
				Id = default,
				Line1 = line1 ?? "Line1",
				Line2 = "Line2",
				LinkedAddressId = null,
				LinkedAddressSyncId = null,
				Postal = postal ?? "12345",
				State = state ?? "SC",
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static FoodEntity GetFood(Action<FoodEntity> update = null)
		{
			var result = new FoodEntity
			{
				Id = default,
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static FoodRelationshipEntity GetFoodRelationship(Action<FoodRelationshipEntity> update = null)
		{
			var result = new FoodRelationshipEntity
			{
				Child = GetFood(),
				Id = default,
				Parent = GetFood(),
				Quantity = default
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static GroupEntity GetGroup(Action<GroupEntity> update = null)
		{
			var time = TimeService.UtcNow;
			var result = new GroupEntity
			{
				Description = Guid.NewGuid().ToString(),
				Id = default,
				Name = Guid.NewGuid().ToString(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static GroupMemberEntity GetGroupMember(GroupEntity group, AccountEntity account, Action<GroupMemberEntity> update = null)
		{
			var time = TimeService.UtcNow;

			var result = new GroupMemberEntity
			{
				Group = group,
				GroupId = group.Id,
				Id = default,
				Member = account,
				MemberSyncId = account.SyncId,
				Role = Guid.NewGuid().ToString(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static LogEventEntity GetLogEvent(string message, LogLevel? level = null, Action<LogEventEntity> update = null)
		{
			var time = TimeService.UtcNow;
			var result = new LogEventEntity
			{
				AcknowledgedOn = null,
				LoggedOn = TimeService.UtcNow,
				Message = message,
				Level = level ?? LogLevel.Information,
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static PetEntity GetPet(Action<PetEntity> update = null, AccountEntity account = null)
		{
			var time = TimeService.UtcNow;
			var petPerson = account ?? GetAccount(null, "John");

			var result = new PetEntity
			{
				Name = Guid.NewGuid().ToString(),
				Owner = petPerson,
				Type = GetPetType(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static PetTypeEntity GetPetType(Action<PetTypeEntity> update = null)
		{
			var result = new PetTypeEntity
			{
				Id = Guid.NewGuid().ToString().Substring(0, 25),
				Type = null
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static SettingEntity GetSetting(string name, string value)
		{
			return new SettingEntity
			{
				Name = name,
				Value = value
			};
		}

		#endregion
	}
}