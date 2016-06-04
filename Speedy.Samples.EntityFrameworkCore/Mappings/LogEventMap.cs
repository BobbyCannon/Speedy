#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFrameworkCore;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFrameworkCore.Mappings
{
	[ExcludeFromCodeCoverage]
	public class LogEventMap : IEntityTypeConfiguration
	{
		#region Methods

		public void Configure(ModelBuilder instance)
		{
			var mapping = instance.Entity<LogEvent>();

			mapping.HasKey(t => t.Id);
			mapping.ToTable("LogEvents");
			mapping.Property(t => t.Id).UseSqlServerIdentityColumn();
			mapping.Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2");
		}

		#endregion
	}
}