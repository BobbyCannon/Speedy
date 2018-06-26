#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.EntityFramework
{
	[ExcludeFromCodeCoverage]
	public class PersonMap : EntityMappingConfiguration<Person>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<Person> b)
		{
			b.ToTable("People", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.AddressId).HasColumnName("AddressId").HasColumnType("int").IsRequired();
			b.Property(x => x.AddressSyncId).HasColumnName("AddressSyncId").HasColumnType("uniqueidentifier").IsRequired();
			b.Property(x => x.BillingAddressId).HasColumnName("BillingAddressId").HasColumnType("int").IsRequired(false);
			b.Property(x => x.BillingAddressSyncId).HasColumnName("BillingAddressSyncId").HasColumnType("uniqueidentifier").IsRequired(false);
			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired();
			b.Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired();
			b.Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar(256)").IsRequired();
			b.Property(x => x.SyncId).HasColumnName("SyncId").HasColumnType("uniqueidentifier").IsRequired();

			b.HasIndex(x => x.AddressId).HasName("IX_AddressId");
			b.HasIndex(x => x.BillingAddressId).HasName("IX_BillingAddressId");
			b.HasIndex(x => x.Name).HasName("IX_Name").IsUnique();
			b.HasIndex(x => x.SyncId).HasName("IX_SyncId").IsUnique();

			b.HasOne(x => x.Address).WithMany(x => x.People).HasForeignKey(x => x.AddressId).OnDelete(DeleteBehavior.Restrict);
			b.HasOne(x => x.BillingAddress).WithMany(x => x.BillingPeople).HasForeignKey(x => x.BillingAddressId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}