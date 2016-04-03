#region References

using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public interface ISampleDatabase : IDatabase
	{
		#region Properties

		IRepository<Address> Addresses { get; }

		IRepository<FoodRelationship> FoodRelationships { get; }

		IRepository<Food> Foods { get; }

		IRepository<LogEvent> LogEvents { get; }

		IRepository<Person> People { get; }

		#endregion
	}
}