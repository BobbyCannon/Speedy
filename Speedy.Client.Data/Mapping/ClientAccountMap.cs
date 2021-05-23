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

			b.Property(x => x.AddressId).HasColumnName("AccountAddressId").IsRequired();
			b.Property(x => x.AddressSyncId).HasColumnName("AccountAddressSyncId").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("AccountCreatedOn").IsRequired();
			b.Property(x => x.EmailAddress).HasColumnName("AccountEmailAddress").HasMaxLength(128).IsRequired();
			b.Property(x => x.Id).HasColumnName("AccountId").IsRequired();
			b.Property(x => x.IsDeleted).HasColumnName("AccountIsDeleted").IsRequired();
			b.Property(x => x.LastClientUpdate).HasColumnName("AccountLastClientUpdate").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("AccountModifiedOn").IsRequired();
			b.Property(x => x.Name).HasColumnName("AccountName").IsRequired();
			b.Property(x => x.Roles).HasColumnName("AccountRoles").IsRequired();
			b.Property(x => x.SyncId).HasColumnName("AccountSyncId").IsRequired();

			b.HasIndex(x => x.LastClientUpdate).HasDatabaseName("IX_Accounts_LastClientUpdate").IsUnique(false);
			b.HasIndex(x => x.SyncId).HasDatabaseName("IX_Accounts_SyncId").IsUnique();

			b.HasOne(x => x.Address).WithMany(x => x.Accounts).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.NoAction);
		}

		#endregion
	}
}