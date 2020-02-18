#region References

using Speedy.EntityFramework;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(DatabaseOptions options = null) : base(options)
		{
			Accounts = GetSyncableRepository<AccountEntity, int>();
			Addresses = GetSyncableRepository<AddressEntity, long>();
			Food = GetRepository<FoodEntity, int>();
			FoodRelationships = GetRepository<FoodRelationshipEntity, int>();
			Groups = GetRepository<GroupEntity, int>();
			GroupMembers = GetRepository<GroupMemberEntity, int>();
			LogEvents = GetSyncableRepository<LogEventEntity, long>();
			Pets = GetRepository<PetEntity, (string Name, int OwnerId)>();
			PetTypes = GetRepository<PetTypeEntity, string>();
			Settings = GetSyncableRepository<SettingEntity, long>();

			ContosoDatabase.SetRequiredOptions(Options);

			this.ConfigureModelViaMapping();
		}

		#endregion

		#region Properties

		public IRepository<AccountEntity, int> Accounts { get; }
		public IRepository<AddressEntity, long> Addresses { get; }
		public IRepository<FoodEntity, int> Food { get; }
		public IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		public IRepository<GroupMemberEntity, int> GroupMembers { get; }
		public IRepository<GroupEntity, int> Groups { get; }
		public IRepository<LogEventEntity, long> LogEvents { get; }
		public IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		public IRepository<PetTypeEntity, string> PetTypes { get; }
		public IRepository<SettingEntity, long> Settings { get; }

		#endregion
	}
}