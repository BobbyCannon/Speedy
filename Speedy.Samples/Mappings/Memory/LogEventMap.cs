#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class LogEventMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<LogEventEntity, string>(x => x.CreatedOn).IsRequired();
			database.Property<LogEventEntity, string>(x => x.Id).IsRequired().HasMaximumLength(250).IsUnique();
			database.Property<LogEventEntity, string>(x => x.Message).IsOptional().HasMaximumLength(4000);
		}

		#endregion
	}
}