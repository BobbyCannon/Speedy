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

		public static Person Get(Action<Person> update = null)
		{
			var result = new Person
			{
				Id = default(int),
				Address = AddressFactory.Get(),
				AddressSyncId = default(Guid),
				BillingAddressId = null,
				BillingAddressSyncId = null,
				Name = Guid.NewGuid().ToString(),
				SyncId = default(Guid)
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}