#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneMap : EntityTypeConfiguration<SyncTombstone>
	{
		#region Constructors

		public SyncTombstoneMap()
		{
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("SyncTombstones");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));
			Property(x => x.SyncId).IsRequired();
			Property(x => x.ReferenceId).IsRequired().HasMaxLength(128).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute("IX_SyncTombstones_ReferenceId_TypeName") { Order = 1, IsUnique = false } }));
			Property(x => x.TypeName).IsRequired().HasMaxLength(768).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute("IX_SyncTombstones_ReferenceId_TypeName") { Order = 2, IsUnique = false } }));
		}

		#endregion
	}
}