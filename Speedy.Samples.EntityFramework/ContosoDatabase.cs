#region References

using System;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;
using Speedy;

#endregion

namespace Speedy.Samples.EntityFramework
{
	public class ContosoDatabase : Speedy.EntityFramework.EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public ContosoDatabase()
			: this("name=ContosoDatabaseConnection")
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoDatabase(DatabaseOptions options)
			: this("name=ContosoDatabaseConnection", options)
		{
		}

		public ContosoDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
		}

		#endregion

		#region Properties

		public Speedy.IRepository<Speedy.Samples.Entities.Address, int> Addresses => GetRepository<Speedy.Samples.Entities.Address, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.Food, int> Foods => GetRepository<Speedy.Samples.Entities.Food, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships => GetRepository<Speedy.Samples.Entities.FoodRelationship, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.Group, int> Groups => GetRepository<Speedy.Samples.Entities.Group, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers => GetRepository<Speedy.Samples.Entities.GroupMember, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents => GetRepository<Speedy.Samples.Entities.LogEvent, string>();
		public Speedy.IRepository<Speedy.Samples.Entities.Person, int> People => GetRepository<Speedy.Samples.Entities.Person, int>();
		public Speedy.IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets => GetRepository<Speedy.Samples.Entities.Pet, Pet.PetKey>();
		public Speedy.IRepository<Speedy.Samples.Entities.PetType, string> PetTypes => GetRepository<Speedy.Samples.Entities.PetType, string>();

		#endregion
	}
}