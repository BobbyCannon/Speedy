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