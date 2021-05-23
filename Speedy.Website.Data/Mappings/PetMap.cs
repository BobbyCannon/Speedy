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
	public class PetMap : EntityMappingConfiguration<PetEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<PetEntity> b)
		{
			b.ToTable("Pets", "dbo");
			b.HasKey(x => new { x.Name, x.OwnerId });

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasMaxLength(128).IsRequired();
			b.Property(x => x.OwnerId).HasColumnName("OwnerId").IsRequired();
			b.Property(x => x.TypeId).HasColumnName("TypeId").HasMaxLength(25).IsRequired(false);
			b.Ignore(x => x.Id);

			b.HasIndex(x => x.OwnerId).HasDatabaseName("IX_Pets_OwnerId");
			b.HasIndex(x => x.TypeId).HasDatabaseName("IX_Pets_TypeId");
			b.HasIndex(x => new { x.Name, x.OwnerId }).IsUnique();

			b.HasOne(x => x.Owner).WithMany(x => x.Pets).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Type).WithMany(x => x.Types).HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.SetNull);
		}

		#endregion
	}
}