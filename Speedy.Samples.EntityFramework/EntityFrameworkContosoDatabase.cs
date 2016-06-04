#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFramework
{
	[ExcludeFromCodeCoverage]
	public class EntityFrameworkContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public EntityFrameworkContosoDatabase()
			: this("name=DefaultConnection", new DatabaseOptions())
		{
		}

		public EntityFrameworkContosoDatabase(DatabaseOptions options)
			: this("name=DefaultConnection", options)
		{
		}

		public EntityFrameworkContosoDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
			Options.SyncOrder = ContosoDatabase.SyncOrder;
		}

		static EntityFrameworkContosoDatabase()
		{
			System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersionByContext<EntityFrameworkContosoDatabase, Migrations.Configuration>());
		}

		#endregion

		#region Properties

		public IRepository<Address> Addresses => GetSyncableRepository<Address>();

		public IRepository<FoodRelationship> FoodRelationships => GetRepository<FoodRelationship>();

		public IRepository<Food> Foods => GetRepository<Food>();

		public IRepository<GroupMember> GroupMembers => GetSyncableRepository<GroupMember>();

		public IRepository<Group> Groups => GetSyncableRepository<Group>();

		public IRepository<LogEvent> LogEvents => GetRepository<LogEvent>();

		public IRepository<Person> People => GetSyncableRepository<Person>();

		#endregion
	}
}