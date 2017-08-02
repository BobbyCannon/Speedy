#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;
using Speedy;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodMap : EntityTypeConfiguration<Food>
	{
		#region Constructors

		public FoodMap()
		{
			ToTable("Foods", "dbo");
			HasKey(x => x.Id);

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
		}

		#endregion
	}
}