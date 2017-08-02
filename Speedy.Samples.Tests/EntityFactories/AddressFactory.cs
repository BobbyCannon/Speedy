#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class AddressFactory
	{
		#region Methods

		public static Address Get(Action<Address> update = null)
		{
			var result = new Address
			{
				Id = default(int),
				City = Guid.NewGuid().ToString(),
				Line1 = Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				LinkedAddressId = null,
				LinkedAddressSyncId = null,
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString(),
				SyncId = default(Guid)
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}