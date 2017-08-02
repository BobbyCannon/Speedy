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
			ToTable("SyncTombstones", "dbo");
			HasKey(x => x.Id);

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_CreatedOn") { IsUnique = false }));
			Property(x => x.Id).HasColumnName("Id").HasColumnType("bigint").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.ReferenceId).HasColumnName("ReferenceId").HasColumnType("nvarchar").IsRequired().HasMaxLength(128).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_SyncTombstones_ReferenceId_TypeName") { Order = 1, IsUnique = false }));
			Property(x => x.SyncId).HasColumnName("SyncId").HasColumnType("uniqueidentifier").IsRequired();
			Property(x => x.TypeName).HasColumnName("TypeName").HasColumnType("nvarchar").IsRequired().HasMaxLength(768).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_SyncTombstones_ReferenceId_TypeName") { Order = 2, IsUnique = false }));
		}

		#endregion
	}
}