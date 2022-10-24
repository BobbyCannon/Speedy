#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.UnitTests;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	[TestClass]
	public class EntityTests
	{
		#region Methods

		[TestMethod]
		public void UnwrapCustomShouldWork()
		{
			var expected = new AccountEntity
			{
				Id = 99,
				SyncId = Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"),
				IsDeleted = true,
				CreatedOn = new DateTime(2019, 8, 4),
				ModifiedOn = new DateTime(2019, 8, 5),
				AddressId = 98,
				AddressSyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"),
				Address = new AddressEntity { Id = 98, SyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D") },
				Name = "John",
				EmailAddress = "john@domain.com",
				ExternalId = "j-123",
				LastLoginDate = new DateTime(2021, 08, 10, 08, 59, 45, DateTimeKind.Utc),
				Nickname = "nick",
				PasswordHash = "hash",
				Roles = ",roles,"
			};

			var actual = (AccountEntity) expected.Unwrap();
			TestHelper.AreEqual(expected, actual, nameof(AccountEntity.Address));
			Assert.AreEqual(null, actual.Address);
			Assert.AreNotEqual(null, actual.Groups);
			Assert.AreEqual(0, actual.Groups.Count);
			Assert.AreNotEqual(null, actual.Pets);
			Assert.AreEqual(0, actual.Pets.Count);
		}

		[TestMethod]
		public void UnwrapShouldIgnoreVirtuals()
		{
			TestHelper.GetDataContexts()
				.ForEach(provider =>
				{
					FoodEntity food;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						food = new FoodEntity { Name = "Bourbon Reduction", ChildRelationships = new[] { new FoodRelationshipEntity { Child = new FoodEntity { Name = "Bourbon" }, Quantity = 2 } } };

						database.Food.Add(food);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var entity = database.Food.Including(x => x.ChildRelationships).First(x => x.Name == food.Name);
						var actual = entity.Unwrap();
						var expected = new FoodEntity { Name = "Bourbon Reduction" };

						TestHelper.AreEqual(expected, actual, true, nameof(FoodEntity.Id), nameof(FoodEntity.CreatedOn), nameof(FoodEntity.ModifiedOn));
					}
				});
		}

		[TestMethod]
		public void UpdateWithShouldExcludeAllSyncItems()
		{
			var expected = new AccountEntity
			{
				Id = 99,
				SyncId = Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"),
				IsDeleted = true,
				CreatedOn = new DateTime(2019, 8, 4),
				ModifiedOn = new DateTime(2019, 8, 5),
				AddressId = 98,
				AddressSyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"),
				Address = new AddressEntity { Id = 98, SyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D") },
				Name = "John",
				EmailAddress = "john@domain.com",
				ExternalId = "j-123",
				LastLoginDate = new DateTime(2021, 08, 10, 08, 59, 45, DateTimeKind.Utc),
				Nickname = "nick",
				PasswordHash = "hash",
				Roles = ",roles,",
				Groups = new[] { new GroupMemberEntity() },
				Pets = new[] { new PetEntity() }
			};

			var actual = new AccountEntity();
			actual.UpdateWith(expected, true, true, true);

			TestHelper.AreEqual(expected, actual,
				nameof(AccountEntity.Address),
				nameof(AccountEntity.AddressId),
				nameof(AccountEntity.Groups),
				nameof(AccountEntity.Id),
				nameof(AccountEntity.IsDeleted),
				nameof(AccountEntity.LastLoginDate),
				nameof(AccountEntity.PasswordHash),
				nameof(AccountEntity.Pets),
				nameof(AccountEntity.Roles),
				nameof(AccountEntity.SyncId)
			);

			Assert.AreEqual(null, actual.Address);
			Assert.AreNotEqual(null, actual.Groups);
			Assert.AreEqual(0, actual.Groups.Count);
			Assert.AreNotEqual(null, actual.Pets);
			Assert.AreEqual(0, actual.Pets.Count);
		}

		[TestMethod]
		public void UpdateWithShouldExcludeVirtuals()
		{
			var expected = new AccountEntity
			{
				Id = 99,
				SyncId = Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"),
				IsDeleted = true,
				CreatedOn = new DateTime(2019, 8, 4),
				ModifiedOn = new DateTime(2019, 8, 5),
				AddressId = 98,
				AddressSyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"),
				Address = new AddressEntity { Id = 98, SyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D") },
				Name = "John",
				EmailAddress = "john@domain.com",
				ExternalId = "j-123",
				LastLoginDate = new DateTime(2021, 08, 10, 08, 59, 45, DateTimeKind.Utc),
				Nickname = "nick",
				PasswordHash = "hash",
				Roles = ",roles,",
				Groups = new[] { new GroupMemberEntity() },
				Pets = new[] { new PetEntity() }
			};

			var actual = new AccountEntity();
			actual.UpdateWith(expected, true);

			TestHelper.AreEqual(expected, actual,
				nameof(AccountEntity.Address),
				nameof(AccountEntity.Groups),
				nameof(AccountEntity.Pets)
			);

			Assert.AreEqual(null, actual.Address);
			Assert.AreNotEqual(null, actual.Groups);
			Assert.AreEqual(0, actual.Groups.Count);
			Assert.AreNotEqual(null, actual.Pets);
			Assert.AreEqual(0, actual.Pets.Count);
		}

		[TestMethod]
		public void UpdateWithShouldIncludeVirtuals()
		{
			var expected = new AccountEntity
			{
				Id = 99,
				SyncId = Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"),
				IsDeleted = true,
				CreatedOn = new DateTime(2019, 8, 4),
				ModifiedOn = new DateTime(2019, 8, 5),
				AddressId = 98,
				AddressSyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"),
				Address = new AddressEntity { Id = 98, SyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D") },
				Name = "John",
				EmailAddress = "john@domain.com",
				ExternalId = "j-123",
				LastLoginDate = new DateTime(2021, 08, 10, 08, 59, 45, DateTimeKind.Utc),
				Nickname = "nick",
				PasswordHash = "hash",
				Roles = ",roles,",
				Groups = new[] { new GroupMemberEntity() },
				Pets = new[] { new PetEntity() }
			};

			var actual = new AccountEntity();
			actual.UpdateWith(expected);

			TestHelper.AreEqual(expected, actual);
			Assert.AreNotEqual(null, actual.Address);
			Assert.AreNotEqual(null, actual.Groups);
			Assert.AreEqual(1, actual.Groups.Count);
			Assert.AreNotEqual(null, actual.Pets);
			Assert.AreEqual(1, actual.Pets.Count);
		}

		#endregion
	}
}