#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class PetMap : EntityTypeConfiguration<Pet>
	{
		#region Constructors

		public PetMap()
		{
			ToTable("Pets", "dbo");
			HasKey(x => new { x.Name, x.OwnerId });

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar").IsRequired().HasMaxLength(128).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			Property(x => x.OwnerId).HasColumnName("OwnerId").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			Property(x => x.TypeId).HasColumnName("TypeId").HasColumnType("nvarchar").IsRequired().HasMaxLength(25);
			Ignore(x => x.Id);

			HasRequired(x => x.Owner).WithMany(x => x.Owners).HasForeignKey(x => x.OwnerId).WillCascadeOnDelete(false);
			HasRequired(x => x.Type).WithMany(x => x.Types).HasForeignKey(x => x.TypeId).WillCascadeOnDelete(false);
		}

		#endregion
	}
}