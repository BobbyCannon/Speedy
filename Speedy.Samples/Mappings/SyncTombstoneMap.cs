#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneMap : EntityTypeConfiguration<SyncTombstone>
	{
		#region Constructors

		public SyncTombstoneMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("SyncTombstones");
			Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));
			Property(t => t.SyncId).IsRequired();
		}

		#endregion
	}
}