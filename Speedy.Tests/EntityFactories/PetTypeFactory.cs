#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PetTypeFactory
	{
		#region Methods

		public static PetTypeEntity Get(Action<PetTypeEntity> update = null)
		{
			var result = new PetTypeEntity
			{
				Id = Guid.NewGuid().ToString().Substring(0, 25),
				Type = null
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}