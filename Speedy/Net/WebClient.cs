#region References

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// This class is used for making GET and POST calls to an HTTP endpoint.
	/// </summary>
	public class WebClient : Bindable, IDisposable
	{
		#region Fields

		private AuthenticationHeaderValue _authenticationHeaderValue;
		private NetworkCredential _credential;
		private readonly HttpClientHandler _handler;
		private readonly HttpClient _httpClient;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new HTTP helper to point at a specific URI, and with the specified session identifier.
		/// </summary>
		/// <param name="baseUri"> The base URI. </param>
		/// <param name="timeout"> The timeout in milliseconds. </param>
		/// <param name="credential"> The optional credential to authenticate with. </param>
		/// <param name="proxy"> The optional proxy to use. </param>
		public WebClient(string baseUri, int timeout, NetworkCredential credential = null, WebProxy proxy = null)
		{
			BaseUri = new Uri(baseUri);
			Credential = credential;
			Timeout = TimeSpan.FromMilliseconds(timeout);
			Proxy = proxy;

			_handler = new HttpClientHandler();
			_handler.ServerCertificateCustomValidationCallback += OnServerCertificateCustomValidationCallback;
			_httpClient = CreateHttpClient(_handler);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the base URI for connecting.
		/// </summary>
		public Uri BaseUri
		{
			get => _httpClient.BaseAddress;
			set => _httpClient.BaseAddress = value;
		}

		/// <summary>
		/// The cookies for this client.
		/// </summary>
		public CookieCollection Cookies
		{
			get => _handler.CookieContainer.GetCookies(BaseUri);
			set
			{
				var cookieContainer = new CookieContainer();
				cookieContainer.Add(value);
				_handler.CookieContainer = cookieContainer;
			}
		}

		/// <summary>
		/// The credentials for the connection.
		/// </summary>
		public NetworkCredential Credential
		{
			get => _credential;
			set
			{
				_credential = value;

				if (_httpClient == null)
				{
					return;
				}

				if (_credential != null)
				{
					var headerValue = _authenticationHeaderValue ??= new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_credential.UserName}:{_credential.Password}")));
					_httpClient.DefaultRequestHeaders.Authorization = headerValue;
				}
				else
				{
					_httpClient.DefaultRequestHeaders.Authorization = null;
				}
			}
		}

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
		/// Gets or sets an optional proxy for the connection.
		/// </summary>
		public WebProxy Proxy { get; set; }

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
			return _httpClient.DeleteAsync(uri).AwaitResults(Timeout);
		}

		/// <summary>
		/// Deserialize the response.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="result"> The result to deserialize. </param>
		/// <returns> The deserialized type. </returns>
		public virtual T Deserialize<T>(HttpResponseMessage result)
		{
			return result.Content
				.ReadAsStringAsync()
				.AwaitResults(Timeout)
				.FromJson<T>();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <returns> The deserialized type. </returns>
		public virtual T Get<T>(string uri)
		{
			using var result = Get(uri);

			if (!result.IsSuccessStatusCode)
			{
				throw new WebClientException(result);
			}

			return Deserialize<T>(result);
		}

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <returns> The response from the server. </returns>
		public virtual HttpResponseMessage Get(string uri)
		{
			return _httpClient.GetAsync(uri).AwaitResults(Timeout);
		}

		/// <inheritdoc />
		public override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(Credential):
				{
					_authenticationHeaderValue = null;
					break;
				}
			}
			base.OnPropertyChanged(propertyName);
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
			using var result = InternalPost(uri, content);

			if (!result.IsSuccessStatusCode)
			{
				throw new WebClientException(result);
			}

			return Deserialize<TResult>(result);
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
			using var result = InternalPut(uri, content);

			if (!result.IsSuccessStatusCode)
			{
				throw new WebClientException(result);
			}

			return result.Content
				.ReadAsStringAsync()
				.AwaitResults(Timeout)
				.FromJson<TResult>();
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

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			_httpClient?.Dispose();
			_handler?.Dispose();
		}

		/// <summary>
		/// Validates a server certificate. Defaults to
		/// </summary>
		/// <param name="message"> </param>
		/// <param name="certificate2"> </param>
		/// <param name="arg3"> </param>
		/// <param name="arg4"> </param>
		/// <returns> </returns>
		protected virtual bool OnServerCertificateCustomValidationCallback(HttpRequestMessage message, X509Certificate2 certificate2, X509Chain arg3, SslPolicyErrors arg4)
		{
			return true;
		}

		private HttpClient CreateHttpClient(HttpClientHandler handler)
		{
			foreach (Cookie ck in Cookies)
			{
				handler.CookieContainer.Add(ck);
			}

			if (Proxy != null)
			{
				handler.Proxy = Proxy;
			}

			var client = new HttpClient(handler)
			{
				BaseAddress = BaseUri,
				Timeout = Timeout
			};

			if (Credential != null)
			{
				var headerValue = _authenticationHeaderValue ??= new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Credential.UserName}:{Credential.Password}")));
				client.DefaultRequestHeaders.Authorization = headerValue;
			}

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
			var json = GetJson(content);
			using var objectContent = new StringContent(json, Encoding.UTF8, "application/json-patch+json");
			return PatchAsync(_httpClient, uri, objectContent).AwaitResults(Timeout);
		}

		private HttpResponseMessage InternalPost<T>(string uri, T content)
		{
			var json = GetJson(content);
			using var objectContent = new StringContent(json, Encoding.UTF8, "application/json");
			return _httpClient.PostAsync(uri, objectContent).AwaitResults(Timeout);
		}

		private HttpResponseMessage InternalPut<T>(string uri, T content)
		{
			var json = GetJson(content);
			using var objectContent = new StringContent(json, Encoding.UTF8, "application/json");
			return _httpClient.PutAsync(uri, objectContent).AwaitResults(Timeout);
		}

		private async Task<HttpResponseMessage> PatchAsync(HttpClient client, string uri, HttpContent content)
		{
			var method = new HttpMethod("PATCH");
			var request = new HttpRequestMessage(method, uri) { Content = content };
			return await client.SendAsync(request);
		}

		#endregion
	}
}