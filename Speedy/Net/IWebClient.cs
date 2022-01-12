#region References

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Represents a web client contract.
	/// </summary>
	public interface IWebClient : IDisposable, Serialization.ICloneable
	{
		#region Properties

		/// <summary>
		/// Gets the base URI for connecting.
		/// </summary>
		Uri BaseUri { get; set; }

		/// <summary>
		/// The credentials for the connection.
		/// </summary>
		WebCredential Credential { get; set; }

		/// <summary>
		/// Headers for this client.
		/// </summary>
		HttpHeaders Headers { get; set; }

		/// <summary>
		/// Gets or sets an optional proxy for the connection.
		/// </summary>
		IWebProxy Proxy { get; set; }

		/// <summary>
		/// Gets or sets the number of milliseconds to wait before the request times out. The default value is 100 seconds.
		/// </summary>
		TimeSpan Timeout { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Delete request
		/// </summary>
		/// <param name="uri"> The URI to use. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Delete(string uri, TimeSpan? timeout = null);

		/// <summary>
		/// Deserialize the response.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="result"> The result to deserialize. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The deserialized type. </returns>
		T Deserialize<T>(HttpResponseMessage result, TimeSpan? timeout = null);

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The deserialized type. </returns>
		T Get<T>(string uri, TimeSpan? timeout = null);

		/// <summary>
		/// Gets a response and deserialize it.
		/// </summary>
		/// <param name="uri"> The URI of the content to deserialize. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Get(string uri, TimeSpan? timeout = null);

		/// <summary>
		/// Initialize the web client.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Patch an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to patch to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Patch<TContent>(string uri, TContent content, TimeSpan? timeout = null);

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <typeparam name="TResult"> The type to respond with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The server result. </returns>
		TResult Post<TContent, TResult>(string uri, TContent content, TimeSpan? timeout = null);

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Post<TContent>(string uri, TContent content, TimeSpan? timeout = null);

		/// <summary>
		/// Post an item on the server with the provide content.
		/// </summary>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Post(string uri, string content, TimeSpan? timeout = null);

		/// <summary>
		/// Put an item on the server with the provide content.
		/// </summary>
		/// <param name="uri"> The URI to put to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		TResult Put<TContent, TResult>(string uri, TContent content, TimeSpan? timeout = null);

		/// <summary>
		/// Put (update) an item on the server with the provide content.
		/// </summary>
		/// <typeparam name="TContent"> The type to update with. </typeparam>
		/// <param name="uri"> The URI to post to. </param>
		/// <param name="content"> The content to update with. </param>
		/// <param name="timeout"> An optional timeout to override the default Timeout value. </param>
		/// <returns> The response from the server. </returns>
		HttpResponseMessage Put<TContent>(string uri, TContent content, TimeSpan? timeout = null);

		/// <summary>
		/// Reset the web client.
		/// </summary>
		void Reset();

		#endregion
	}
}