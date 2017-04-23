#region References

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
			HasKey(x => new { x.Name, x.OwnerId });

			ToTable("Pets");

			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.Name);
			Ignore(x => x.Id);

			HasRequired(x => x.Owner)
				.WithMany(x => x.Pets)
				.HasForeignKey(x => x.OwnerId)
				.WillCascadeOnDelete(false);
		}

		#endregion
	}
}