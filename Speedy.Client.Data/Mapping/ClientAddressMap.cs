#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.Data.Client;
using Speedy.EntityFramework;

#endregion

namespace Speedy.Client.Data.Mapping
{
	[ExcludeFromCodeCoverage]
	public class ClientAddressMap : EntityMappingConfiguration<ClientAddress>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<ClientAddress> b)
		{
			b.ToTable("Addresses", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).HasColumnName("AddressCreatedOn").IsRequired();
			b.Property(x => x.City).HasColumnName("AddressCity").IsRequired();
			b.Property(x => x.Id).HasColumnName("AddressId").IsRequired();
			b.Property(x => x.IsDeleted).HasColumnName("AddressIsDeleted").IsRequired();
			b.Property(x => x.LastClientUpdate).HasColumnName("AddressLastClientUpdate").IsRequired();
			b.Property(x => x.Line1).HasColumnName("AddressLineOne").HasMaxLength(128).IsRequired();
			b.Property(x => x.Line2).HasColumnName("AddressLineTwo").HasMaxLength(128).IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("AddressModifiedOn").IsRequired();
			b.Property(x => x.Postal).HasColumnName("AddressPostal").HasMaxLength(25).IsRequired();
			b.Property(x => x.State).HasColumnName("AddressState").HasMaxLength(25).IsRequired();
			b.Property(x => x.SyncId).HasColumnName("AddressSyncId").IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasDatabaseName("IX_Addresses_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasDatabaseName("IX_Addresses_SyncId").IsUnique();
		}

		#endregion
	}
}