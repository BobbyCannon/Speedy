#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net;

[TestClass]
public class WebCredentialTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void Reset()
	{
		var credential = new WebCredential("username", "password");
		IsTrue(credential.HasCredentials());

		credential.Reset();
		IsFalse(credential.HasCredentials());
	}

	#endregion
}