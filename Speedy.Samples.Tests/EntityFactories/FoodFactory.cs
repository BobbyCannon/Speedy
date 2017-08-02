#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class FoodFactory
	{
		#region Methods

		public static Food Get(Action<Food> update = null)
		{
			var result = new Food
			{
				Id = default(int),
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}