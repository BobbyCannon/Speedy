#region References

using System;
using System.Collections.Concurrent;

#endregion

namespace Speedy.Net
{
	/// <inheritdoc />
	public class WebClientProvider : IWebClientProvider
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, WebClient> _clients;

		#endregion

		#region Constructors

		static WebClientProvider()
		{
			_clients = new ConcurrentDictionary<string, WebClient>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public IWebClient CreateClient(string baseUri, Func<WebCredential> getCredential = null)
		{
			return _clients.GetOrAdd(baseUri, x => new WebClient(x, credential: getCredential?.Invoke()));
		}

		#endregion
	}

	/// <summary>
	/// A provider for IWebClients.
	/// </summary>
	public interface IWebClientProvider
	{
		#region Methods

		/// <summary>
		/// Get or create a client for a base URi.
		/// </summary>
		/// <param name="baseUri"> The URI the client is for. </param>
		/// <param name="credential"> An optional method to get credential for authorization. </param>
		/// <returns> The web client. </returns>
		IWebClient CreateClient(string baseUri, Func<WebCredential> credential = null);

		#endregion
	}
}