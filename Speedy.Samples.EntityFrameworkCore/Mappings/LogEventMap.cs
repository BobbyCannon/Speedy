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

			mapping.HasKey(x => x.Id);
			mapping.ToTable("LogEvents");
			mapping.Property(x => x.Id).UseSqlServerIdentityColumn();
			mapping.Property(x => x.CreatedOn).IsRequired().HasColumnType("datetime2");
		}

		#endregion
	}
}