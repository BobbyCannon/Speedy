#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net;

[TestClass]
public class WebCredentialTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void Dispose()
	{
		var actual = new WebCredential("username", "password");
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
		using var credential = new WebCredential("username", "password") { RememberMe = true };
		IsTrue(credential.HasCredentials());
		AreEqual("username", credential.UserName);
		AreEqual("password", credential.Password);
		AreEqual(8, credential.SecurePassword.Length);
		AreEqual("password", credential.SecurePassword.ToUnsecureString());

		credential.Reset();
		IsFalse(credential.HasCredentials());
		IsFalse(credential.RememberMe);
		AreEqual(string.Empty, credential.UserName);
		AreEqual(string.Empty, credential.Password);
		AreEqual(null, credential.SecurePassword);
	}

	[TestMethod]
	public void ToAndFromHeaderValue()
	{
		using var credential = new WebCredential("UserName", "Password");
		var headerValue = credential.GetAuthenticationHeaderValue();
		AreEqual("Basic", headerValue.Scheme);
		AreEqual("VXNlck5hbWU6UGFzc3dvcmQ=", headerValue.Parameter);

		var token = headerValue.Parameter.FromBase64String();
		AreEqual("UserName:Password", token);

		using var actual = WebCredential.FromAuthenticationHeaderValue(headerValue);
		AreEqual(credential, actual);
		AreEqual("UserName", actual.UserName);
		AreEqual("Password", actual.Password);
	}

	[TestMethod]
	public void UpdateWith()
	{
		using var credential = new WebCredential("UserName", "Password");
		IsTrue(credential.HasCredentials());

		using var actual = new WebCredential();
		AreNotEqual(credential, actual);
		AreEqual(string.Empty, actual.UserName);
		AreEqual(string.Empty, actual.Password);
		AreEqual(0, actual.SecurePassword.Length);

		IsTrue(actual.UpdateWith(credential));
		AreEqual(actual, actual);
		AreEqual("UserName", actual.UserName);
		AreEqual("Password", actual.Password);
		AreEqual(8, actual.SecurePassword.Length);
		AreEqual("Password", actual.SecurePassword.ToUnsecureString());
	}

	#endregion
}