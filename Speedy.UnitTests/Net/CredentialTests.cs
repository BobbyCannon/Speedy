#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net;

[TestClass]
public class CredentialTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void Reset()
	{
		var credential = new Credential("user name", "password");
		IsTrue(credential.HasCredentials());

		credential.Reset();
		IsFalse(credential.HasCredentials());
	}

	[TestMethod]
	public void ToAndFromHeaderValue()
	{
		var credential = new Credential("username", "password");
		var headerValue = credential.GetAuthenticationHeaderValue();
		AreEqual("Basic", headerValue.Scheme);
		AreEqual("dXNlcm5hbWU6cGFzc3dvcmQ=", headerValue.Parameter);

		var token = headerValue.Parameter.FromBase64();
		AreEqual("username:password", token);

		var actualCredential = Credential.FromAuthenticationHeaderValue(headerValue);
		AreEqual(credential, actualCredential);
		AreEqual("username", actualCredential.UserName);
		AreEqual("password", actualCredential.Password);
	}

	[TestMethod]
	public void UpdateWith()
	{
		var credential = new Credential("username", "password");
		IsTrue(credential.HasCredentials());

		var actual = new Credential();
		AreNotEqual(credential, actual);
		AreEqual(string.Empty, actual.UserName);
		AreEqual(string.Empty, actual.Password);

		IsTrue(actual.UpdateWith(credential));
		AreEqual(credential, actual);
		AreEqual("username", actual.UserName);
		AreEqual("password", actual.Password);
	}

	#endregion
}