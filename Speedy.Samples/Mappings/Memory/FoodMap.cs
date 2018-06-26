#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class FoodMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<Food, int>(x => x.CreatedOn).IsRequired();
			database.Property<Food, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<Food, int>(x => x.ModifiedOn).IsRequired();
			database.Property<Food, int>(x => x.Name).IsRequired().HasMaximumLength(256);
		}

		#endregion
	}
}