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