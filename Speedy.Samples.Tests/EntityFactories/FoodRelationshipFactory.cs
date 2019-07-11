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

		public static FoodRelationshipEntity Get(Action<FoodRelationshipEntity> update = null)
		{
			var result = new FoodRelationshipEntity
			{
				Child = FoodFactory.Get(),
				Id = default,
				Parent = FoodFactory.Get(),
				Quantity = default
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}