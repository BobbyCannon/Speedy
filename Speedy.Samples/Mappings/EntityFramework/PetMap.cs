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
	public class PetMap : EntityMappingConfiguration<Pet>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<Pet> b)
		{
			b.ToTable("Pets", "dbo");
			b.HasKey(x => new { x.Name, x.OwnerId });

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(128)").IsRequired();
			b.Property(x => x.OwnerId).HasColumnName("OwnerId").HasColumnType("int").IsRequired();
			b.Property(x => x.TypeId).HasColumnName("TypeId").HasColumnType("nvarchar(25)").IsRequired();
			b.Ignore(x => x.Id);

			b.HasIndex(x => x.OwnerId).HasName("IX_OwnerId");
			b.HasIndex(x => x.TypeId).HasName("IX_TypeId");

			b.HasOne(x => x.Owner).WithMany(x => x.Owners).HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Type).WithMany(x => x.Types).HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}