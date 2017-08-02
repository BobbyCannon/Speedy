#region References

using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework
{
	public class ContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public ContosoDatabase()
			: this("name=DefaultConnection")
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoDatabase(DatabaseOptions options)
			: this("name=DefaultConnection", options)
		{
		}

		public ContosoDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
			Options.SyncOrder = new[]
			{
				typeof(Address).FullName,
				typeof(Person).FullName
			};
		}

		#endregion

		#region Properties

		public IRepository<Address, int> Addresses => GetSyncableRepository<Address>();
		public IRepository<Food, int> Food => GetRepository<Food, int>();
		public IRepository<FoodRelationship, int> FoodRelationships => GetRepository<FoodRelationship, int>();
		public IRepository<GroupMember, int> GroupMembers => GetRepository<GroupMember, int>();
		public IRepository<Group, int> Groups => GetRepository<Group, int>();
		public IRepository<LogEvent, string> LogEvents => GetRepository<LogEvent, string>();
		public IRepository<Person, int> People => GetSyncableRepository<Person>();
		public IRepository<Pet, Pet.PetKey> Pets => GetRepository<Pet, Pet.PetKey>();
		public IRepository<PetType, string> PetTypes => GetRepository<PetType, string>();

		#endregion
	}
}