#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Mappings.EntityFramework
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneMap : EntityMappingConfiguration<SyncTombstone>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<SyncTombstone> b)
		{
			b.ToTable("SyncTombstones", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("bigint").IsRequired();
			b.Property(x => x.ReferenceId).HasColumnName("ReferenceId").HasColumnType("nvarchar(128)").IsRequired();
			b.Property(x => x.SyncId).HasColumnName("SyncId").HasColumnType("uniqueidentifier").IsRequired();
			b.Property(x => x.TypeName).HasColumnName("TypeName").HasColumnType("nvarchar(768)").IsRequired();

			b.HasIndex(x => x.CreatedOn).HasName("IX_CreatedOn");
			b.HasIndex(x => new { x.TypeName, x.ReferenceId }).HasName("IX_SyncTombstones_TypeName_ReferenceId");
		}

		#endregion
	}
}