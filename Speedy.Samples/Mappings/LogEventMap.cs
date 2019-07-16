#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class LogEventMap : EntityMappingConfiguration<LogEventEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<LogEventEntity> b)
		{
			b.ToTable("LogEvents", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.CreatedOn).HasColumnName("CreatedOn").IsRequired();
			b.Property(x => x.Id).HasColumnName("Id").HasMaxLength(250).IsRequired();
			b.Property(x => x.Message).HasColumnName("Message").IsRequired(false);
		}

		#endregion
	}
}