#region References

using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;
using Speedy.Samples.EntityFramework;
using Speedy.Sync;

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
					database.Addresses.Add(GetAddress());
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
					database.FoodRelationships.Add(GetFoodRelationship());
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
					database.Food.Add(GetFood());
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
					database.GroupMembers.Add(GetGroupMember());
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
					database.Groups.Add(GetGroup());
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
					database.LogEvents.Add(GetLogEvent());
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
					database.People.Add(GetPerson());
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
					database.Pets.Add(GetPet());
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
					database.PetTypes.Add(GetPetType());
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
					database.SyncTombstones.Add(GetSyncTombstone());
					SaveDatabase(database);
				}
			}
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext context)
		{
			System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<ContosoDatabase>());
		}

		public static Address GetAddress()
		{
			return new Address
			{
				Id = default(int),
				City = Guid.NewGuid().ToString(),
				Line1 = Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				LinkedAddressId = null,
				LinkedAddressSyncId = null,
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString(),
				SyncId = default(Guid)
			};
		}

		public static Food GetFood()
		{
			return new Food
			{
				Id = default(int),
				Name = Guid.NewGuid().ToString()
			};
		}

		public static FoodRelationship GetFoodRelationship()
		{
			return new FoodRelationship
			{
				Id = default(int),
				Child = GetFood(),
				Parent = GetFood(),
				Quantity = default(decimal)
			};
		}

		public static Group GetGroup()
		{
			return new Group
			{
				Id = default(int),
				Description = Guid.NewGuid().ToString(),
				Name = Guid.NewGuid().ToString()
			};
		}

		public static GroupMember GetGroupMember()
		{
			return new GroupMember
			{
				Id = default(int),
				Group = GetGroup(),
				GroupSyncId = default(Guid),
				Member = GetPerson(),
				MemberSyncId = default(Guid),
				Role = Guid.NewGuid().ToString()
			};
		}

		public static LogEvent GetLogEvent()
		{
			return new LogEvent
			{
				Id = Guid.NewGuid().ToString(),
				Message = null
			};
		}

		public static Person GetPerson()
		{
			return new Person
			{
				Id = default(int),
				Address = GetAddress(),
				AddressSyncId = default(Guid),
				BillingAddressId = null,
				BillingAddressSyncId = null,
				Name = Guid.NewGuid().ToString(),
				SyncId = default(Guid)
			};
		}

		public static Pet GetPet()
		{
			return new Pet
			{
				Name = Guid.NewGuid().ToString(),
				Owner = GetPerson(),
				CreatedOn = default(DateTime),
				ModifiedOn = default(DateTime),
				Type = GetPetType()
			};
		}

		public static PetType GetPetType()
		{
			return new PetType
			{
				Id = Guid.NewGuid().ToString().Substring(0, 25),
				Type = null
			};
		}

		public static SyncTombstone GetSyncTombstone()
		{
			return new SyncTombstone
			{
				Id = default(long),
				ReferenceId = Guid.NewGuid().ToString(),
				SyncId = default(Guid),
				TypeName = Guid.NewGuid().ToString()
			};
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