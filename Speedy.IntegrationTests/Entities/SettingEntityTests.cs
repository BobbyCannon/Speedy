#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.IntegrationTests.Entities
{
	[TestClass]
	public class SettingEntityTests : BaseEntityTests<SettingEntity>
	{
		#region Methods

		/// <summary>
		/// We want to make sure these never change, when they do it should be very deliberate
		/// </summary>
		[TestMethod]
		public void SyncExclusions()
		{
			var entity = GetModelWithNonDefaultValues();
			var expected = new Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)>
			{
				{ "Id", (true, true, true, false) },
				{ "Name", (false, false, false, false) },
				{ "Value", (false, false, false, false) },
				{ "CreatedOn", (false, false, false, false) },
				{ "IsDeleted", (false, false, true, false) },
				{ "ModifiedOn", (false, false, false, false) },
				{ "SyncId", (false, false, false, false) }
			};

			ValidateExclusions(entity, expected, false);
		}

		#endregion
	}
}