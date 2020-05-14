#region References

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Speedy.EntityFramework;
using Speedy.Website.Samples.Entities;

#endregion

namespace Speedy.Website.Samples.Mappings
{
	[ExcludeFromCodeCoverage]
	public class LogEventMap : EntityMappingConfiguration<LogEventEntity>
	{
		#region Methods

		public override void Map(EntityTypeBuilder<LogEventEntity> b)
		{
			b.ToTable("LogEvents", "dbo");
			b.HasKey(x => x.Id);

			b.Property(x => x.AcknowledgedOn).IsRequired(false);
			b.Property(x => x.CreatedOn).IsRequired();
			b.Property(x => x.Id).HasMaxLength(250).IsRequired();
			b.Property(x => x.Level).IsRequired();
			b.Property(x => x.LoggedOn).IsRequired();
			b.Property(x => x.Message).IsRequired(false);
			b.Property(x => x.ModifiedOn).IsRequired();
		}

		#endregion
	}
}