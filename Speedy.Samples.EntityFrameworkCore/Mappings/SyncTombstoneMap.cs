#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFrameworkCore;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Mappings
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneMap : IEntityTypeConfiguration
	{
		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<SyncTombstone>();

			mapping.HasKey(x => x.Id);
			mapping.ToTable("SyncTombstones");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.SyncId).IsRequired();
			mapping.Property(x => x.ReferenceId).IsRequired().HasMaxLength(128);
			mapping.Property(x => x.TypeName).IsRequired().HasMaxLength(768);
		}

		#endregion
	}
}