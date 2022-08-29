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
	public class AccountMap : EntityMappingConfiguration<AccountEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<AccountEntity> b)
		{
			b.ToTable("Accounts", "dbo");
			b.HasKey(x => x.Id);

			//
			// All database names are going to be renamed to ensure all
			//  - BULK commands work
			//  - Custom SQL works
			//

			b.Property(x => x.AddressId).HasColumnName("AccountAddressId").IsRequired();
			b.Property(x => x.AddressSyncId).HasColumnName("AccountAddressSyncId").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("AccountCreatedOn").IsRequired();
			b.Property(x => x.EmailAddress).HasColumnName("AccountEmailAddress").IsRequired(false);
			b.Property(x => x.ExternalId).HasColumnName("AccountExternalId").IsRequired(false);
			b.Property(x => x.Id).HasColumnName("AccountId").IsRequired();
			b.Property(x => x.IsDeleted).HasColumnName("AccountIsDeleted").IsRequired();
			b.Property(x => x.LastLoginDate).HasColumnName("AccountLastLoginDate").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("AccountModifiedOn").IsRequired();
			b.Property(x => x.Name).HasColumnName("AccountName").HasMaxLength(256).IsRequired();
			b.Property(x => x.Nickname).HasColumnName("AccountNickname").HasMaxLength(256).IsRequired(false);
			b.Property(x => x.PasswordHash).HasColumnName("AccountPasswordHash").IsRequired(false);
			b.Property(x => x.Roles).HasColumnName("AccountRoles").IsRequired(false);
			b.Property(x => x.SyncId).HasColumnName("AccountSyncId").IsRequired();

			#if NET6_0_OR_GREATER
			b.HasIndex(x => x.AddressId).HasDatabaseName("IX_Accounts_AddressId");
			b.HasIndex(x => new { x.AddressId, x.ExternalId }).HasDatabaseName("IX_Accounts_AddressId_ExternalId").IsUnique();
			b.HasIndex(x => x.Name).HasDatabaseName("IX_Accounts_Name").IsUnique();
			b.HasIndex(x => x.Nickname).HasDatabaseName("IX_Accounts_Nickname").IsUnique();
			b.HasIndex(x => x.SyncId).HasDatabaseName("IX_Accounts_SyncId").IsUnique();
			#else
			b.HasIndex(x => x.AddressId).HasName("IX_Accounts_AddressId");
			b.HasIndex(x => new { x.AddressId, x.ExternalId }).HasName("IX_Accounts_AddressId_ExternalId").IsUnique();
			b.HasIndex(x => x.Name).HasName("IX_Accounts_Name").IsUnique();
			b.HasIndex(x => x.Nickname).HasName("IX_Accounts_Nickname").IsUnique();
			b.HasIndex(x => x.SyncId).HasName("IX_Accounts_SyncId").IsUnique();
			#endif

			b.HasOne(x => x.Address).WithMany(x => x.Accounts).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}