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
	public class ClientAccountMap : EntityMappingConfiguration<ClientAccount>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<ClientAccount> b)
		{
			b.ToTable("Accounts", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.EmailAddress).HasMaxLength(128).IsRequired();
			b.Property(x => x.Id).IsRequired();
			b.Property(x => x.LastClientUpdate).IsRequired();
			b.Property(x => x.ModifiedOn).IsRequired();
			b.Property(x => x.Name).IsRequired();
			b.Property(x => x.Roles).IsRequired();
			b.Property(x => x.SyncId).IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasName("IX_Accounts_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasName("IX_Accounts_SyncId").IsUnique();

			b.HasOne(x => x.Address).WithMany(x => x.Accounts).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.NoAction);
		}

		#endregion
	}
}