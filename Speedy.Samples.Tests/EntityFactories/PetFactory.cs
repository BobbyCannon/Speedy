#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PetFactory
	{
		#region Methods

		public static Pet Get(Action<Pet> update = null)
		{
			var result = new Pet
			{
				CreatedOn = default(DateTime),
				ModifiedOn = default(DateTime),
				Name = Guid.NewGuid().ToString(),
				Owner = PersonFactory.Get(),
				Type = PetTypeFactory.Get()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}