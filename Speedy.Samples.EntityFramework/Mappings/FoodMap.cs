#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodMap : EntityTypeConfiguration<Food>
	{
		#region Constructors

		public FoodMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("Foods");
			Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(t => t.Name).IsRequired().HasMaxLength(256);
		}

		#endregion
	}
}