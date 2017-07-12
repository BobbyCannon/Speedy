#region References

using System;
using Speedy.Samples.Entities;
using Speedy;

#endregion

namespace Speedy.Samples
{
	public class ContosoMemoryDatabase : Speedy.Database, IContosoDatabase
	{
		#region Constructors

		public ContosoMemoryDatabase(string directory = null, DatabaseOptions options = null)
			: base(directory, options)
		{
			Addresses = GetRepository<Speedy.Samples.Entities.Address, int>();
			Property<Speedy.Samples.Entities.Address, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.Address, int>(x => x.City).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Address, int>(x => x.Line1).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Address, int>(x => x.Line2).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Address, int>(x => x.LinkedAddressId).IsOptional();
			Property<Speedy.Samples.Entities.Address, int>(x => x.LinkedAddressSyncId).IsOptional();
			Property<Speedy.Samples.Entities.Address, int>(x => x.Postal).IsRequired().HasMaximumLength(128);
			Property<Speedy.Samples.Entities.Address, int>(x => x.State).IsRequired().HasMaximumLength(128);
			Property<Speedy.Samples.Entities.Address, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.Address, int>(x => x.CreatedOn).IsRequired();
			HasOptional<Address, Address, int>(x => x.LinkedAddress, x => x.LinkedAddressId, x => x.LinkedAddresses);

			Foods = GetRepository<Speedy.Samples.Entities.Food, int>();
			Property<Speedy.Samples.Entities.Food, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.Food, int>(x => x.Name).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Food, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.Food, int>(x => x.CreatedOn).IsRequired();

			FoodRelationships = GetRepository<Speedy.Samples.Entities.FoodRelationship, int>();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.ChildId).IsRequired();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.ParentId).IsRequired();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.Quantity).IsRequired();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.FoodRelationship, int>(x => x.CreatedOn).IsRequired();
			HasRequired<FoodRelationship, Food, int>(x => x.Child, x => x.ChildId, x => x.Children);
			HasRequired<FoodRelationship, Food, int>(x => x.Parent, x => x.ParentId, x => x.Parents);

			Groups = GetRepository<Speedy.Samples.Entities.Group, int>();
			Property<Speedy.Samples.Entities.Group, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.Group, int>(x => x.Description).IsRequired().HasMaximumLength(4000);
			Property<Speedy.Samples.Entities.Group, int>(x => x.Name).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Group, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.Group, int>(x => x.CreatedOn).IsRequired();

			GroupMembers = GetRepository<Speedy.Samples.Entities.GroupMember, int>();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.GroupId).IsRequired();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.GroupSyncId).IsRequired();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.MemberId).IsRequired();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.MemberSyncId).IsRequired();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.Role).IsRequired().HasMaximumLength(4000);
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.GroupMember, int>(x => x.CreatedOn).IsRequired();
			HasRequired<GroupMember, Group, int>(x => x.Group, x => x.GroupId, x => x.GroupMembers);
			HasRequired<GroupMember, Person, int>(x => x.Member, x => x.MemberId, x => x.Members);

			LogEvents = GetRepository<Speedy.Samples.Entities.LogEvent, string>();
			Property<Speedy.Samples.Entities.LogEvent, string>(x => x.Id).IsRequired().HasMaximumLength(250).IsUnique();
			Property<Speedy.Samples.Entities.LogEvent, string>(x => x.Message).IsOptional().HasMaximumLength(4000);
			Property<Speedy.Samples.Entities.LogEvent, string>(x => x.CreatedOn).IsRequired();

			People = GetRepository<Speedy.Samples.Entities.Person, int>();
			Property<Speedy.Samples.Entities.Person, int>(x => x.Id).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.Person, int>(x => x.AddressId).IsRequired();
			Property<Speedy.Samples.Entities.Person, int>(x => x.AddressSyncId).IsRequired();
			Property<Speedy.Samples.Entities.Person, int>(x => x.BillingAddressId).IsOptional();
			Property<Speedy.Samples.Entities.Person, int>(x => x.BillingAddressSyncId).IsOptional();
			Property<Speedy.Samples.Entities.Person, int>(x => x.Name).IsRequired().HasMaximumLength(256);
			Property<Speedy.Samples.Entities.Person, int>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.Person, int>(x => x.CreatedOn).IsRequired();
			HasRequired<Person, Address, int>(x => x.Address, x => x.AddressId, x => x.People);
			HasOptional<Person, Address, int>(x => x.BillingAddress, x => x.BillingAddressId, x => x.BillingPeople);

			Pets = GetRepository<Speedy.Samples.Entities.Pet, Pet.PetKey>();
			Property<Speedy.Samples.Entities.Pet, Pet.PetKey>(x => x.Name).IsRequired().HasMaximumLength(128).IsUnique();
			Property<Speedy.Samples.Entities.Pet, Pet.PetKey>(x => x.OwnerId).IsRequired().IsUnique();
			Property<Speedy.Samples.Entities.Pet, Pet.PetKey>(x => x.CreatedOn).IsRequired();
			Property<Speedy.Samples.Entities.Pet, Pet.PetKey>(x => x.ModifiedOn).IsRequired();
			Property<Speedy.Samples.Entities.Pet, Pet.PetKey>(x => x.TypeId).IsRequired().HasMaximumLength(25);
			HasRequired<Pet, Pet.PetKey, Person, int>(x => x.Owner, x => x.OwnerId, x => x.Owners);
			HasRequired<Pet, Pet.PetKey, PetType, string>(x => x.Type, x => x.TypeId, x => x.Types);

			PetTypes = GetRepository<Speedy.Samples.Entities.PetType, string>();
			Property<Speedy.Samples.Entities.PetType, string>(x => x.Id).IsRequired().HasMaximumLength(25).IsUnique();
			Property<Speedy.Samples.Entities.PetType, string>(x => x.Type).IsOptional().HasMaximumLength(200);
		}

		#endregion

		#region Properties

		public Speedy.IRepository<Speedy.Samples.Entities.Address, int> Addresses { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.Food, int> Foods { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.FoodRelationship, int> FoodRelationships { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.Group, int> Groups { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.GroupMember, int> GroupMembers { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.LogEvent, string> LogEvents { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.Person, int> People { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.Pet, Pet.PetKey> Pets { get; }
		public Speedy.IRepository<Speedy.Samples.Entities.PetType, string> PetTypes { get; }

		#endregion
	}
}