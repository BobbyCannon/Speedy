#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class PersonMap : EntityTypeConfiguration<Person>
	{
		#region Constructors

		public PersonMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("People");
			Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(t => t.Name).IsRequired().HasMaxLength(256).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = true } }));
			Property(t => t.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);

			// Relationships
			HasRequired(x => x.Address)
				.WithMany(x => x.People)
				.HasForeignKey(x => x.AddressId);
		}

		#endregion
	}
}