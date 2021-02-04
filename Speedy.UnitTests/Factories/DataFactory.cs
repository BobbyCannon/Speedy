#region References

using System;
using Speedy.Data.WebApi;

#endregion

namespace Speedy.UnitTests.Factories
{
	public static class DataFactory
	{
		#region Methods

		public static Address GetAddress(Action<Address> update = null, string line1 = null, string postal = null, string state = null)
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
				ModifiedOn = time,
				IsDeleted = false
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}