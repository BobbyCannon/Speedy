#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests.Entities
{
	[TestClass]
	public class AccountEntityTests : BaseEntityTests<AccountEntity>
	{
		#region Methods

		[TestMethod]
		public void AllPropertiesSet()
		{
			ValidateAllValuesAreNotDefault(GetModelWithNonDefaultValues(),
				nameof(AccountEntity.Address),
				nameof(AccountEntity.Groups)
			);
		}

		/// <summary>
		/// We want to make sure these never change, when they do it should be very deliberate
		/// </summary>
		[TestMethod]
		public void SyncExclusions()
		{
			var entity = GetModelWithNonDefaultValues();
			var expected = new Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)>
			{
				{ "Address", (true, true, true, false) },
				{ "AddressId", (true, true, true, false) },
				{ "AddressSyncId", (false, false, false, false) },
				{ "EmailAddress", (false, false, false, false) },
				{ "ExternalId", (false, false, false, false) },
				{ "Groups", (true, true, true, false) },
				{ "Id", (true, true, true, false) },
				{ "LastLoginDate", (true, false, true, false) },
				{ "Name", (false, false, false, false) },
				{ "Nickname", (false, false, false, false) },
				{ "PasswordHash", (true, true, true, false) },
				{ "Pets", (true, true, true, false) },
				{ "Roles", (true, false, true, false) },
				{ "CreatedOn", (false, false, false, false) },
				{ "IsDeleted", (false, false, true, false) },
				{ "ModifiedOn", (false, false, false, false) },
				{ "SyncId", (false, false, true, false) }
			};

			ValidateExclusions(entity, expected, false);
		}

		#endregion
	}
}