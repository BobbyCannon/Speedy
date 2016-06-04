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

			mapping.HasKey(t => t.Id);
			mapping.ToTable("Addresses");
			mapping.Property(t => t.Id).UseSqlServerIdentityColumn();
			mapping.Property(t => t.City).IsRequired().HasMaxLength(256);
			mapping.Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.Line1).IsRequired().HasMaxLength(256);
			mapping.Property(t => t.Line2).IsRequired().HasMaxLength(256);
			mapping.Property(t => t.LinkedAddressSyncId).IsRequired(false);
			mapping.Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.Postal).IsRequired().HasMaxLength(128);
			mapping.Property(t => t.State).IsRequired().HasMaxLength(128);
			mapping.HasOne(x => x.LinkedAddress).WithMany().HasForeignKey(x => x.LinkedAddressId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}