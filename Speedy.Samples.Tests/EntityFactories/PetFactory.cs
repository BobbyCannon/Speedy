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
				Name = Guid.NewGuid().ToString(),
				Owner = PersonFactory.Get(),
				CreatedOn = default(DateTime),
				ModifiedOn = default(DateTime),
				Type = PetTypeFactory.Get()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}