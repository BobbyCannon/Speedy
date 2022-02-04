#region References

using System;
using System.Collections.Generic;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.UnitTests.Factories
{
	public static class DataFactory
	{
		#region Methods

		public static Account GetAccount(Action<Account> update = null)
		{
			var time = TimeService.UtcNow;
			var result = new Account
			{
				Id = default,
				EmailAddress = "john@domain.com",
				Name = "John Doe",
				Roles = new [] { "Administrator" },
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetChangeTracking();

			return result;
		}

		public static Address GetAddress(Action<Address> update = null, string line1 = null, string postal = null, string state = null)
		{
			var time = TimeService.UtcNow;
			var result = new Address
			{
				City = "City",
				Id = default,
				Line1 = line1 ?? "Line1",
				Line2 = "Line2",
				Postal = postal ?? "12345",
				State = state ?? "SC",
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);
			result.ResetChangeTracking();

			return result;
		}

		#endregion
	}
}