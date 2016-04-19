#region References

using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Samples
{
	[ExcludeFromCodeCoverage]
	public class EntityFrameworkContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public EntityFrameworkContosoDatabase()
			: this("name=DefaultConnection", DatabaseOptions.GetDefaults())
		{
		}

		public EntityFrameworkContosoDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
			// Setup options.
			Options.SyncOrder = ContosoDatabase.SyncOrder;
		}

		static EntityFrameworkContosoDatabase()
		{
			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<EntityFrameworkContosoDatabase, Migrations.Configuration>());
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => GetSyncableRepository<Address>();

		public IRepository<FoodRelationship> FoodRelationships => GetRepository<FoodRelationship>();

		public IRepository<Food> Foods => GetRepository<Food>();

		public IRepository<LogEvent> LogEvents => GetRepository<LogEvent>();

		public IRepository<Person> People => GetSyncableRepository<Person>();

		public IRepository<SyncTombstone> SyncTombstones => GetRepository<SyncTombstone>();

		#endregion
	}
}