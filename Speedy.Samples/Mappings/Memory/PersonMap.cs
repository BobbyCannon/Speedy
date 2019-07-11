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
			database.Property<PersonEntity, int>(x => x.AddressId).IsRequired();
			database.Property<PersonEntity, int>(x => x.AddressSyncId).IsRequired();
			database.Property<PersonEntity, int>(x => x.BillingAddressId).IsOptional();
			database.Property<PersonEntity, int>(x => x.BillingAddressSyncId).IsOptional();
			database.Property<PersonEntity, int>(x => x.CreatedOn).IsRequired();
			database.Property<PersonEntity, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<PersonEntity, int>(x => x.ModifiedOn).IsRequired();
			database.Property<PersonEntity, int>(x => x.Name).IsRequired().HasMaximumLength(256).IsUnique();
			database.Property<PersonEntity, int>(x => x.SyncId).IsRequired().IsUnique();
			database.HasRequired<PersonEntity, int, AddressEntity, long>(x => x.Address, x => x.AddressId, x => x.People);
			database.HasOptional<PersonEntity, int, AddressEntity, long>(x => x.BillingAddress, x => x.BillingAddressId);
		}

		#endregion
	}
}