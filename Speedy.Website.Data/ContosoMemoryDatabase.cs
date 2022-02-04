#region References

using Speedy.EntityFramework;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Data
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(DatabaseOptions options = null, DatabaseKeyCache keyCache = null) : base(options, keyCache)
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
			TrackerPaths = GetRepository<TrackerPathEntity, long>();
			TrackerPathConfigurations = GetRepository<TrackerPathConfigurationEntity, int>();

			ContosoDatabase.SetRequiredOptions(Options);

			this.ConfigureModelViaMapping();
		}

		#endregion

		#region Properties

		public ISyncableRepository<AccountEntity, int> Accounts { get; }
		public ISyncableRepository<AddressEntity, long> Addresses { get; }
		public IRepository<FoodEntity, int> Food { get; }
		public IRepository<FoodRelationshipEntity, int> FoodRelationships { get; }
		public IRepository<GroupMemberEntity, int> GroupMembers { get; }
		public IRepository<GroupEntity, int> Groups { get; }
		public ISyncableRepository<LogEventEntity, long> LogEvents { get; }
		public IRepository<PetEntity, (string Name, int OwnerId)> Pets { get; }
		public IRepository<PetTypeEntity, string> PetTypes { get; }
		public ISyncableRepository<SettingEntity, long> Settings { get; }
		public IRepository<TrackerPathEntity, long> TrackerPaths { get; }
		public IRepository<TrackerPathConfigurationEntity, int> TrackerPathConfigurations { get; }

		#endregion
	}
}