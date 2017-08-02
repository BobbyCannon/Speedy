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
			ToTable("People", "dbo");
			HasKey(x => x.Id);

			Property(x => x.AddressId).HasColumnName("AddressId").HasColumnType("int").IsRequired();
			Property(x => x.AddressSyncId).HasColumnName("AddressSyncId").HasColumnType("uniqueidentifier").IsRequired();
			Property(x => x.BillingAddressId).HasColumnName("BillingAddressId").HasColumnType("int").IsOptional();
			Property(x => x.BillingAddressSyncId).HasColumnName("BillingAddressSyncId").HasColumnType("uniqueidentifier").IsOptional();
			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("int").IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(x => x.ModifiedOn).HasColumnName("ModifiedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Name).HasColumnName("Name").HasColumnType("nvarchar").IsRequired().HasMaxLength(256).HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_Name") { IsUnique = true }));
			Property(x => x.SyncId).HasColumnName("SyncId").HasColumnType("uniqueidentifier").IsRequired().HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_SyncId") { Order = 1, IsUnique = true }));

			HasRequired(x => x.Address).WithMany(x => x.People).HasForeignKey(x => x.AddressId).WillCascadeOnDelete(false);
			HasOptional(x => x.BillingAddress).WithMany(x => x.BillingPeople).HasForeignKey(x => x.BillingAddressId).WillCascadeOnDelete(false);
		}

		#endregion
	}
}