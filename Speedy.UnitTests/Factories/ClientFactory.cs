#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Speedy.Data.Client;

#endregion

namespace Speedy.UnitTests.Factories
{
	[ExcludeFromCodeCoverage]
	public static class ClientFactory
	{
		#region Methods

		public static ClientAccount GetClientAccount(string name, ClientAddress address, Action<ClientAccount> update = null)
		{
			var result = new ClientAccount
			{
				Address = address,
				EmailAddress = name + "@speedy.local",
				Name = name,
				SyncId = Guid.NewGuid(),
			};

			update?.Invoke(result);

			return result;
		}

		public static ClientAddress GetClientAddress(string line1 = null, string postal = null, string state = null, Action<ClientAddress> update = null)
		{
			var result = new ClientAddress
			{
				City = Guid.NewGuid().ToString(),
				Id = default,
				Line1 = line1 ?? Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				Postal = postal ?? Guid.NewGuid().ToString(),
				State = state ?? Guid.NewGuid().ToString(),
				SyncId = Guid.NewGuid(),
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}