#region References

using System.ComponentModel.DataAnnotations.Schema;
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
			ToTable("Groups", "dbo");
			HasKey(x => x.Id);

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Description).HasColumnName("Description").HasColumnType("nvarchar").IsRequired().HasMaxLength(4000);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar").IsRequired().HasMaxLength(256);
		}

		#endregion
	}
}