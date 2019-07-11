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
			database.Property<GroupEntity, int>(x => x.CreatedOn).IsRequired();
			database.Property<GroupEntity, int>(x => x.Description).IsRequired().HasMaximumLength(4000);
			database.Property<GroupEntity, int>(x => x.Id).IsRequired().IsUnique();
			database.Property<GroupEntity, int>(x => x.ModifiedOn).IsRequired();
			database.Property<GroupEntity, int>(x => x.Name).IsRequired().HasMaximumLength(256);
		}

		#endregion
	}
}