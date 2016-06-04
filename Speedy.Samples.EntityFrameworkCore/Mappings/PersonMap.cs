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

			mapping.HasKey(t => t.Id);
			mapping.ToTable("People");
			mapping.Property(t => t.Id).UseSqlServerIdentityColumn();
			mapping.Property(t => t.AddressSyncId).IsRequired();
			mapping.Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(t => t.Name).IsRequired().HasMaxLength(256);
			mapping.Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.HasOne(x => x.Address).WithMany(x => x.People).HasForeignKey(x => x.AddressId).IsRequired().OnDelete(DeleteBehavior.Restrict);
			mapping.HasOne(x => x.BillingAddress).WithMany().HasForeignKey(x => x.BillingAddressId).IsRequired(false);
			mapping.HasIndex(x => x.Name).IsUnique();
		}

		#endregion
	}
}