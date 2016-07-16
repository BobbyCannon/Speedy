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
	public class PersonMap : EntityTypeConfiguration<Person>
	{
		#region Constructors

		public PersonMap()
		{
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("People");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.AddressSyncId).IsRequired();
			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.Name).IsRequired().HasMaxLength(256)
				.HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = true } }));
			Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7)
				.HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));

			// Relationships
			HasRequired(x => x.Address)
				.WithMany(x => x.People)
				.HasForeignKey(x => x.AddressId)
				.WillCascadeOnDelete(false);
			HasOptional(x => x.BillingAddress)
				.WithMany()
				.HasForeignKey(x => x.BillingAddressId)
				.WillCascadeOnDelete(false);
		}

		#endregion
	}
}