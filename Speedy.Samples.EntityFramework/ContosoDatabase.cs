#region References

using System;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;
using Speedy.Samples.Mappings;
using Speedy.Sync;
using Speedy;

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

		public IRepository<Speedy.Samples.Entities.Address, int> Addresses => GetSyncableRepository<Speedy.Samples.Entities.Address>();
		public IRepository<Speedy.Samples.Entities.Food, int> Food => GetRepository<Speedy.Samples.Entities.Food, int>();
		public IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships => GetRepository<Speedy.Samples.Entities.FoodRelationship, int>();
		public IRepository<Speedy.Samples.Entities.Group, int> Groups => GetRepository<Speedy.Samples.Entities.Group, int>();
		public IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers => GetRepository<Speedy.Samples.Entities.GroupMember, int>();
		public IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents => GetRepository<Speedy.Samples.Entities.LogEvent, string>();
		public IRepository<Speedy.Samples.Entities.Person, int> People => GetSyncableRepository<Speedy.Samples.Entities.Person>();
		public IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets => GetRepository<Speedy.Samples.Entities.Pet, Pet.PetKey>();
		public IRepository<Speedy.Samples.Entities.PetType, string> PetTypes => GetRepository<Speedy.Samples.Entities.PetType, string>();

		#endregion
	}
}