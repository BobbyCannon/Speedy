#region References

using System.Net.Http;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Speedy.Extensions;
using Speedy.Net;

#endregion

namespace Speedy.UnitTests.Net
{
	[TestClass]
	public class WebClientTests : SpeedyUnitTest
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

			var credential = new Credential("admin@speedy.local", "Password");
			client = new WebClient("http://127.0.0.1", 1234, credential);
			Assert.AreEqual("http://127.0.0.1/", client.BaseUri.AbsoluteUri);
			Assert.AreEqual(credential, client.Credential);
			Assert.AreEqual(null, client.Proxy);
			Assert.AreEqual(1234, client.Timeout.TotalMilliseconds);

			// _httpClient.DefaultRequestHeaders.Authorization
			var fieldInfo = client.GetCachedField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(fieldInfo);

			var httpClient = (HttpClient) fieldInfo.GetValue(client);
			Assert.IsNotNull(httpClient);
			Assert.IsNotNull(httpClient.DefaultRequestHeaders.Authorization);

			AreEqual("Basic", httpClient.DefaultRequestHeaders.Authorization.Scheme);
			AreEqual("YWRtaW5Ac3BlZWR5LmxvY2FsOlBhc3N3b3Jk", httpClient.DefaultRequestHeaders.Authorization.Parameter);
		}
		
		[TestMethod]
		public void TokenCredential()
		{
			var credential = new TokenCredential("this is my special token");
			var client = new WebClient("http://127.0.0.1", 1234, credential);
			
			// _httpClient.DefaultRequestHeaders.Authorization
			var fieldInfo = client.GetCachedField("_httpClient", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(fieldInfo);

			var httpClient = (HttpClient) fieldInfo.GetValue(client);
			Assert.IsNotNull(httpClient);
			Assert.IsNotNull(httpClient.DefaultRequestHeaders.Authorization);

			AreEqual("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
			AreEqual("dGhpcyBpcyBteSBzcGVjaWFsIHRva2Vu", httpClient.DefaultRequestHeaders.Authorization.Parameter);
			AreEqual("this is my special token", httpClient.DefaultRequestHeaders.Authorization.Parameter.FromBase64String());
		}

		
		#endregion
	}
}