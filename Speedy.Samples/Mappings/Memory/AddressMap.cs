#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class AddressMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<Address, int>(x => x.City).IsRequired().HasMaximumLength(256);
			database.Property<Address, int>(x => x.CreatedOn).IsRequired();
			database.Property<Address, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<Address, int>(x => x.Line1).IsRequired().HasMaximumLength(256);
			database.Property<Address, int>(x => x.Line2).IsRequired().HasMaximumLength(256);
			database.Property<Address, int>(x => x.LinkedAddressId).IsOptional();
			database.Property<Address, int>(x => x.LinkedAddressSyncId).IsOptional();
			database.Property<Address, int>(x => x.ModifiedOn).IsRequired();
			database.Property<Address, int>(x => x.Postal).IsRequired().HasMaximumLength(128);
			database.Property<Address, int>(x => x.State).IsRequired().HasMaximumLength(128);
			database.Property<Address, int>(x => x.SyncId).IsRequired().IsUnique();
			database.HasOptional<Address, Address, int>(x => x.LinkedAddress, x => x.LinkedAddressId, x => x.LinkedAddresses);
		}

		#endregion
	}
}