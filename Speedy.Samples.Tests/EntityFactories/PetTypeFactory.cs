#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PetTypeFactory
	{
		#region Methods

		public static PetType Get(Action<PetType> update = null)
		{
			var result = new PetType
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