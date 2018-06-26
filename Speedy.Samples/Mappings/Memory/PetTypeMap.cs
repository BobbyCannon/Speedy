#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class PetTypeMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<PetType, string>(x => x.Id).IsRequired().HasMaximumLength(25).IsUnique();
			database.Property<PetType, string>(x => x.Type).IsOptional().HasMaximumLength(200);
		}

		#endregion
	}
}