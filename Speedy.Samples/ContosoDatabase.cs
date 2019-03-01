#region References

using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Speedy.EntityFramework;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples
{
	public class ContosoDatabase : EntityFrameworkDatabase, IContosoDatabase
	{
		#region Constructors

		public ContosoDatabase()
		{
			// Default constructor needed for Add-Migration
		}

		public ContosoDatabase(DbContextOptions<ContosoDatabase> options)
			: this(options, null)
		{
		}

		public ContosoDatabase(DbContextOptions contextOptions, DatabaseOptions options)
			: base(contextOptions, options)
		{
			Options.SyncOrder = new[] { typeof(Address).FullName, typeof(Person).FullName };
		}

		#endregion

		#region Properties

		public IRepository<Address, int> Addresses => GetSyncableRepository<Address>();
		public IRepository<Food, int> Food => GetRepository<Food, int>();
		public IRepository<FoodRelationship, int> FoodRelationships => GetRepository<FoodRelationship, int>();
		public IRepository<GroupMember, int> GroupMembers => GetRepository<GroupMember, int>();
		public IRepository<Group, int> Groups => GetRepository<Group, int>();
		public IRepository<LogEvent, string> LogEvents => GetRepository<LogEvent, string>();
		public IRepository<Person, int> People => GetSyncableRepository<Person>();
		public IRepository<Pet, Pet.PetKey> Pets => GetRepository<Pet, Pet.PetKey>();
		public IRepository<PetType, string> PetTypes => GetRepository<PetType, string>();

		#endregion

		#region Methods

		public static ContosoDatabase UseSql(string connectionString)
		{
			var builder = new DbContextOptionsBuilder<ContosoDatabase>();
			return new ContosoDatabase(builder.UseSqlServer(connectionString, UpdateOptions).Options);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
				optionsBuilder.UseSqlServer(connectionString, UpdateOptions);
			}

			optionsBuilder.UseLazyLoadingProxies();

			base.OnConfiguring(optionsBuilder);
		}

		#endregion
	}
}