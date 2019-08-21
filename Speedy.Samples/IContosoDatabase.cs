#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : ISyncableDatabase
	{
		#region Properties

		IRepository<AddressEntity, long> Addresses { get; }
		IRepository<FoodEntity, int> Food { get; }
		IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		IRepository<GroupMemberEntity, int> GroupMembers { get; }
		IRepository<GroupEntity, int> Groups { get; }
		IRepository<LogEventEntity, string> LogEvents { get; }
		IRepository<PersonEntity, int> People { get; }
		IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		IRepository<PetTypeEntity, string> PetTypes { get; }

		#endregion
	}
}