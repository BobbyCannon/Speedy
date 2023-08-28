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
	public class TrackerPathConfigurationMap : EntityMappingConfiguration<TrackerPathConfigurationEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<TrackerPathConfigurationEntity> b)
		{
			b.ToTable("TrackerPathConfigurations", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CompletedOnName).IsRequired(false);
			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.DataName).IsRequired(false);
			b.Property(x => x.PathName).IsRequired().HasMaxLength(896);
			b.Property(x => x.PathType).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.IsDeleted).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.Name01).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name02).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name03).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name04).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name05).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name06).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name07).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name08).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.Name09).IsRequired(false).HasMaxLength(900);
			b.Property(x => x.StartedOnName).IsRequired(false);
			b.Property(x => x.SyncId).IsRequired();
			b.Property(x => x.Type01).IsRequired();
			b.Property(x => x.Type02).IsRequired();
			b.Property(x => x.Type03).IsRequired();
			b.Property(x => x.Type04).IsRequired();
			b.Property(x => x.Type05).IsRequired();
			b.Property(x => x.Type06).IsRequired();
			b.Property(x => x.Type07).IsRequired();
			b.Property(x => x.Type08).IsRequired();
			b.Property(x => x.Type09).IsRequired();

			#if NET6_0_OR_GREATER
			b.HasIndex(x => x.SyncId).HasDatabaseName("IX_TrackerPathConfigurations_SyncId").IsUnique();
			#else
			b.HasIndex(x => x.SyncId).HasName("IX_TrackerPathConfigurations_SyncId").IsUnique();
			#endif
		}

		#endregion
	}
}