#region References

using System;
using System.Collections.Generic;
using Speedy.Sync;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Web client for a sync server implemented over Web API.
	/// </summary>
	public class SyncServerClient : ISyncServer
	{
		#region Fields

		private readonly string _serverUri;
		private readonly int _timeout;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="serverUri"> The server to send data to. </param>
		/// <param name="timeout"> The timeout for each transaction. </param>
		public SyncServerClient(string serverUri, int timeout = 10000)
		{
			_serverUri = serverUri;
			_timeout = timeout;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sends changes to a server.
		/// </summary>
		/// <param name="changes"> The changes to write to the server. </param>
		/// <returns> The date and time for the sync process. </returns>
		public DateTime ApplyChanges(IEnumerable<SyncObject> changes)
		{
			return WebClient.Post<IEnumerable<SyncObject>, DateTime>(_serverUri, $"api/Sync/{nameof(ApplyChanges)}", changes, _timeout);
		}

		/// <summary>
		/// Gets the changes from the server.
		/// </summary>
		/// <param name="since"> The date and time get changes for. </param>
		/// <returns> The list of changes from the server. </returns>
		public IEnumerable<SyncObject> GetChanges(DateTime since)
		{
			return WebClient.Post<DateTime, IEnumerable<SyncObject>>(_serverUri, $"api/Sync/{nameof(GetChanges)}", since, _timeout);
		}

		#endregion
	}
}