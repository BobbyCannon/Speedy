#region References

using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Web client for a sync server implemented over Web API.
	/// </summary>
	public class WebSyncClient : ISyncClient
	{
		#region Fields

		private readonly int _timeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="serverUri"> The server to send data to. </param>
		/// <param name="timeout"> The timeout for each transaction. </param>
		public WebSyncClient(string serverUri, int timeout = 10000)
		{
			Name = serverUri;
			_timeout = timeout;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the name of the sync client.
		/// </summary>
		public string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public void ApplyChanges(IEnumerable<SyncObject> changes)
		{
			WebClient.Post(Name, $"api/Sync/{nameof(ApplyChanges)}", changes, _timeout);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public int GetChangeCount(SyncRequest request)
		{
			return WebClient.Post<SyncRequest, int>(Name, $"api/Sync/{nameof(GetChangeCount)}", request, _timeout);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="request"> The details for the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(SyncRequest request)
		{
			return WebClient.Post<SyncRequest, IEnumerable<SyncObject>>(Name, $"api/Sync/{nameof(GetChanges)}", request, _timeout);
		}

		#endregion
	}
}