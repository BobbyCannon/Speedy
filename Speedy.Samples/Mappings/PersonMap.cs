#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class PersonMap : EntityMappingConfiguration<PersonEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<PersonEntity> b)
		{
			b.ToTable("People", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.AddressId).HasColumnName("AddressId").IsRequired();
			b.Property(x => x.AddressSyncId).HasColumnName("AddressSyncId").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasMaxLength(256).IsRequired();
			b.Property(x => x.SyncId).HasColumnName("SyncId").IsRequired();

			b.HasIndex(x => x.AddressId).HasName("IX_People_AddressId");
			b.HasIndex(x => x.Name).HasName("IX_People_Name").IsUnique();
			b.HasIndex(x => x.SyncId).HasName("IX_People_SyncId").IsUnique();

			b.HasOne(x => x.Address).WithMany(x => x.People).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}