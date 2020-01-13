#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodRelationshipMap : EntityMappingConfiguration<FoodRelationshipEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<FoodRelationshipEntity> b)
		{
			b.ToTable("FoodRelationships", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.ChildId).HasColumnName("ChildId").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").IsRequired();
			b.Property(x => x.ParentId).HasColumnName("ParentId").IsRequired();
			b.Property(x => x.Quantity).HasColumnName("Quantity").IsRequired();

			b.HasIndex(x => x.ChildId).HasName("IX_FoodRelationships_ChildId");
			b.HasIndex(x => x.ParentId).HasName("IX_FoodRelationships_ParentId");

			b.HasOne(x => x.Child).WithMany(x => x.ParentRelationships).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Parent).WithMany(x => x.ChildRelationships).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}