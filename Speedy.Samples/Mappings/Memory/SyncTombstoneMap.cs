#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Samples.Mappings.Memory
{
	[ExcludeFromCodeCoverage]
	public class SyncTombstoneMap
	{
		#region Methods

		public static void ConfigureDatabase(Database database)
		{
			database.Property<SyncTombstone, long>(x => x.CreatedOn).IsRequired();
			database.Property<SyncTombstone, long>(x => x.Id).IsRequired().IsUnique();
			database.Property<SyncTombstone, long>(x => x.ReferenceId).IsRequired().HasMaximumLength(128);
			database.Property<SyncTombstone, long>(x => x.SyncId).IsRequired();
			database.Property<SyncTombstone, long>(x => x.TypeName).IsRequired().HasMaximumLength(768);
		}

		#endregion
	}
}