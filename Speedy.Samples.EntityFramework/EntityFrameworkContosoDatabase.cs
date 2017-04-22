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

		public IRepository<Address, int> Addresses => GetRepository<Address, int>();

		public IRepository<FoodRelationship, int> FoodRelationships => GetRepository<FoodRelationship, int>();

		public IRepository<Food, int> Foods => GetRepository<Food, int>();

		public IRepository<GroupMember, int> GroupMembers => GetRepository<GroupMember, int>();

		public IRepository<Group, int> Groups => GetRepository<Group, int>();

		public IRepository<LogEvent, int> LogEvents => GetRepository<LogEvent, int>();

		public IRepository<Person, int> People => GetRepository<Person, int>();

		#endregion
	}
}