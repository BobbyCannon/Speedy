#region References

using System;
using Speedy.Samples.Entities;
using Speedy.Samples.Mappings;
using Speedy.Sync;
using Speedy;

#endregion

namespace Speedy.Samples
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Addresses = GetSyncableRepository<Speedy.Samples.Entities.Address>();
			AddressMap.ConfigureDatabase(this);

			Food = GetRepository<Speedy.Samples.Entities.Food, int>();
			FoodMap.ConfigureDatabase(this);

			FoodRelationships = GetRepository<Speedy.Samples.Entities.FoodRelationship, int>();
			FoodRelationshipMap.ConfigureDatabase(this);

			Groups = GetRepository<Speedy.Samples.Entities.Group, int>();
			GroupMap.ConfigureDatabase(this);

			GroupMembers = GetRepository<Speedy.Samples.Entities.GroupMember, int>();
			GroupMemberMap.ConfigureDatabase(this);

			LogEvents = GetRepository<Speedy.Samples.Entities.LogEvent, string>();
			LogEventMap.ConfigureDatabase(this);

			People = GetSyncableRepository<Speedy.Samples.Entities.Person>();
			PersonMap.ConfigureDatabase(this);

			Pets = GetRepository<Speedy.Samples.Entities.Pet, Pet.PetKey>();
			PetMap.ConfigureDatabase(this);

			PetTypes = GetRepository<Speedy.Samples.Entities.PetType, string>();
			PetTypeMap.ConfigureDatabase(this);

			// Configuration for the sync tombstone
			SyncTombstoneMap.ConfigureDatabase(this);
		}

		#endregion

		#region Properties

		public IRepository<Speedy.Samples.Entities.Address, int> Addresses { get; }
		public IRepository<Speedy.Samples.Entities.Food, int> Food { get; }
		public IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships { get; }
		public IRepository<Speedy.Samples.Entities.Group, int> Groups { get; }
		public IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers { get; }
		public IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents { get; }
		public IRepository<Speedy.Samples.Entities.Person, int> People { get; }
		public IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets { get; }
		public IRepository<Speedy.Samples.Entities.PetType, string> PetTypes { get; }

		#endregion
	}
}