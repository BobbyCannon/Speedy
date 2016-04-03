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

		public SampleDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Addresses = GetRepository<Address>();
			Foods = GetRepository<Food>();
			FoodRelationships = GetRepository<FoodRelationship>();
			LogEvents = GetRepository<LogEvent>();
			People = GetRepository<Person>();

			// Address Map
			Property<Address>(x => x.Line1).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.Line2).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.City).IsRequired().HasMaxLength(256);
			Property<Address>(x => x.State).IsRequired().HasMaxLength(128);
			Property<Address>(x => x.Postal).IsRequired().HasMaxLength(128);

			// Food Map
			Property<Food>(x => x.Name).IsRequired().HasMaxLength(256);

			// Food Relationship 
			Property<FoodRelationship>(x => x.Quantity).IsRequired();
			HasMany<FoodRelationship, Food>(x => x.Parent, x => x.ParentId, x => x.Children);
			HasMany<FoodRelationship, Food>(x => x.Child, x => x.ChildId, x => x.Parents);
			Property<FoodRelationship>(x => x.ParentId).IsRequired();
			Property<FoodRelationship>(x => x.ChildId).IsRequired();

			// LogEvent Map
			Property<LogEvent>(x => x.Message).IsRequired().HasMaxLength(900);

			// Person Map
			Property<Person>(x => x.Name).IsRequired().HasMaxLength(256);
			HasMany<Person, Address>(p => p.Address, p => p.AddressId, a => a.People);
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses { get; }

		public IRepository<FoodRelationship> FoodRelationships { get; }

		public IRepository<Food> Foods { get; }

		public IRepository<LogEvent> LogEvents { get; }

		public IRepository<Person> People { get; }

		#endregion
	}
}