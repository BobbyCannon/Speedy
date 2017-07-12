#region References

using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Samples.EntityFramework;
using Speedy;

#endregion

namespace Speedy.Samples.Tests
{
	[TestClass]
	public class ContosoDatabaseTests
	{
		#region Methods

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			System.Data.Entity.Database.SetInitializer<ContosoDatabase>(new CreateDatabaseIfNotExists<ContosoDatabase>());
		}

		[TestMethod]
		public void AddAddressTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Addresses.Add(GetAddress());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddFoodRelationshipTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.FoodRelationships.Add(GetFoodRelationship());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddFoodTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Foods.Add(GetFood());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddGroupMemberTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.GroupMembers.Add(GetGroupMember());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddGroupTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Groups.Add(GetGroup());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddLogEventTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.LogEvents.Add(GetLogEvent());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPersonTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.People.Add(GetPerson());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPetTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.Pets.Add(GetPet());
					SaveDatabase(database);
				}
			}
		}

		[TestMethod]
		public void AddPetTypeTest()
		{
			foreach (var database in new IContosoDatabase[]{ new ContosoDatabase(), new ContosoMemoryDatabase() })
			{
				using (database)
				{
					database.PetTypes.Add(GetPetType());
					SaveDatabase(database);
				}
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
		public static Speedy.Samples.Entities.Address GetAddress()
		{
			return new Speedy.Samples.Entities.Address
			{
				City = Guid.NewGuid().ToString(),
				Line1 = Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				LinkedAddressId = null,
				LinkedAddressSyncId = null,
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString()
			};
		}

		public static Speedy.Samples.Entities.FoodRelationship GetFoodRelationship()
		{
			return new Speedy.Samples.Entities.FoodRelationship
			{
				Child = GetFood(),
				Parent = GetFood(),
				Quantity = default(decimal)
			};
		}

		public static Speedy.Samples.Entities.Food GetFood()
		{
			return new Speedy.Samples.Entities.Food
			{
				Name = Guid.NewGuid().ToString()
			};
		}

		public static Speedy.Samples.Entities.GroupMember GetGroupMember()
		{
			return new Speedy.Samples.Entities.GroupMember
			{
				Group = GetGroup(),
				GroupSyncId = default(Guid),
				Member = GetPerson(),
				MemberSyncId = default(Guid),
				Role = Guid.NewGuid().ToString()
			};
		}

		public static Speedy.Samples.Entities.Group GetGroup()
		{
			return new Speedy.Samples.Entities.Group
			{
				Description = Guid.NewGuid().ToString(),
				Name = Guid.NewGuid().ToString()
			};
		}

		public static Speedy.Samples.Entities.LogEvent GetLogEvent()
		{
			return new Speedy.Samples.Entities.LogEvent
			{
				Id = Guid.NewGuid().ToString(),
				Message = null
			};
		}

		public static Speedy.Samples.Entities.Person GetPerson()
		{
			return new Speedy.Samples.Entities.Person
			{
				Address = GetAddress(),
				AddressSyncId = default(Guid),
				BillingAddressId = null,
				BillingAddressSyncId = null,
				Name = Guid.NewGuid().ToString()
			};
		}

		public static Speedy.Samples.Entities.Pet GetPet()
		{
			return new Speedy.Samples.Entities.Pet
			{
				Name = Guid.NewGuid().ToString(),
				Owner = GetPerson(),
				CreatedOn = default(DateTime),
				ModifiedOn = default(DateTime),
				Type = GetPetType()
			};
		}

		public static Speedy.Samples.Entities.PetType GetPetType()
		{
			return new Speedy.Samples.Entities.PetType
			{
				Id = Guid.NewGuid().ToString().Substring(0, 25),
				Type = null
			};
		}

		private static void ProcessException(DbEntityValidationException ex)
		{
			foreach (var error in ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors))
			{
				Console.WriteLine(error.PropertyName + " => " + error.ErrorMessage);
			}
		}

		#endregion
	}
}
