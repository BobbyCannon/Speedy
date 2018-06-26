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
	public class FoodRelationshipMap : EntityMappingConfiguration<FoodRelationship>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<FoodRelationship> b)
		{
			b.ToTable("FoodRelationships", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.ChildId).HasColumnName("ChildId").HasColumnType("int").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.ParentId).HasColumnName("ParentId").HasColumnType("int").IsRequired();
			b.Property(x => x.Quantity).HasColumnName("Quantity").HasColumnType("decimal").IsRequired();

			b.HasIndex(x => x.ChildId).HasName("IX_ChildId");
			b.HasIndex(x => x.ParentId).HasName("IX_ParentId");

			b.HasOne(x => x.Child).WithMany(x => x.ParentRelationships).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Parent).WithMany(x => x.ChildRelationships).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}