#region References

using Speedy.Samples.Entities;
using Speedy.Samples.Mappings.Memory;

#endregion

namespace Speedy.Samples
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Options.SyncOrder = new[] { typeof(Address).FullName, typeof(Person).FullName };

			Addresses = GetSyncableRepository<Address>();
			AddressMap.ConfigureDatabase(this);

			Food = GetRepository<Food, int>();
			FoodMap.ConfigureDatabase(this);

			FoodRelationships = GetRepository<FoodRelationship, int>();
			FoodRelationshipMap.ConfigureDatabase(this);

			Groups = GetRepository<Group, int>();
			GroupMap.ConfigureDatabase(this);

			GroupMembers = GetRepository<GroupMember, int>();
			GroupMemberMap.ConfigureDatabase(this);

			LogEvents = GetRepository<LogEvent, string>();
			LogEventMap.ConfigureDatabase(this);

			People = GetSyncableRepository<Person>();
			PersonMap.ConfigureDatabase(this);

			Pets = GetRepository<Pet, Pet.PetKey>();
			PetMap.ConfigureDatabase(this);

			PetTypes = GetRepository<PetType, string>();
			PetTypeMap.ConfigureDatabase(this);

			// Configuration for the sync tombstone
			SyncTombstoneMap.ConfigureDatabase(this);
		}

		#endregion

		#region Properties

		public IRepository<Address, int> Addresses { get; }
		public IRepository<Food, int> Food { get; }
		public IRepository<FoodRelationship, int> FoodRelationships { get; }
		public IRepository<GroupMember, int> GroupMembers { get; }
		public IRepository<Group, int> Groups { get; }
		public IRepository<LogEvent, string> LogEvents { get; }
		public IRepository<Person, int> People { get; }
		public IRepository<Pet, Pet.PetKey> Pets { get; }
		public IRepository<PetType, string> PetTypes { get; }

		#endregion
	}
}