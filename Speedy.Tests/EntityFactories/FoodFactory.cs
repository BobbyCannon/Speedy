#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class FoodFactory
	{
		#region Methods

		public static FoodEntity Get(Action<FoodEntity> update = null)
		{
			var result = new FoodEntity
			{
				Id = default,
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}