#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : IDatabase
	{
		#region Properties

		IRepository<Address, int> Addresses { get; }

		IRepository<FoodRelationship, int> FoodRelationships { get; }

		IRepository<Food, int> Foods { get; }

		IRepository<GroupMember, int> GroupMembers { get; }

		IRepository<Group, int> Groups { get; }

		IRepository<LogEvent, int> LogEvents { get; }

		IRepository<Person, int> People { get; }

		IRepository<Pet, Pet.PetKey> Pets { get; }

		#endregion
	}
}