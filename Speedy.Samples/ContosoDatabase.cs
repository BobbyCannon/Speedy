#region References

using System;
using System.Linq.Expressions;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples
{
	[Serializable]
	public class ContosoDatabase : Database, IContosoDatabase
	{
		#region Fields

		public static readonly string[] SyncOrder;

		#endregion

		#region Constructors

		public ContosoDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Addresses = GetSyncableRepository<Address>();
			Foods = GetRepository<Food>();
			FoodRelationships = GetRepository<FoodRelationship>();
			GroupMembers = GetSyncableRepository<GroupMember>();
			Groups = GetSyncableRepository<Group>();
			LogEvents = GetRepository<LogEvent>();
			People = GetSyncableRepository<Person>();
			SyncTombstones = GetSyncTombstonesRepository<SyncTombstone>();

			// Setup options.
			Options.SyncOrder = SyncOrder;

			// Address Map
			Property<Address>(x => x.Line1).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.Line2).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.City).IsRequired().HasMaximumLength(256);
			Property<Address>(x => x.State).IsRequired().HasMaximumLength(128);
			Property<Address>(x => x.Postal).IsRequired().HasMaximumLength(128);

			// Food Map
			Property<Food>(x => x.Name).IsRequired().HasMaximumLength(256);

			// Food Relationship Map
			Property<FoodRelationship>(x => x.Quantity).IsRequired();
			HasMany<FoodRelationship, Food>(x => x.Parent, x => x.ParentId, x => x.Children);
			HasMany<FoodRelationship, Food>(x => x.Child, x => x.ChildId, x => x.Parents);
			Property<FoodRelationship>(x => x.ParentId).IsRequired();
			Property<FoodRelationship>(x => x.ChildId).IsRequired();

			// Group Member Map
			HasMany<GroupMember, Person>(p => p.Member, p => p.MemberId, a => a.Groups);
			HasMany<GroupMember, Group>(p => p.Group, p => p.GroupId, a => a.Members);

			// LogEvent Map
			Property<LogEvent>(x => x.Message).IsRequired().HasMaximumLength(900);

			// Person Map
			Property<Person>(x => x.Name).IsRequired().HasMaximumLength(256);
			HasMany<Person, Address>(p => p.Address, p => p.AddressId, a => a.People);
		}

		static ContosoDatabase()
		{
			SyncOrder = new[]
			{
				typeof(Address).FullName,
				typeof(Person).FullName,
				typeof(Group).FullName,
				typeof(GroupMember).FullName
			};
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses { get; }

		public IRepository<FoodRelationship> FoodRelationships { get; }

		public IRepository<Food> Foods { get; }

		public IRepository<GroupMember> GroupMembers { get; }

		public IRepository<Group> Groups { get; }

		public IRepository<LogEvent> LogEvents { get; }

		public IRepository<Person> People { get; }

		public IRepository<SyncTombstone> SyncTombstones { get; }

		#endregion
	}
}