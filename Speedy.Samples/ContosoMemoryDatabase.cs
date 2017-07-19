#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public class ContosoMemoryDatabase : Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Options.SyncOrder = new[]
			{
				typeof(Address).FullName,
				typeof(Person).FullName
			};

			Addresses = GetSyncableRepository<Address>();
			Property<Address, int>(x => x.Id).IsRequired().IsUnique();
			Property<Address, int>(x => x.City).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.Line1).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.Line2).IsRequired().HasMaximumLength(256);
			Property<Address, int>(x => x.LinkedAddressId).IsOptional();
			Property<Address, int>(x => x.LinkedAddressSyncId).IsOptional();
			Property<Address, int>(x => x.Postal).IsRequired().HasMaximumLength(128);
			Property<Address, int>(x => x.State).IsRequired().HasMaximumLength(128);
			Property<Address, int>(x => x.ModifiedOn).IsRequired();
			Property<Address, int>(x => x.CreatedOn).IsRequired();
			HasOptional<Address, Address, int>(x => x.LinkedAddress, x => x.LinkedAddressId, x => x.LinkedAddresses);

			Foods = GetRepository<Food, int>();
			Property<Food, int>(x => x.Id).IsRequired().IsUnique();
			Property<Food, int>(x => x.Name).IsRequired().HasMaximumLength(256);
			Property<Food, int>(x => x.ModifiedOn).IsRequired();
			Property<Food, int>(x => x.CreatedOn).IsRequired();

			FoodRelationships = GetRepository<FoodRelationship, int>();
			Property<FoodRelationship, int>(x => x.Id).IsRequired().IsUnique();
			Property<FoodRelationship, int>(x => x.ChildId).IsRequired();
			Property<FoodRelationship, int>(x => x.ParentId).IsRequired();
			Property<FoodRelationship, int>(x => x.Quantity).IsRequired();
			Property<FoodRelationship, int>(x => x.ModifiedOn).IsRequired();
			Property<FoodRelationship, int>(x => x.CreatedOn).IsRequired();
			HasRequired<FoodRelationship, Food, int>(x => x.Child, x => x.ChildId, x => x.Parents);
			HasRequired<FoodRelationship, Food, int>(x => x.Parent, x => x.ParentId, x => x.Children);

			Groups = GetRepository<Group, int>();
			Property<Group, int>(x => x.Id).IsRequired().IsUnique();
			Property<Group, int>(x => x.Description).IsRequired().HasMaximumLength(4000);
			Property<Group, int>(x => x.Name).IsRequired().HasMaximumLength(256);
			Property<Group, int>(x => x.ModifiedOn).IsRequired();
			Property<Group, int>(x => x.CreatedOn).IsRequired();

			GroupMembers = GetRepository<GroupMember, int>();
			Property<GroupMember, int>(x => x.Id).IsRequired().IsUnique();
			Property<GroupMember, int>(x => x.GroupId).IsRequired();
			Property<GroupMember, int>(x => x.GroupSyncId).IsRequired();
			Property<GroupMember, int>(x => x.MemberId).IsRequired();
			Property<GroupMember, int>(x => x.MemberSyncId).IsRequired();
			Property<GroupMember, int>(x => x.Role).IsRequired().HasMaximumLength(4000);
			Property<GroupMember, int>(x => x.ModifiedOn).IsRequired();
			Property<GroupMember, int>(x => x.CreatedOn).IsRequired();
			HasRequired<GroupMember, Group, int>(x => x.Group, x => x.GroupId, x => x.GroupMembers);
			HasRequired<GroupMember, Person, int>(x => x.Member, x => x.MemberId, x => x.Groups);

			LogEvents = GetRepository<LogEvent, string>();
			Property<LogEvent, string>(x => x.Id).IsRequired().HasMaximumLength(250).IsUnique();
			Property<LogEvent, string>(x => x.Message).IsOptional().HasMaximumLength(4000);
			Property<LogEvent, string>(x => x.CreatedOn).IsRequired();

			People = GetSyncableRepository<Person>();
			Property<Person, int>(x => x.Id).IsRequired().IsUnique();
			Property<Person, int>(x => x.AddressId).IsRequired();
			Property<Person, int>(x => x.AddressSyncId).IsRequired();
			Property<Person, int>(x => x.BillingAddressId).IsOptional();
			Property<Person, int>(x => x.BillingAddressSyncId).IsOptional();
			Property<Person, int>(x => x.Name).IsRequired().HasMaximumLength(256).IsUnique();
			Property<Person, int>(x => x.ModifiedOn).IsRequired();
			Property<Person, int>(x => x.CreatedOn).IsRequired();
			HasRequired<Person, Address, int>(x => x.Address, x => x.AddressId, x => x.People);
			HasOptional<Person, Address, int>(x => x.BillingAddress, x => x.BillingAddressId, x => x.BillingPeople);

			Pets = GetRepository<Pet, Pet.PetKey>();
			Property<Pet, Pet.PetKey>(x => x.Name).IsRequired().HasMaximumLength(128).IsUnique();
			Property<Pet, Pet.PetKey>(x => x.OwnerId).IsRequired().IsUnique();
			Property<Pet, Pet.PetKey>(x => x.CreatedOn).IsRequired();
			Property<Pet, Pet.PetKey>(x => x.ModifiedOn).IsRequired();
			Property<Pet, Pet.PetKey>(x => x.TypeId).IsRequired().HasMaximumLength(25);
			HasRequired<Pet, Pet.PetKey, Person, int>(x => x.Owner, x => x.OwnerId, x => x.Owners);
			HasRequired<Pet, Pet.PetKey, PetType, string>(x => x.Type, x => x.TypeId, x => x.Types);

			PetTypes = GetRepository<PetType, string>();
			Property<PetType, string>(x => x.Id).IsRequired().HasMaximumLength(25).IsUnique();
			Property<PetType, string>(x => x.Type).IsOptional().HasMaximumLength(200);
		}

		#endregion

		#region Properties

		public IRepository<Address, int> Addresses { get; }
		public IRepository<FoodRelationship, int> FoodRelationships { get; }
		public IRepository<Food, int> Foods { get; }
		public IRepository<GroupMember, int> GroupMembers { get; }
		public IRepository<Group, int> Groups { get; }
		public IRepository<LogEvent, string> LogEvents { get; }
		public IRepository<Person, int> People { get; }
		public IRepository<Pet, Pet.PetKey> Pets { get; }
		public IRepository<PetType, string> PetTypes { get; }

		#endregion
	}
}