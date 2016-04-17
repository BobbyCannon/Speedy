#region References

using System;
using Speedy.Samples.Entities;
using Speedy.Sync;

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
			Addresses = GetSyncableRepository<Address>();
			Foods = GetRepository<Food>();
			FoodRelationships = GetRepository<FoodRelationship>();
			LogEvents = GetRepository<LogEvent>();
			People = GetSyncableRepository<Person>();
			SyncTombstones = GetSyncTombstonesRepository<SyncTombstone>();

			// Address Map
			Property<Address>(x => x.Line1).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.Line2).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.City).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.State).IsRequired().HasMaximumLength(128);
			Property<Address>(x => x.Postal).IsRequired().HasMaximumLength(128);

			// Food Map
			Property<Food>(x => x.Name).IsRequired().HasMaximumLength(256);

			// Food Relationship 
			Property<FoodRelationship>(x => x.Quantity).IsRequired();
			HasMany<FoodRelationship, Food>(x => x.Parent, x => x.ParentId, x => x.Children);
			HasMany<FoodRelationship, Food>(x => x.Child, x => x.ChildId, x => x.Parents);
			Property<FoodRelationship>(x => x.ParentId).IsRequired();
			Property<FoodRelationship>(x => x.ChildId).IsRequired();

			// LogEvent Map
			Property<LogEvent>(x => x.Message).IsRequired().HasMaximumLength(900);

			// Person Map
			Property<Person>(x => x.Name).IsRequired().HasMaximumLength(256);
			HasMany<Person, Address>(p => p.Address, p => p.AddressId, a => a.People);
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses { get; }

		public IRepository<FoodRelationship> FoodRelationships { get; }

		public IRepository<Food> Foods { get; }

		public IRepository<LogEvent> LogEvents { get; }

		public IRepository<Person> People { get; }

		public IRepository<SyncTombstone> SyncTombstones { get; }

		#endregion
	}
}