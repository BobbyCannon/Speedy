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
	public class ClientAddressMap : EntityMappingConfiguration<ClientAddress>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<ClientAddress> b)
		{
			b.ToTable("Addresses", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.LastClientUpdate).IsRequired();
			b.Property(x => x.Line1).HasMaxLength(128).IsRequired();
			b.Property(x => x.Line2).HasMaxLength(128).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.Postal).HasMaxLength(25).IsRequired();
			b.Property(x => x.State).HasMaxLength(25).IsRequired();
			b.Property(x => x.SyncId).IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasName("IX_Addresses_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasName("IX_Addresses_SyncId").IsUnique();
		}

		#endregion
	}
}