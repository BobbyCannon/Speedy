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
					Food food;

					using (var database = provider.GetDatabase())
					{
						Console.WriteLine(database.GetType().Name);
						food = new Food { Name = "Bourbon Reduction", ChildRelationships = new[] { new FoodRelationship { Child = new Food { Name = "Bourbon" }, Quantity = 2 } } };

						database.Food.Add(food);
						database.SaveChanges();
					}

					using (var database = provider.GetDatabase())
					{
						var actual = database.Food.Including(x => x.ChildRelationships).First(x => x.Name == food.Name);
						var expected = new Food { Name = "Bourbon Reduction" };

						TestHelper.AreEqual(expected, actual.Unwrap(), true, nameof(Food.Id), nameof(Food.CreatedOn), nameof(Food.ModifiedOn));
					}
				});
		}

		#endregion
	}
}