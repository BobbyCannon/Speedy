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
			: this("name=DefaultConnection")
		{
		}

		public EntityFrameworkSampleDatabase(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
		}

		#endregion

		#region Properties

		public IEntityRepository<Address> Addresses => GetRepository<Address>();

		public IEntityRepository<FoodRelationship> FoodRelationships => GetRepository<FoodRelationship>();

		public IEntityRepository<Food> Foods => GetRepository<Food>();

		public IEntityRepository<Person> People => GetRepository<Person>();

		#endregion
	}
}