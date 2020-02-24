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
	public class AccountMap : EntityMappingConfiguration<AccountEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<AccountEntity> b)
		{
			b.ToTable("Accounts", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.AddressId).HasColumnName("AddressId").IsRequired();
			b.Property(x => x.AddressSyncId).HasColumnName("AddressSyncId").IsRequired();
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasMaxLength(256).IsRequired();
			b.Property(x => x.Nickname).HasColumnName("Nickname").HasMaxLength(256).IsRequired(false);
			b.Property(x => x.SyncId).HasColumnName("SyncId").IsRequired();

			b.HasIndex(x => x.AddressId).HasName("IX_Accounts_AddressId");
			b.HasIndex(x => x.Name).HasName("IX_Accounts_Name").IsUnique();
			b.HasIndex(x => x.Nickname).HasName("IX_Accounts_Nickname").IsUnique().HasFilter("Nickname IS NOT NULL");
			b.HasIndex(x => x.SyncId).HasName("IX_Accounts_SyncId").IsUnique();

			b.HasOne(x => x.Address).WithMany(x => x.Accounts).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}