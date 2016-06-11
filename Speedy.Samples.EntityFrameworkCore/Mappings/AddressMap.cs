#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Speedy.EntityFrameworkCore;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Mappings
{
	[ExcludeFromCodeCoverage]
	public class AddressMap : IEntityTypeConfiguration
	{
		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<Address>();

			mapping.HasKey(x => x.Id);
			mapping.ToTable("Addresses");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.City).IsRequired().HasMaxLength(256);
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Line1).IsRequired().HasMaxLength(256);
			mapping.Property(x => x.Line2).IsRequired().HasMaxLength(256);
			mapping.Property(x => x.LinkedAddressSyncId).IsRequired(false);
			mapping.Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Postal).IsRequired().HasMaxLength(128);
			mapping.Property(x => x.State).IsRequired().HasMaxLength(128);
			mapping.HasOne(x => x.LinkedAddress).WithMany().HasForeignKey(x => x.LinkedAddressId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}