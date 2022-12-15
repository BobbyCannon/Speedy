#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net;

[TestClass]
public class TokenCredentialTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void Reset()
	{
		var credential = new TokenCredential("password");
		IsTrue(credential.HasCredentials());

		credential.Reset();
		IsFalse(credential.HasCredentials());
	}

	[TestMethod]
	public void ToAndFromHeaderValue()
	{
		var credential = new TokenCredential("password");
		var headerValue = credential.GetAuthenticationHeaderValue();
		AreEqual("Bearer", headerValue.Scheme);
		AreEqual("cGFzc3dvcmQ=", headerValue.Parameter);

		var token = headerValue.Parameter.FromBase64String();
		AreEqual("password", token);

		var actualCredential = TokenCredential.FromAuthenticationHeaderValue(headerValue);
		AreEqual(credential, actualCredential);
		AreEqual("password", actualCredential.Password);
	}

	[TestMethod]
	public void UpdateWith()
	{
		var credential = new TokenCredential("password");
		IsTrue(credential.HasCredentials());

		var actual = new TokenCredential();
		AreNotEqual(credential, actual);
		AreEqual(string.Empty, actual.UserName);
		AreEqual(string.Empty, actual.Password);

		IsTrue(actual.UpdateWith(credential));
		AreEqual(credential, actual);
		AreEqual(string.Empty, credential.UserName);
		AreEqual("password", credential.Password);
	}

	#endregion
}