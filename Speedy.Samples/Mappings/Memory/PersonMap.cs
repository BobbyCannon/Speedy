#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class PersonMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<Person, int>(x => x.AddressId).IsRequired();
			database.Property<Person, int>(x => x.AddressSyncId).IsRequired();
			database.Property<Person, int>(x => x.BillingAddressId).IsOptional();
			database.Property<Person, int>(x => x.BillingAddressSyncId).IsOptional();
			database.Property<Person, int>(x => x.CreatedOn).IsRequired();
			database.Property<Person, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<Person, int>(x => x.ModifiedOn).IsRequired();
			database.Property<Person, int>(x => x.Name).IsRequired().HasMaximumLength(256).IsUnique();
			database.Property<Person, int>(x => x.SyncId).IsRequired().IsUnique();
			database.HasRequired<Person, Address, int>(x => x.Address, x => x.AddressId, x => x.People);
			database.HasOptional<Person, Address, int>(x => x.BillingAddress, x => x.BillingAddressId, x => x.BillingPeople);
		}

		#endregion
	}
}