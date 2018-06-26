#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class GroupMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<Group, int>(x => x.CreatedOn).IsRequired();
			database.Property<Group, int>(x => x.Description).IsRequired().HasMaximumLength(4000);
			database.Property<Group, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<Group, int>(x => x.ModifiedOn).IsRequired();
			database.Property<Group, int>(x => x.Name).IsRequired().HasMaximumLength(256);
		}

		#endregion
	}
}