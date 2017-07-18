#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : ISyncableDatabase
	{
		#region Properties

		IRepository<Address, int> Addresses { get; }
		IRepository<FoodRelationship, int> FoodRelationships { get; }
		IRepository<Food, int> Foods { get; }
		IRepository<GroupMember, int> GroupMembers { get; }
		IRepository<Group, int> Groups { get; }
		IRepository<LogEvent, string> LogEvents { get; }
		IRepository<Person, int> People { get; }
		IRepository<Pet, Pet.PetKey> Pets { get; }
		IRepository<PetType, string> PetTypes { get; }

		#endregion
	}
}