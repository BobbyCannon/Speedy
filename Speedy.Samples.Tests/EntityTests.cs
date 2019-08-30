#region References

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests
{
	[TestClass]
	public class EntityTests
	{
		#region Methods

		[TestMethod]
		public void EntitiesOfDifferentTypesShouldStoreDifferentUpdateProperties()
		{
			var address = new AddressEntity();
			address.ResetPropertyChangeTrackingExclusions(false);
			address.ExcludePropertiesForChangeTracking(nameof(AddressEntity.City));

			var person = new PersonEntity();
			person.ResetPropertyChangeTrackingExclusions(false);
			person.ExcludePropertiesForChangeTracking(nameof(PersonEntity.Name));

			var actual1 = address.GetExcludedPropertiesForChangeTracking().ToList();
			var actual2 = person.GetExcludedPropertiesForChangeTracking().ToList();

			Assert.AreEqual(1, actual1.Count);
			Assert.AreEqual(1, actual2.Count);
			Assert.AreNotEqual(actual1[0], actual2[0]);

			Assert.AreEqual(nameof(AddressEntity.City), actual1[0]);
			Assert.AreEqual(nameof(PersonEntity.Name), actual2[0]);
		}

		[TestMethod]
		public void EntitiesOfSameTypesExclusionCollectionsShouldNotInterfereWithOthers()
		{
			var address1 = new AddressEntity();
			address1.ResetPropertyChangeTrackingExclusions(false);
			address1.ResetPropertySyncExclusions(false);
			address1.ResetPropertyUpdateExclusions(false);
			address1.ExcludePropertiesForChangeTracking(nameof(AddressEntity.IsDeleted));

			var actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			var actual2 = address1.GetExcludedPropertiesForSync().ToList();
			var actual3 = address1.GetExcludedPropertiesForUpdate().ToList();

			Assert.AreEqual(1, actual1.Count);
			Assert.AreEqual(nameof(AddressEntity.IsDeleted), actual1[0]);
			Assert.AreEqual(0, actual2.Count);
			Assert.AreEqual(0, actual3.Count);

			address1.ResetPropertyChangeTrackingExclusions(false);
			address1.ResetPropertySyncExclusions(false);
			address1.ResetPropertyUpdateExclusions(false);
			address1.ExcludePropertiesForSync(nameof(AddressEntity.City));

			actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			actual2 = address1.GetExcludedPropertiesForSync().ToList();
			actual3 = address1.GetExcludedPropertiesForUpdate().ToList();

			Assert.AreEqual(0, actual1.Count);
			Assert.AreEqual(1, actual2.Count);
			Assert.AreEqual(nameof(AddressEntity.City), actual2[0]);
			Assert.AreEqual(0, actual3.Count);
			
			address1.ResetPropertyChangeTrackingExclusions(false);
			address1.ResetPropertySyncExclusions(false);
			address1.ResetPropertyUpdateExclusions(false);
			address1.ExcludePropertiesForUpdate(nameof(AddressEntity.Line1));

			actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			actual2 = address1.GetExcludedPropertiesForSync().ToList();
			actual3 = address1.GetExcludedPropertiesForUpdate().ToList();

			Assert.AreEqual(0, actual1.Count);
			Assert.AreEqual(0, actual2.Count);
			Assert.AreEqual(1, actual3.Count);
			Assert.AreEqual(nameof(AddressEntity.Line1), actual3[0]);
		}

		[TestMethod]
		public void EntitiesOfSameTypesShouldShareSameExclusionCollectionForChangeTracking()
		{
			var address1 = new AddressEntity();
			var address2 = new AddressEntity();

			address1.ExcludePropertiesForChangeTracking(nameof(AddressEntity.IsDeleted));

			var actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			var actual2 = address2.GetExcludedPropertiesForChangeTracking().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(1, actual1.Count);
			Assert.AreEqual(1, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);

			address1.ExcludePropertiesForChangeTracking(nameof(AddressEntity.City));

			actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			actual2 = address2.GetExcludedPropertiesForChangeTracking().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(2, actual1.Count);
			Assert.AreEqual(2, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);
			Assert.AreEqual(actual1[1], actual2[1]);
			Assert.AreEqual(nameof(AddressEntity.IsDeleted), actual1[0]);
			Assert.AreEqual(nameof(AddressEntity.City), actual1[1]);

			address1.ResetPropertyChangeTrackingExclusions(false);

			actual1 = address1.GetExcludedPropertiesForChangeTracking().ToList();
			actual2 = address2.GetExcludedPropertiesForChangeTracking().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(0, actual1.Count);
			Assert.AreEqual(0, actual2.Count);
		}

		[TestMethod]
		public void EntitiesOfSameTypesShouldShareSameExclusionCollectionForSync()
		{
			var address1 = new AddressEntity();
			var address2 = new AddressEntity();
			address1.ResetPropertySyncExclusions(false);

			address1.ExcludePropertiesForSync(nameof(AddressEntity.IsDeleted));

			var actual1 = address1.GetExcludedPropertiesForSync().ToList();
			var actual2 = address2.GetExcludedPropertiesForSync().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(1, actual1.Count);
			Assert.AreEqual(1, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);

			address1.ExcludePropertiesForSync(nameof(AddressEntity.City));

			actual1 = address1.GetExcludedPropertiesForSync().ToList();
			actual2 = address2.GetExcludedPropertiesForSync().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(2, actual1.Count);
			Assert.AreEqual(2, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);
			Assert.AreEqual(actual1[1], actual2[1]);
			Assert.AreEqual(nameof(AddressEntity.IsDeleted), actual1[0]);
			Assert.AreEqual(nameof(AddressEntity.City), actual1[1]);

			address1.ResetPropertySyncExclusions(false);

			actual1 = address1.GetExcludedPropertiesForSync().ToList();
			actual2 = address2.GetExcludedPropertiesForSync().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(0, actual1.Count);
			Assert.AreEqual(0, actual2.Count);
		}

		[TestMethod]
		public void EntitiesOfSameTypesShouldShareSameExclusionCollectionForUpdate()
		{
			var address1 = new AddressEntity();
			var address2 = new AddressEntity();
			address1.ResetPropertyUpdateExclusions(false);

			address1.ExcludePropertiesForUpdate(nameof(AddressEntity.IsDeleted));

			var actual1 = address1.GetExcludedPropertiesForUpdate().ToList();
			var actual2 = address2.GetExcludedPropertiesForUpdate().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(1, actual1.Count);
			Assert.AreEqual(1, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);

			address1.ExcludePropertiesForUpdate(nameof(AddressEntity.City));

			actual1 = address1.GetExcludedPropertiesForUpdate().ToList();
			actual2 = address2.GetExcludedPropertiesForUpdate().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(2, actual1.Count);
			Assert.AreEqual(2, actual2.Count);
			Assert.AreEqual(actual1[0], actual2[0]);
			Assert.AreEqual(actual1[1], actual2[1]);
			Assert.AreEqual(nameof(AddressEntity.IsDeleted), actual1[0]);
			Assert.AreEqual(nameof(AddressEntity.City), actual1[1]);

			address1.ResetPropertyUpdateExclusions(false);

			actual1 = address1.GetExcludedPropertiesForUpdate().ToList();
			actual2 = address2.GetExcludedPropertiesForUpdate().ToList();

			TestHelper.AreEqual(actual1, actual2);
			Assert.AreEqual(0, actual1.Count);
			Assert.AreEqual(0, actual2.Count);
		}

		[TestMethod]
		public void UnwrapCustomShouldWork()
		{
			var people = new PersonEntity
			{
				Id = 99,
				SyncId = Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"),
				IsDeleted = true,
				CreatedOn = new DateTime(2019, 8, 4),
				ModifiedOn = new DateTime(2019, 8, 5),
				AddressId = 98,
				AddressSyncId = Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"),
				Name = "John"
			};

			var actual = (PersonEntity) people.Unwrap();
			Assert.AreEqual(99, actual.Id);
			Assert.AreEqual(Guid.Parse("BA664BE6-EA39-49F1-8B8F-0965294590BD"), actual.SyncId);
			Assert.AreEqual(true, actual.IsDeleted);
			Assert.AreEqual(new DateTime(2019, 8, 4), actual.CreatedOn);
			Assert.AreEqual(new DateTime(2019, 8, 5), actual.ModifiedOn);
			Assert.AreEqual(98, actual.AddressId);
			Assert.AreEqual(Guid.Parse("09AE3305-211E-48EF-99E0-191E2E1D686D"), actual.AddressSyncId);
			Assert.AreEqual("John", actual.Name);
			Assert.AreEqual(null, actual.Address);
			Assert.AreNotEqual(null, actual.Groups);
			Assert.AreEqual(0, actual.Groups.Count);
			Assert.AreNotEqual(null, actual.Owners);
			Assert.AreEqual(0, actual.Owners.Count);
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

		#endregion
	}
}