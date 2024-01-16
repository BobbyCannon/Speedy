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
	public void Dispose()
	{
		var actual = new TokenCredential("password");
		IsTrue(actual.HasCredentials());
		AreEqual(string.Empty, actual.UserName);
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
		using var credential = new TokenCredential("password");
		IsTrue(credential.HasCredentials());

		credential.Reset();
		IsFalse(credential.HasCredentials());
	}

	[TestMethod]
	public void ToAndFromHeaderValue()
	{
		using var credential = new TokenCredential("password");
		var headerValue = credential.GetAuthenticationHeaderValue();
		AreEqual("Bearer", headerValue.Scheme);
		AreEqual("cGFzc3dvcmQ=", headerValue.Parameter);

		var token = headerValue.Parameter.FromBase64String();
		AreEqual("password", token);

		using var actualCredential = TokenCredential.FromAuthenticationHeaderValue(headerValue);
		AreEqual(credential, actualCredential);
		AreEqual("password", actualCredential.Password);
	}

	[TestMethod]
	public void UpdateWith()
	{
		using var credential = new TokenCredential("password");
		IsTrue(credential.HasCredentials());

		using var actual = new TokenCredential();
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