#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.Client;

#endregion

namespace Speedy.UnitTests.Factories
{
	[ExcludeFromCodeCoverage]
	public static class ClientFactory
	{
		#region Methods

		public static ClientAddress GetClientAddress(Action<ClientAddress> update = null, string line1 = null, string postal = null, string state = null)
		{
			var time = TimeService.UtcNow;
			var result = new ClientAddress
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