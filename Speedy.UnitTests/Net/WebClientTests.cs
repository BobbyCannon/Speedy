#region References

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net
{
	[TestClass]
	public class WebClientTests
	{
		#region Methods

		[TestMethod]
		public void Constructor()
		{
			var client = new WebClient("http://127.0.0.1", 1000);
			Assert.AreEqual("http://127.0.0.1/", client.BaseUri.AbsoluteUri);
			Assert.AreEqual(null, client.Credential);
			Assert.AreEqual(null, client.Proxy);
			Assert.AreEqual(1000, client.Timeout.TotalMilliseconds);

			var credential = new WebCredential("admin@speedy.local", "Password");
			client = new WebClient("http://127.0.0.1", 1234, credential);
			Assert.AreEqual("http://127.0.0.1/", client.BaseUri.AbsoluteUri);
			Assert.AreEqual(credential, client.Credential);
			Assert.AreEqual(null, client.Proxy);
			Assert.AreEqual(1234, client.Timeout.TotalMilliseconds);
		}

		#endregion
	}
}