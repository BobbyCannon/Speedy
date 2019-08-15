#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public static class AddressEntityFactory
	{
		#region Methods

		public static AddressEntity Get(Action<AddressEntity> update = null, string line1 = null, string postal = null, string state = null)
		{
			var time = TimeService.UtcNow;
			var result = new AddressEntity
			{
				City = Guid.NewGuid().ToString(),
				Id = default,
				Line1 = line1 ?? Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				LinkedAddressId = null,
				LinkedAddressSyncId = null,
				Postal = postal ?? Guid.NewGuid().ToString(),
				State = state ?? Guid.NewGuid().ToString(),
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}