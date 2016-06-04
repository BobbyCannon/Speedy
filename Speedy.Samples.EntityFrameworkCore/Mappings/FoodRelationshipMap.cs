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
	public class FoodRelationshipMap : IEntityTypeConfiguration
	{
		#region Constructors

		#endregion

		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<FoodRelationship>();

			mapping.HasKey(x => x.Id);
			mapping.ToTable("FoodRelationships");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Quantity).IsRequired();
			mapping.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);
			mapping.HasOne(x => x.Child).WithMany(x => x.Parents).HasForeignKey(d => d.ChildId).OnDelete(DeleteBehavior.Restrict);
		}

		#endregion
	}
}