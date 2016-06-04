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
			mapping.Property(t => t.Id).UseSqlServerIdentityColumn();
			mapping.Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.SyncId).IsRequired();
			mapping.Property(t => t.ReferenceId).IsRequired().HasMaxLength(128);
			mapping.Property(t => t.TypeName).IsRequired().HasMaxLength(768);
		}

		#endregion
	}
}