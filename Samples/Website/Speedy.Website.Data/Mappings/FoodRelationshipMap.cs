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

			#if NET6_0_OR_GREATER
			b.HasIndex(x => x.ChildId).HasDatabaseName("IX_FoodRelationships_ChildId");
			b.HasIndex(x => x.ParentId).HasDatabaseName("IX_FoodRelationships_ParentId");
			#else
			b.HasIndex(x => x.ChildId).HasName("IX_FoodRelationships_ChildId");
			b.HasIndex(x => x.ParentId).HasName("IX_FoodRelationships_ParentId");
			#endif

			b.HasOne(x => x.Child).WithMany(x => x.ParentRelationships).HasForeignKey(x => x.ChildId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Parent).WithMany(x => x.ChildRelationships).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}