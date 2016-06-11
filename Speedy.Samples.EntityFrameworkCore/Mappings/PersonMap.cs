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
	public class PersonMap : IEntityTypeConfiguration
	{
		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<Person>();

			mapping.HasKey(x => x.Id);
			mapping.ToTable("People");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.AddressSyncId).IsRequired();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Name).IsRequired().HasMaxLength(256);
			mapping.Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.HasOne(x => x.Address).WithMany(x => x.People).HasForeignKey(x => x.AddressId).IsRequired().OnDelete(DeleteBehavior.Restrict);
			mapping.HasOne(x => x.BillingAddress).WithMany().HasForeignKey(x => x.BillingAddressId).IsRequired(false);
			mapping.HasIndex(x => x.Name).IsUnique();
		}

		#endregion
	}
}