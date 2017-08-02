#region References

using System;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : ISyncableDatabase
	{
		#region Properties

		IRepository<Speedy.Samples.Entities.Address, int> Addresses { get; }
		IRepository<Speedy.Samples.Entities.Food, int> Food { get; }
		IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships { get; }
		IRepository<Speedy.Samples.Entities.Group, int> Groups { get; }
		IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers { get; }
		IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents { get; }
		IRepository<Speedy.Samples.Entities.Person, int> People { get; }
		IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets { get; }
		IRepository<Speedy.Samples.Entities.PetType, string> PetTypes { get; }

		#endregion
	}
}