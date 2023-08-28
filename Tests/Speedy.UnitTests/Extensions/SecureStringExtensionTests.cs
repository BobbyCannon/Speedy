#region References

using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;

#endregion

namespace Speedy.UnitTests.Extensions;

[TestClass]
public class SecureStringExtensionTests : SpeedyUnitTest
{
	#region Methods

	[TestMethod]
	public void AppendSecureStringToAnEmptySecureString()
	{
		using var secureString1 = new SecureString();
		using var secureString2 = "123456".ToSecureString();

		secureString1.Append(secureString2);

		AreEqual(secureString1.ToUnsecureString(), secureString2.ToUnsecureString());
		IsTrue(secureString1.IsEqual(secureString2));
	}

	#endregion
}