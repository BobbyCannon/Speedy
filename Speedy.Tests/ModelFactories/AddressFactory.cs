#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Client.Samples.Models;

#endregion

namespace Speedy.Tests.ModelFactories
{
	[ExcludeFromCodeCoverage]
	public static class AddressFactory
	{
		#region Methods

		public static Address Get(Action<Address> update = null, string line1 = null, string postal = null, string state = null)
		{
			var time = TimeService.UtcNow;
			var result = new Address
			{
				City = Guid.NewGuid().ToString(),
				Id = default,
				Line1 = line1 ?? Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
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