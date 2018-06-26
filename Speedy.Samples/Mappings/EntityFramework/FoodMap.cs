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
	public class FoodMap : EntityMappingConfiguration<Food>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<Food> b)
		{
			b.ToTable("Foods", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(256)").IsRequired();
		}

		#endregion
	}
}