#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.Data.Client;
using Speedy.EntityFramework;

#endregion

namespace Speedy.Client.Data.Mapping
{
	[ExcludeFromCodeCoverage]
	public class ClientLogEventMap : EntityMappingConfiguration<ClientLogEvent>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<ClientLogEvent> b)
		{
			b.ToTable("LogEvents", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.LastClientUpdate).IsRequired();
			b.Property(x => x.Level).IsRequired();
			b.Property(x => x.Message).HasMaxLength(256).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.SyncId).IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasDatabaseName("IX_LogEvents_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasDatabaseName("IX_LogEvents_SyncId").IsUnique();
		}

		#endregion
	}
}