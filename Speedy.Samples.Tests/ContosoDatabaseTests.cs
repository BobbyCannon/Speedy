#region References

using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.EntityFramework;
using Speedy.Samples.Tests.EntityFactories;

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
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Addresses.Add(AddressFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddFoodRelationshipTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.FoodRelationships.Add(FoodRelationshipFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddFoodTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Food.Add(FoodFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddGroupMemberTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.GroupMembers.Add(GroupMemberFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddGroupTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Groups.Add(GroupFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddLogEventTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.LogEvents.Add(LogEventFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPersonTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.People.Add(PersonFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPetTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Pets.Add(PetFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPetTypeTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.PetTypes.Add(PetTypeFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddSyncTombstoneTest()
		{
			foreach (var database in new IContosoDatabase[] { new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.SyncTombstones.Add(SyncTombstoneFactory.Get());
					SaveDatabase(database);
				}
			}
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<ContosoDatabase>());
		}

		private static void ProcessException(DbEntityValidationException ex)
		{
			foreach (var error in ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors))
			{
				Console.WriteLine(error.PropertyName + " => " + error.ErrorMessage);
			}
		}

		private void SaveDatabase(IDatabase database)
		{
			try
			{
				database.SaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				ProcessException(ex);
				throw;
			}
		}

		#endregion
	}
}