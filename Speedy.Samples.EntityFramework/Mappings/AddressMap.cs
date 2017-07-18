#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class AddressMap : EntityTypeConfiguration<Address>
	{
		#region Constructors

		public AddressMap()
		{
			ToTable("Addresses", "dbo");
			HasKey(x => x.Id);

			Property(x => x.City).HasColumnName("City").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.Line1).HasColumnName("Line1").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
			Property(x => x.Line2).HasColumnName("Line2").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
			Property(x => x.LinkedAddressId).HasColumnName("LinkedAddressId").HasColumnType("int").IsOptional();
			Property(x => x.LinkedAddressSyncId).HasColumnName("LinkedAddressSyncId").HasColumnType("uniqueidentifier").IsOptional();
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Postal).HasColumnName("Postal").HasColumnType("nvarchar").IsRequired().HasMaxLength(128);
			Property(x => x.State).HasColumnName("State").HasColumnType("nvarchar").IsRequired().HasMaxLength(128);
			Property(x => x.SyncId).HasColumnType("uniqueidentifier").IsRequired().HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = true } }));

			HasOptional(x => x.LinkedAddress).WithMany(x => x.LinkedAddresses).HasForeignKey(x => x.LinkedAddressId).WillCascadeOnDelete(false);
		}

		#endregion
	}
}