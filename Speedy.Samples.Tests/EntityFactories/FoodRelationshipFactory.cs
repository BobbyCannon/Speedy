#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class FoodRelationshipFactory
	{
		#region Methods

		public static FoodRelationship Get(Action<FoodRelationship> update = null)
		{
			var result = new FoodRelationship
			{
				Id = default(int),
				Child = FoodFactory.Get(),
				Parent = FoodFactory.Get(),
				Quantity = default(decimal)
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}