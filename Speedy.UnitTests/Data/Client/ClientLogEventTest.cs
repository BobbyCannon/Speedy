#region References

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Automation.Tests.Sync;
using Speedy.Data.Client;

#endregion

namespace Speedy.UnitTests.Data.Client;

[TestClass]
public class ClientLogEventTest : SyncEntityTest<ClientLogEvent, long>
{
	#region Methods

	[TestMethod]
	public void ValidateExclusions()
	{
		// true means the member is excluded.
		var expected = new Dictionary<string, (bool incoming, bool outgoing, bool syncUpdate, bool changeTracking)>
		{
			{ "CreatedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Id", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "IsDeleted", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "LastClientUpdate", (incoming: true, outgoing: true, syncUpdate: true, changeTracking: false) },
			{ "Level", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "Message", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "ModifiedOn", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) },
			{ "SyncId", (incoming: false, outgoing: false, syncUpdate: false, changeTracking: false) }
		};
		ValidateExclusions(expected, false);
	}

	#endregion
}