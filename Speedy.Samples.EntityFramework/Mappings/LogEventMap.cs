#region References

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework.Mappings
{
	[ExcludeFromCodeCoverage]
	public class LogEventMap : EntityTypeConfiguration<LogEvent>
	{
		#region Constructors

		public LogEventMap()
		{
			// Primary Key
			HasKey(t => t.Id);

			// Table & Column Mappings
			ToTable("LogEvents");
			Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			Property(t => t.CreatedOn).IsRequired().HasColumnType("datetime2").HasPrecision(7);
		}

		#endregion
	}
}