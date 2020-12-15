#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class TrackerPathMap : EntityMappingConfiguration<TrackerPathEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<TrackerPathEntity> b)
		{
			b.ToTable("TrackerPaths", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CompletedOn).IsRequired();
			b.Property(x => x.ConfigurationId).IsRequired();
			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.Data).IsRequired(false);
			b.Property(x => x.ElapsedTicks).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.ParentId).IsRequired(false);
			b.Property(x => x.StartedOn).IsRequired();
			b.Property(x => x.SyncId).IsRequired();
			b.Property(x => x.Value01).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value02).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value03).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value04).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value05).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value06).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value07).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value08).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Value09).IsRequired(false).HasMaxLength(900);

			b.HasIndex(x => x.SyncId).HasName("IX_TrackerPaths_SyncId").IsUnique();

			b.HasOne(x => x.Configuration).WithMany(x => x.Paths).HasForeignKey(x => x.ConfigurationId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}