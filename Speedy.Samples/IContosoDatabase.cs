#region References

using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples
{
	public interface IContosoDatabase : IDatabase
	{
		#region Properties

		IRepository<Address> Addresses { get; }

		IRepository<FoodRelationship> FoodRelationships { get; }

		IRepository<Food> Foods { get; }

		IRepository<LogEvent> LogEvents { get; }

		IRepository<Person> People { get; }

		IRepository<SyncTombstone> SyncTombstones { get; }

		#endregion
	}
}