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
	public class PetTypeMap : EntityMappingConfiguration<PetTypeEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<PetTypeEntity> b)
		{
			b.ToTable("PetType", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.Id).HasColumnName("PetTypeId").HasMaxLength(25).IsRequired();
			b.Property(x => x.Type).HasColumnName("Type").HasMaxLength(200).IsRequired(false);
		}

		#endregion
	}
}