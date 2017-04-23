#region References

using System;
using Speedy.Samples.Entities;

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
			Addresses = GetRepository<Address, int>();
			Foods = GetRepository<Food, int>();
			FoodRelationships = GetRepository<FoodRelationship, int>();
			GroupMembers = GetRepository<GroupMember, int>();
			Groups = GetRepository<Group, int>();
			LogEvents = GetRepository<LogEvent, int>();
			People = GetRepository<Person, int>();
			Pets = GetRepository<Pet, Pet.PetKey>();

			// Setup options.
			Options.SyncOrder = SyncOrder;

			// Address Map
			Property<Address, int>(x => x.Line1).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.Line2).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.City).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.State).IsRequired().HasMaximumLength(128);
			Property<Address, int>(x => x.Postal).IsRequired().HasMaximumLength(128);
			HasOptional<Address, Address, int>(x => x.LinkedAddress, x => x.LinkedAddressId);

			// Food Map
			Property<Food, int>(x => x.Name).IsRequired().HasMaximumLength(256).IsUnique();

			// Food Relationship Map
			Property<FoodRelationship, int>(x => x.Quantity).IsRequired();
			HasRequired<FoodRelationship, Food, int>(x => x.Parent, x => x.ParentId, x => x.Children);
			HasRequired<FoodRelationship, Food, int>(x => x.Child, x => x.ChildId, x => x.Parents);
			Property<FoodRelationship, int>(x => x.ParentId).IsRequired();
			Property<FoodRelationship, int>(x => x.ChildId).IsRequired();

			// Group Member Map
			HasRequired<GroupMember, Person, int>(p => p.Member, p => p.MemberId, a => a.Groups);
			HasRequired<GroupMember, Group, int>(p => p.Group, p => p.GroupId, a => a.Members);

			// LogEvent Map
			Property<LogEvent, int>(x => x.Message).IsRequired().HasMaximumLength(900);

			// Person Map
			Property<Person, int>(x => x.Name).IsRequired().HasMaximumLength(256).IsUnique();
			HasRequired<Person, Address, int>(p => p.Address, p => p.AddressId, a => a.People);
			HasOptional<Person, Address, int>(p => p.BillingAddress, p => p.BillingAddressId);

			// Pet Map
			HasRequired<Pet, Pet.PetKey, Person, int>(x => x.Owner, x => x.OwnerId, x => x.Pets);
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

		public IRepository<Address, int> Addresses { get; }

		public IRepository<FoodRelationship, int> FoodRelationships { get; }

		public IRepository<Food, int> Foods { get; }

		public IRepository<GroupMember, int> GroupMembers { get; }

		public IRepository<Group, int> Groups { get; }

		public IRepository<LogEvent, int> LogEvents { get; }

		public IRepository<Person, int> People { get; }

		public IRepository<Pet, Pet.PetKey> Pets { get; }

		#endregion
	}
}