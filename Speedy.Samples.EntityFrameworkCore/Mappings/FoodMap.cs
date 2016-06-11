#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFrameworkCore;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Mappings
{
	[ExcludeFromCodeCoverage]
	public class FoodMap : IEntityTypeConfiguration
	{
		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<Food>();

			mapping.HasKey(x => x.Id);
			mapping.ToTable("Foods");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.ModifiedOn).IsRequired().HasColumnType("datetime2");
			mapping.Property(x => x.Name).IsRequired().HasMaxLength(256);
		}

		#endregion
	}
}