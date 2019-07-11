#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PersonFactory
	{
		#region Methods

		public static PersonEntity Get(Action<PersonEntity> update = null, string name = null, AddressEntity address = null)
		{
			var result = new PersonEntity
			{
				Address = address ?? AddressFactory.Get(),
				AddressSyncId = default,
				BillingAddressId = null,
				BillingAddressSyncId = null,
				Id = default,
				Name = name ?? Guid.NewGuid().ToString(),
				SyncId = Guid.NewGuid()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}