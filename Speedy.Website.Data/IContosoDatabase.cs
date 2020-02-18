#region References

using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples
{
	public interface IContosoDatabase : ISyncableDatabase
	{
		#region Properties

		IRepository<AddressEntity, long> Addresses { get; }
		IRepository<FoodEntity, int> Food { get; }
		IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		IRepository<GroupMemberEntity, int> GroupMembers { get; }
		IRepository<GroupEntity, int> Groups { get; }
		IRepository<LogEventEntity, long> LogEvents { get; }
		IRepository<AccountEntity, int> Accounts { get; }
		IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		IRepository<PetTypeEntity, string> PetTypes { get; }
		IRepository<SettingEntity, long> Settings { get; }

		#endregion
	}
}