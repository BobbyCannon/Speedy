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
			database.Property<AddressEntity, long>(x => x.City).IsRequired().HasMaximumLength(256);
			database.Property<AddressEntity, long>(x => x.CreatedOn).IsRequired();
			database.Property<AddressEntity, long>(x => x.Id).IsRequired().IsUnique();
			database.Property<AddressEntity, long>(x => x.Line1).IsRequired().HasMaximumLength(256);
			database.Property<AddressEntity, long>(x => x.Line2).IsRequired().HasMaximumLength(256);
			database.Property<AddressEntity, long>(x => x.LinkedAddressId).IsOptional();
			database.Property<AddressEntity, long>(x => x.LinkedAddressSyncId).IsOptional();
			database.Property<AddressEntity, long>(x => x.ModifiedOn).IsRequired();
			database.Property<AddressEntity, long>(x => x.Postal).IsRequired().HasMaximumLength(128);
			database.Property<AddressEntity, long>(x => x.State).IsRequired().HasMaximumLength(128);
			database.Property<AddressEntity, long>(x => x.SyncId).IsRequired().IsUnique();
			database.HasOptional<AddressEntity, long, AddressEntity, long>(x => x.LinkedAddress, x => x.LinkedAddressId, x => x.LinkedAddresses);
		}

		#endregion
	}
}