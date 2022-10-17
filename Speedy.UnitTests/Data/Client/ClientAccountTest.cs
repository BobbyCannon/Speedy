#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests.Sync;
using Speedy.Data.Client;

#endregion

namespace Speedy.UnitTests.Data.Client;

[TestClass]
public class ClientAccountTest : SyncEntityTest<ClientAccount, int>
{
	#region Methods

	[TestMethod]
	public void ValidateExclusions()
	{
		// true means the member is excluded.
		var expected = new Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)>
		{
			{ "Address", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "AddressId", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "AddressSyncId", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "CreatedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "EmailAddress", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Id", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "IsDeleted", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "LastClientUpdate", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "ModifiedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Name", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Roles", (incoming: true, outgoing: false, syncUpdate: true, changeTracking: false) },
			{ "SyncId", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) }
		};
		ValidateExclusions(expected, false);
	}

	#endregion
}