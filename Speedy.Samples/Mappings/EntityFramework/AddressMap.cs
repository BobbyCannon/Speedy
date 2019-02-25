#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.EntityFramework
{
	[ExcludeFromCodeCoverage]
	public class AddressMap : EntityMappingConfiguration<Address>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<Address> b)
		{
			b.ToTable("Addresses", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.City).HasColumnName("City").HasMaxLength(256).IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
			b.Property(x => x.Line1).HasColumnName("Line1").HasMaxLength(256).IsRequired();
			b.Property(x => x.Line2).HasColumnName("Line2").HasMaxLength(256).IsRequired();
			b.Property(x => x.LinkedAddressId).HasColumnName("LinkedAddressId").HasColumnType("int").IsRequired(false);
			b.Property(x => x.LinkedAddressSyncId).HasColumnName("LinkedAddressSyncId").IsRequired(false);
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").IsRequired();
			b.Property(x => x.Postal).HasColumnName("Postal").HasMaxLength(128).IsRequired();
			b.Property(x => x.State).HasColumnName("State").HasMaxLength(128).IsRequired();
			b.Property(x => x.SyncId).HasColumnName("SyncId").IsRequired();

			b.HasIndex(x => x.LinkedAddressId).HasName("IX_LinkedAddressId");
			b.HasIndex(x => x.SyncId).HasName("IX_SyncId").IsUnique();

			b.HasOne(x => x.LinkedAddress).WithMany(x => x.LinkedAddresses).HasForeignKey(x => x.LinkedAddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}