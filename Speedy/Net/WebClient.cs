#region References

using Speedy.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// This class is used for making GET and POST calls to an HTTP endpoint.
	/// </summary>
	public class WebClient
	{
		#region Constructors

		/// <summary>
		/// Initializes a new HTTP helper to point at a specific URI, and with the specified session identifier.
		/// </summary>
		/// <param name="baseUri"> The base URI. </param>
		/// <param name="timeout"> The timeout in milliseconds. </param>
		/// <param name="credential"> The optional credential to authenticate with. </param>
		public WebClient(string baseUri, int timeout, NetworkCredential credential = null)
		{
			BaseUri = baseUri;
			Cookies = new CookieCollection();
			Credential = credential;
			Headers = new Dictionary<string, string>();
			Timeout = TimeSpan.FromMilliseconds(timeout);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the base URI for connecting.
		/// </summary>
		public string BaseUri { get; set; }

		/// <summary>
		/// The cookies for this client.
		/// </summary>
		public CookieCollection Cookies { get; set; }

		/// <summary>
		/// The credentials for the connection.
		/// </summary>
		public NetworkCredential Credential { get; set; }

		/// <summary>
		/// The headers for the connection.
		/// </summary>
		public IDictionary<string, string> Headers { get; set; }

		/// <summary>
		/// Determines if the connection is authenticated.
		/// </summary>
		public bool IsAuthenticated
		{
			get
			{
				foreach (Cookie cookie in Cookies)
				{
					if (cookie.Name == ".ASPXAUTH")
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Gets or sets the number of milliseconds to wait before the request times out. The default value is 100 seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Delete request
		/// </summary>
		/// <param name="uri"> The URI to use. </param>
		/// <returns> The response from the server. </returns>
		public HttpResponseMessage Delete(string uri)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = CreateHttpClient(handler))
				{
					var response = client.DeleteAsync(uri).Result;
					return ProcessResponse(response, handler);
				}
			}
		}

		/// <summary>
		/// Deserialize the response.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="result"> The result to deserialize. </param>
		/// <returns> The deserialized type. </returns>
		public T Deserialize<T>(HttpResponseMessage result)
		{
			return result.Content.ReadAsStringAsync().Result.FromJson<T>();
		}

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="content"> The content to deserialize. </param>
		/// <returns> The deserialized type. </returns>
		public static T Get<T>(HttpContent content)
		{
			return content.ReadAsStringAsync().Result.FromJson<T>();
		}

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <returns> The deserialized type. </returns>
		public virtual T Get<T>(string uri)
		{
			using (var result = Get(uri))
			{
				if (!result.IsSuccessStatusCode)
				{
					throw new WebClientException(result);
				}

				return Get<T>(result.Content);
			}
		}

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Get(string uri)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = CreateHttpClient(handler))
				{
					var response = client.GetAsync(uri).Result;
					return ProcessResponse(response, handler);
				}
			}
		}

		/// <summary>
		/// Patch an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to patch to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Patch<TContent>(string uri, TContent content)
		{
			return InternalPatch(uri, content);
		}

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <typeparam name="TResult"> The type to respond with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The server result. </returns>
		public virtual TResult Post<TContent, TResult>(string uri, TContent content)
		{
			using (var result = InternalPost(uri, content))
			{
				if (!result.IsSuccessStatusCode)
				{
					throw new WebClientException(result);
				}

				return Deserialize<TResult>(result);
			}
		}

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Post<TContent>(string uri, TContent content)
		{
			return InternalPost(uri, content);
		}

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Post(string uri, string content)
		{
			return InternalPost(uri, content);
		}

		/// <summary>
		/// Put an item on the server with the provide content.
		/// </summary>
		/// <param name="uri"> The URI to put to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The response from the server. </returns>
		public virtual TResult Put<TContent, TResult>(string uri, TContent content)
		{
			using (var result = InternalPut(uri, content))
			{
				if (!result.IsSuccessStatusCode)
				{
					throw new WebClientException(result);
				}

				return result.Content.ReadAsStringAsync().Result.FromJson<TResult>();
			}
		}

		/// <summary>
		/// Put (update) an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Put<TContent>(string uri, TContent content)
		{
			return InternalPut(uri, content);
		}

		private HttpClient CreateHttpClient(HttpClientHandler handler)
		{
			foreach (Cookie ck in Cookies)
			{
				handler.CookieContainer.Add(ck);
			}

			var client = new HttpClient(handler)
			{
				BaseAddress = new Uri(BaseUri),
				Timeout = Timeout
			};

			if (Credential != null)
			{
				var value = $"{Credential.UserName}:{Credential.Password}";
				var headerValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
				client.DefaultRequestHeaders.Authorization = headerValue;
			}

			Headers.ForEach(x => client.DefaultRequestHeaders.Add(x.Key, x.Value));
			return client;
		}

		private string GetJson(object content)
		{
			var s = content as string;
			var json = s?.IsJson() == true ? s : content.ToJson();
			return json;
		}

		private HttpResponseMessage InternalPatch<T>(string uri, T content)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = CreateHttpClient(handler))
				{
					var json = GetJson(content);

					using (var objectContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json"))
					{
						var response = PatchAsync(client, uri, objectContent).Result;
						return ProcessResponse(response, handler);
					}
				}
			}
		}

		private HttpResponseMessage InternalPost<T>(string uri, T content)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = CreateHttpClient(handler))
				{
					var json = GetJson(content);

					using (var objectContent = new StringContent(json, Encoding.UTF8, "application/json"))
					{
						var response = client.PostAsync(uri, objectContent).Result;
						return ProcessResponse(response, handler);
					}
				}
			}
		}

		private HttpResponseMessage InternalPut<T>(string uri, T content)
		{
			using (var handler = new HttpClientHandler())
			{
				using (var client = CreateHttpClient(handler))
				{
					var json = GetJson(content);

					using (var objectContent = new StringContent(json, Encoding.UTF8, "application/json"))
					{
						var response = client.PutAsync(uri, objectContent).Result;
						return ProcessResponse(response, handler);
					}
				}
			}
		}

		private async Task<HttpResponseMessage> PatchAsync(HttpClient client, string uri, HttpContent content)
		{
			var method = new HttpMethod("PATCH");
			var request = new HttpRequestMessage(method, uri) { Content = content };
			return await client.SendAsync(request);
		}

		private HttpResponseMessage ProcessResponse(HttpResponseMessage response, HttpClientHandler handler)
		{
			if (handler.CookieContainer != null && Uri.IsWellFormedUriString(BaseUri, UriKind.Absolute))
			{
				Cookies = handler.CookieContainer.GetCookies(new Uri(BaseUri));
			}

			return response;
		}

		#endregion
	}
}