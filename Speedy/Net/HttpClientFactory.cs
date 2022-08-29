#region References

using System;
using System.Collections.Concurrent;
using System.Net.Http;

#endregion

namespace Speedy.Net
{
	/*
	 *
	 * I'm still not sure how this is going to work... especially with WebClient...	 *
	 *
	 */

	/// <inheritdoc />
	public class HttpClientFactory : IHttpClientFactory
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, HttpClient> _clients;

		#endregion

		#region Constructors

		static HttpClientFactory()
		{
			_clients = new ConcurrentDictionary<string, HttpClient>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public HttpClient CreateClient(string name)
		{
			return _clients.GetOrAdd(name, x => new HttpClient());
		}

		#endregion
	}
}