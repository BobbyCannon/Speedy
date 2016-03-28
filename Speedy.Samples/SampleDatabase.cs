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

			HasRequired<Address>(x => x.Line1);
			HasMany<Address, Person>(p => p.Address, p => p.AddressId);
			HasMany<Food, FoodRelationship>(x => x.Parent, x => x.ParentId, "FoodChildren");
			HasMany<Food, FoodRelationship>(x => x.Child, x => x.ChildId, "FoodParents");
			HasRequired<FoodRelationship>(x => x.ParentId);
			HasRequired<FoodRelationship>(x => x.ChildId);
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