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
	public class PetTypeMap : EntityMappingConfiguration<PetType>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<PetType> b)
		{
			b.ToTable("PetType", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.Id).HasColumnName("PetTypeId").HasColumnType("nvarchar(25)").IsRequired();
			b.Property(x => x.Type).HasColumnName("Type").HasColumnType("nvarchar(200)").IsRequired(false);
		}

		#endregion
	}
}