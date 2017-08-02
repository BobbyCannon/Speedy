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
	public class PetTypeMap : EntityTypeConfiguration<PetType>
	{
		#region Constructors

		public PetTypeMap()
		{
			ToTable("PetType", "dbo");
			HasKey(x => x.Id);

			Property(x => x.Id).HasColumnName("PetTypeId").HasColumnType("nvarchar").IsRequired().HasMaxLength(25).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			Property(x => x.Type).HasColumnName("Type").HasColumnType("nvarchar").IsOptional().HasMaxLength(200);
		}

		#endregion
	}
}