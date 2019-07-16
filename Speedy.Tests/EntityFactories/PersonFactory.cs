#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class PersonFactory
	{
		#region Methods

		public static PersonEntity Get(Action<PersonEntity> update = null, string name = null, AddressEntity address = null)
		{
			var time = TimeService.UtcNow;
			var personAddress = address ?? AddressFactory.Get();
			
			var result = new PersonEntity
			{
				Address = personAddress,
				AddressSyncId = personAddress.SyncId,
				Id = default,
				Name = name ?? Guid.NewGuid().ToString(),
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