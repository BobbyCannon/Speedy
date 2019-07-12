#region References

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Sql;
using Speedy.Tests.EntityFactories;

#endregion

namespace Speedy.Samples.Tests
{
	[TestClass]
	public class ContosoDatabaseTests
	{
		#region Methods

		[TestMethod]
		public void AddAddressTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Addresses.Add(AddressFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddFoodRelationshipTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.FoodRelationships.Add(FoodRelationshipFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddFoodTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Food.Add(FoodFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddGroupMemberTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.GroupMembers.Add(GroupMemberFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddGroupTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Groups.Add(GroupFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddLogEventTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.LogEvents.Add(LogEventFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddPersonTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.People.Add(PersonFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddPetTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Pets.Add(PetFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[TestMethod]
		public void AddPetTypeTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoSqlDatabase(GetOptions()), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.PetTypes.Add(PetTypeFactory.Get());
					database.SaveChanges();
				}
			}
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			using (var database = new ContosoSqlDatabase(GetOptions()))
			{
				database.Database.Migrate();
			}
		}

		private static DbContextOptions<ContosoSqlDatabase> GetOptions()
		{
			return new DbContextOptionsBuilder<ContosoSqlDatabase>().UseSqlServer(TestHelper.DefaultSqlConnection, ContosoDatabase.UpdateOptions).Options;
		}

		#endregion
	}
}