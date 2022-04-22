#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Data.Mappings
{
	[ExcludeFromCodeCoverage]
	public class AddressMap : EntityMappingConfiguration<AddressEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<AddressEntity> b)
		{
			b.ToTable("Addresses", "dbo");
			b.HasKey(x => x.Id);

			//
			// All database names are going to be renamed to ensure all
			//  - BULK commands work
			//  - Custom SQL works
			//

			b.Property(x => x.AccountSyncId).IsRequired(false);
			b.Property(x => x.City).HasColumnName("AddressCity").HasMaxLength(256).IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("AddressCreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("AddressId").IsRequired();
			b.Property(x => x.IsDeleted).HasColumnName("AddressIsDeleted").IsRequired();
			b.Property(x => x.Line1).HasColumnName("AddressLineOne").HasMaxLength(256).IsRequired();
			b.Property(x => x.Line2).HasColumnName("AddressLineTwo").HasMaxLength(256).IsRequired();
			b.Property(x => x.LinkedAddressId).HasColumnName("AddressLinkedAddressId").IsRequired(false);
			b.Property(x => x.LinkedAddressSyncId).HasColumnName("AddressLinkedAddressSyncId").IsRequired(false);
			b.Property(x => x.ModifiedOn).HasColumnName("AddressModifiedOn").IsRequired();
			b.Property(x => x.Postal).HasColumnName("AddressPostal").HasMaxLength(128).IsRequired();
			b.Property(x => x.State).HasColumnName("AddressState").HasMaxLength(128).IsRequired();
			b.Property(x => x.SyncId).HasColumnName("AddressSyncId").IsRequired();

			b.HasIndex(x => x.LinkedAddressId).HasName("IX_Address_LinkedAddressId");
			b.HasIndex(x => x.SyncId).HasName("IX_Address_SyncId").IsUnique();

			b.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.LinkedAddress).WithMany(x => x.LinkedAddresses).HasForeignKey(x => x.LinkedAddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}