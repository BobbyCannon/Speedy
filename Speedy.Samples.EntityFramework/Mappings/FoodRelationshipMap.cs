#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;
using Speedy;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodRelationshipMap : EntityTypeConfiguration<FoodRelationship>
	{
		#region Constructors

		public FoodRelationshipMap()
		{
			ToTable("FoodRelationships", "dbo");
			HasKey(x => x.Id);

			Property(x => x.ChildId).HasColumnName("ChildId").HasColumnType("int").IsRequired();
			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.ParentId).HasColumnName("ParentId").HasColumnType("int").IsRequired();
			Property(x => x.Quantity).HasColumnName("Quantity").HasColumnType("decimal").IsRequired().HasPrecision(18, 2);

			HasRequired(x => x.Child).WithMany(x => x.ParentRelationships).HasForeignKey(x => x.ChildId).WillCascadeOnDelete(false);
			HasRequired(x => x.Parent).WithMany(x => x.ChildRelationships).HasForeignKey(x => x.ParentId).WillCascadeOnDelete(false);
		}

		#endregion
	}
}