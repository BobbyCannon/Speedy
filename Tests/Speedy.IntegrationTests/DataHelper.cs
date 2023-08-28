#region References

using System;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests
{
	public static class DataHelper
	{
		#region Methods

		public static AccountEntity NewAccount(string name, AddressEntity address, Guid? syncId = null)
		{
			var time = TimeService.UtcNow;
			return new AccountEntity
			{
				Address = address,
				Name = name,
				SyncId = syncId ?? Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		public static AddressEntity NewAddress(string line1, string line2 = null, Guid? syncId = null)
		{
			var time = TimeService.UtcNow;
			return new AddressEntity
			{
				Line1 = line1,
				Line2 = line2 ?? Guid.NewGuid().ToString(),
				City = Guid.NewGuid().ToString(),
				Postal = Guid.NewGuid().ToString(),
				State = Guid.NewGuid().ToString(),
				SyncId = syncId ?? Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		public static SettingEntity NewSetting(string name, string value)
		{
			var time = TimeService.UtcNow;
			return new SettingEntity
			{
				Name = name,
				Value = value,
				SyncId = Guid.NewGuid(),
				CreatedOn = time,
				ModifiedOn = time
			};
		}

		#endregion
	}
}