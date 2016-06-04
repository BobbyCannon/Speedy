#region References

using System.Diagnostics.CodeAnalysis;
using Speedy.EntityFrameworkCore;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.EntityFrameworkCore
{
	[ExcludeFromCodeCoverage]
	public class EntityFrameworkCoreContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public EntityFrameworkCoreContosoDatabase()
			: this("name=DefaultConnection", new DatabaseOptions())
		{
		}

		public EntityFrameworkCoreContosoDatabase(DatabaseOptions options)
			: this("name=DefaultConnection", options)
		{
		}

		public EntityFrameworkCoreContosoDatabase(string nameOrConnectionString, DatabaseOptions options = null)
			: base(nameOrConnectionString, options)
		{
			Options.SyncOrder = ContosoDatabase.SyncOrder;
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