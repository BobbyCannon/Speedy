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
	public class GroupMap : EntityTypeConfiguration<Group>
	{
		#region Constructors

		public GroupMap()
		{
			// Primary Key
			HasKey(x => x.Id);

			// Table & Column Mappings
			ToTable("Groups");
			Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
			Property(x => x.Description).IsRequired();
			Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = false } }));
			Property(x => x.Name).IsRequired().HasMaxLength(256).HasColumnAnnotation("Index", new IndexAnnotation(new[] { new IndexAttribute { IsUnique = true } }));
		}

		#endregion
	}
}