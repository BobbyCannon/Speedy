#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PetFactory
	{
		#region Methods

		public static PetEntity Get(Action<PetEntity> update = null, PersonEntity person = null)
		{
			var time = TimeService.UtcNow;
			var petPerson = person ?? PersonFactory.Get(null, "John");

			var result = new PetEntity
			{
				Name = Guid.NewGuid().ToString(),
				Owner = petPerson,
				Type = PetTypeFactory.Get(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}