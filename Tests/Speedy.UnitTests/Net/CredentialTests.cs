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
	public void Dispose()
	{
		var actual = new Credential("username", "password");
		IsTrue(actual.HasCredentials());
		AreEqual("username", actual.UserName);
		AreEqual("password", actual.Password);
		AreEqual(8, actual.SecurePassword.Length);
		AreEqual("password", actual.SecurePassword.ToUnsecureString());

		actual.Dispose();
		AreEqual(string.Empty, actual.UserName);
		AreEqual(string.Empty, actual.Password);
		AreEqual(null, actual.SecurePassword);
	}

	[TestMethod]
	public void ResetCredential()
	{
		using var credential = new Credential("username", "password");
		IsTrue(credential.HasCredentials());
		AreEqual("username", credential.UserName);
		AreEqual("password", credential.Password);
		AreEqual(8, credential.SecurePassword.Length);
		AreEqual("password", credential.SecurePassword.ToUnsecureString());

		credential.Reset();
		IsFalse(credential.HasCredentials());
		AreEqual(string.Empty, credential.UserName);
		AreEqual(string.Empty, credential.Password);
		AreEqual(null, credential.SecurePassword);
	}

	[TestMethod]
	public void ToAndFromHeaderValue()
	{
		using var credential = new Credential("username", "password");
		var headerValue = credential.GetAuthenticationHeaderValue();
		AreEqual("Basic", headerValue.Scheme);
		AreEqual("dXNlcm5hbWU6cGFzc3dvcmQ=", headerValue.Parameter);

		var token = headerValue.Parameter.FromBase64String();
		AreEqual("username:password", token);

		using var actualCredential = WebCredential.FromAuthenticationHeaderValue(headerValue);
		AreEqual(credential, actualCredential);
		AreEqual("username", actualCredential.UserName);
		AreEqual("password", actualCredential.Password);
		AreEqual(8, actualCredential.SecurePassword.Length);
		AreEqual("password", actualCredential.SecurePassword.ToUnsecureString());
	}

	[TestMethod]
	public void UpdateWith()
	{
		using var credential = new Credential("username", "password");
		IsTrue(credential.HasCredentials());

		using var actual = new Credential();
		AreNotEqual(credential, actual);
		AreEqual(string.Empty, actual.UserName);
		AreEqual(string.Empty, actual.Password);
		AreEqual(null, actual.SecurePassword);

		IsTrue(actual.UpdateWith(credential));
		AreEqual(credential, actual);
		AreEqual("username", actual.UserName);
		AreEqual("password", actual.Password);
		AreEqual(8, actual.SecurePassword.Length);
		AreEqual("password", actual.SecurePassword.ToUnsecureString());
	}

	#endregion
}