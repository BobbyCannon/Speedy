#region References

using System;
using Speedy.Samples.Entities;
using Speedy;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : Speedy.IDatabase
	{
		#region Properties

		Speedy.IRepository<Speedy.Samples.Entities.Address, int> Addresses { get; }
		Speedy.IRepository<Speedy.Samples.Entities.Food, int> Foods { get; }
		Speedy.IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships { get; }
		Speedy.IRepository<Speedy.Samples.Entities.Group, int> Groups { get; }
		Speedy.IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers { get; }
		Speedy.IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents { get; }
		Speedy.IRepository<Speedy.Samples.Entities.Person, int> People { get; }
		Speedy.IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets { get; }
		Speedy.IRepository<Speedy.Samples.Entities.PetType, string> PetTypes { get; }

		#endregion
	}
}