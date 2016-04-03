#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public interface ISampleDatabase : IDatabase
	{
		#region Properties

		IEntityRepository<Address> Addresses { get; }

		IEntityRepository<FoodRelationship> FoodRelationships { get; }

		IEntityRepository<Food> Foods { get; }

		IEntityRepository<LogEvent> LogEvents { get; }

		IEntityRepository<Person> People { get; }

		#endregion
	}
}