#region References

using Speedy.Sync;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Provides sync provider and some web interfaces.
	/// </summary>
	public class WebSyncClientProvider : SyncClientProvider
	{
		#region Constructors

		/// <summary>
		/// Create an instance of a provider for a web sync client.
		/// </summary>
		/// <param name="client"> The web client to use. </param>
		/// <param name="provider"> The syncable database provider. </param>
		public WebSyncClientProvider(IWebClient client, ISyncableDatabaseProvider provider)
			: base(x => new WebSyncClient(x, provider, client))
		{
			Client = client;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The client for web access.
		/// </summary>
		public IWebClient Client { get; }

		#endregion
	}
}