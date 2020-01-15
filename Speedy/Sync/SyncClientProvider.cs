#region References

using System;
using System.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a provider to get a sync client.
	/// </summary>
	public class SyncClientProvider
	{
		#region Fields

		private readonly Func<string, NetworkCredential, ISyncClient> _getClient;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a provider to get a sync client.
		/// </summary>
		public SyncClientProvider(Func<string, NetworkCredential, ISyncClient> getClient)
		{
			_getClient = getClient;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Return a client by the provided name and credential.
		/// </summary>
		/// <param name="name"> The name of the client. </param>
		/// <param name="credential"> The credential for the client. </param>
		/// <returns> The sync client. </returns>
		public ISyncClient GetClient(string name, NetworkCredential credential)
		{
			return _getClient.Invoke(name, credential);
		}

		#endregion
	}
}