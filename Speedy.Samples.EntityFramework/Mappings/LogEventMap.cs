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
			ToTable("LogEvents", "dbo");
			HasKey(x => x.Id);

			Property(x => x.CreatedOn).HasColumnName("CreatedOn").HasColumnType("datetime2").IsRequired().HasPrecision(7);
			Property(x => x.Id).HasColumnName("Id").HasColumnType("nvarchar").IsRequired().HasMaxLength(250).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
			Property(x => x.Message).HasColumnName("Message").HasColumnType("nvarchar").IsOptional().HasMaxLength(4000);
		}

		#endregion
	}
}