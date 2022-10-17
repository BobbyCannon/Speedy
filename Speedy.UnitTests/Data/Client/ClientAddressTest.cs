#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests.Sync;
using Speedy.Data.Client;

#endregion

namespace Speedy.UnitTests.Data.Client;

[TestClass]
public class ClientAddressTest : SyncEntityTest<ClientAddress, long>
{
	#region Methods

	[TestMethod]
	public void ValidateExclusions()
	{
		// true means the member is excluded.
		var expected = new Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)>
		{
			{ "Accounts", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "City", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "CreatedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Id", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "IsDeleted", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "LastClientUpdate", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "Line1", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Line2", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "ModifiedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Postal", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "State", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "SyncId", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) }
		};
		ValidateExclusions(expected, false);
	}

	#endregion
}