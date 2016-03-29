#region References

using System;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	[Serializable]
	public class SampleDatabase : Database, ISampleDatabase
	{
		#region Constructors

		public SampleDatabase(string directory = null)
			: base(directory)
		{
			Addresses = GetRepository<Address>();
			Foods = GetRepository<Food>();
			FoodRelationships = GetRepository<FoodRelationship>();
			People = GetRepository<Person>();

			// Address Map
			Property<Address>(x => x.Line1).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.Line2).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.City).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.State).IsRequired().HasMaxLength(128);
			Property<Address>(x => x.Postal).IsRequired().HasMaxLength(128);

			// Person Map
			Property<Person>(x => x.Name).IsRequired().HasMaxLength(256);
			HasMany<Person, Address>(p => p.Address, p => p.AddressId);

			// Food Map
			Property<Food>(x => x.Name).IsRequired().HasMaxLength(256);
			
			// Food Relationship 
			Property<FoodRelationship>(x => x.Quantity).IsRequired();
			HasMany<FoodRelationship, Food>(x => x.Parent, x => x.ParentId, "FoodChildren");
			HasMany<FoodRelationship, Food>(x => x.Child, x => x.ChildId, "FoodParents");
			Property<FoodRelationship>(x => x.ParentId);
			Property<FoodRelationship>(x => x.ChildId);
		}

		#endregion

		#region Properties

		public IEntityRepository<Address> Addresses { get; }

		public IEntityRepository<FoodRelationship> FoodRelationships { get; }

		public IEntityRepository<Food> Foods { get; }

		public IEntityRepository<Person> People { get; }

		#endregion
	}
}