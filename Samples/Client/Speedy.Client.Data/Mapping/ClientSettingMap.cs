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
	public class ClientSettingMap : EntityMappingConfiguration<ClientSetting>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<ClientSetting> b)
		{
			b.ToTable("Settings", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.IsDeleted).IsRequired();
			b.Property(x => x.LastClientUpdate).IsRequired();
			b.Property(x => x.Name).HasMaxLength(256).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.SyncId).IsRequired();
			b.Property(x => x.Value).IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasIndexName("IX_Settings_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasIndexName("IX_Settings_SyncId").IsUnique();
		}

		#endregion
	}
}