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

		public static PetEntity Get(Action<PetEntity> update = null)
		{
			var result = new PetEntity
			{
				CreatedOn = default,
				ModifiedOn = default,
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