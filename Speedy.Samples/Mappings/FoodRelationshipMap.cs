#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodRelationshipMap : EntityTypeConfiguration<FoodRelationship>
	{
		#region Constructors

		public FoodRelationshipMap()
		{
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("FoodRelationships");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.Quantity).IsRequired();

			// Relationships
			HasRequired(x => x.Parent)
				.WithMany(x => x.Children)
				.HasForeignKey(d => d.ParentId)
				.WillCascadeOnDelete(false);
			HasRequired(x => x.Child)
				.WithMany(x => x.Parents)
				.HasForeignKey(d => d.ChildId)
				.WillCascadeOnDelete(false);
		}

		#endregion
	}
}