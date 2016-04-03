#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	[ExcludeFromCodeCoverage]
	public class EntityFrameworkSampleDatabase : EntityFrameworkDatabase, ISampleDatabase
	{
		#region Constructors

		public EntityFrameworkSampleDatabase()
			: this("name=DefaultConnection", new DatabaseOptions { MaintainDates = true })
		{
		}

		public EntityFrameworkSampleDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => GetRepository<Address>();

		public IRepository<FoodRelationship> FoodRelationships => GetRepository<FoodRelationship>();

		public IRepository<Food> Foods => GetRepository<Food>();

		public IRepository<LogEvent> LogEvents => GetRepository<LogEvent>();

		public IRepository<Person> People => GetRepository<Person>();

		#endregion
	}
}