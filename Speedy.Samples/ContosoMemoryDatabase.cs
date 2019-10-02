#region References

using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(DatabaseOptions options = null) : base(options)
		{
			Options.SyncOrder = new[] { typeof(AddressEntity).ToAssemblyName(), typeof(PersonEntity).ToAssemblyName() };

			Addresses = GetSyncableRepository<AddressEntity, long>();
			Food = GetRepository<FoodEntity, int>();
			FoodRelationships = GetRepository<FoodRelationshipEntity, int>();
			Groups = GetRepository<GroupEntity, int>();
			GroupMembers = GetRepository<GroupMemberEntity, int>();
			LogEvents = GetRepository<LogEventEntity, string>();
			People = GetSyncableRepository<PersonEntity, int>();
			Pets = GetRepository<PetEntity, (string Name, int OwnerId)>();
			PetTypes = GetRepository<PetTypeEntity, string>();
			Settings = GetSyncableRepository<SettingEntity, long>();

			this.ConfigureModelViaMapping();
		}

		#endregion

		#region Properties

		public IRepository<AddressEntity, long> Addresses { get; }
		public IRepository<FoodEntity, int> Food { get; }
		public IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		public IRepository<GroupMemberEntity, int> GroupMembers { get; }
		public IRepository<GroupEntity, int> Groups { get; }
		public IRepository<LogEventEntity, string> LogEvents { get; }
		public IRepository<PersonEntity, int> People { get; }
		public IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		public IRepository<PetTypeEntity, string> PetTypes { get; }
		public IRepository<SettingEntity, long> Settings { get; }

		#endregion
	}
}