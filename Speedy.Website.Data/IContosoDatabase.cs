#region References

using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Data
{
	public interface IContosoDatabase : ISyncableDatabase
	{
		#region Properties

		ISyncableRepository<AccountEntity, int> Accounts { get; }
		ISyncableRepository<AddressEntity, long> Addresses { get; }
		IRepository<FoodEntity, int> Food { get; }
		IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		IRepository<GroupMemberEntity, int> GroupMembers { get; }
		IRepository<GroupEntity, int> Groups { get; }
		ISyncableRepository<LogEventEntity, long> LogEvents { get; }
		IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		IRepository<PetTypeEntity, string> PetTypes { get; }
		ISyncableRepository<SettingEntity, long> Settings { get; }
		IRepository<TrackerPathEntity, long> TrackerPaths { get; }
		IRepository<TrackerPathConfigurationEntity, int> TrackerPathConfigurations { get; }

		#endregion
	}
}