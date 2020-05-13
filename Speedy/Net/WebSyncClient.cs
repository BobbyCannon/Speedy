#region References

using System;
using System.Net;
using Speedy.Exceptions;
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

		private readonly ISyncableDatabaseProvider _provider;
		private readonly string _syncUri;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="provider"> The database provider for the client </param>
		/// <param name="name"> The name of the client. </param>
		/// <param name="serverUri"> The server to send data to. </param>
		/// <param name="syncUri"> The sync URI. Defaults to "api/Sync". </param>
		/// <param name="credential"> The optional credential for the sync client. </param>
		/// <param name="proxy"> The optional proxy to use. </param>
		/// <param name="timeout"> The timeout in milliseconds for each transaction. </param>
		public WebSyncClient(string name, ISyncableDatabaseProvider provider, string serverUri, string syncUri = "api/Sync", NetworkCredential credential = null, WebProxy proxy = null, int timeout = 10000)
		{
			_provider = provider;
			_syncUri = syncUri;

			Name = name;
			Options = new SyncClientOptions();
			Statistics = new SyncStatistics();
			WebClient = new WebClient(serverUri, timeout, credential, proxy);
		}

		#endregion

		#region Properties

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public SyncClientIncomingConverter IncomingConverter { get; set; }

		/// <inheritdoc />
		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		/// <summary>
		/// The web client to use to connect to the server.
		/// </summary>
		public WebClient WebClient { get; }

		/// <inheritdoc />
		public SyncClientOptions Options { get; set; }

		/// <inheritdoc />
		public SyncStatistics Statistics { get; }

		/// <inheritdoc />
		public SyncOptions SyncOptions { get; private set; }

		/// <inheritdoc />
		public SyncSession SyncSession { get;}

		#endregion

		#region Methods

		/// <inheritdoc />
		public ServiceResult<SyncIssue> ApplyChanges(Guid sessionId, ServiceRequest<SyncObject> changes)
		{
			return WebClient.Post<ServiceRequest<SyncObject>, ServiceResult<SyncIssue>>($"{_syncUri}/{nameof(ApplyChanges)}/{sessionId}", changes);
		}

		/// <inheritdoc />
		public ServiceResult<SyncIssue> ApplyCorrections(Guid sessionId, ServiceRequest<SyncObject> corrections)
		{
			return WebClient.Post<ServiceRequest<SyncObject>, ServiceResult<SyncIssue>>($"{_syncUri}/{nameof(ApplyCorrections)}/{sessionId}", corrections);
		}

		/// <inheritdoc />
		public SyncSession BeginSync(Guid sessionId, SyncOptions options)
		{
			SyncOptions = options;
			return WebClient.Post<SyncOptions, SyncSession>($"{_syncUri}/{nameof(BeginSync)}/{sessionId}", options);
		}

		/// <inheritdoc />
		public SyncStatistics EndSync(Guid sessionId)
		{
			var statistics = WebClient.Post<string, SyncStatistics>($"{_syncUri}/{nameof(EndSync)}/{sessionId}", string.Empty);
			Statistics.UpdateWith(statistics);
			return Statistics;
		}

		/// <inheritdoc />
		public ServiceResult<SyncObject> GetChanges(Guid sessionId, SyncRequest request)
		{
			return WebClient.Post<SyncRequest, ServiceResult<SyncObject>>($"{_syncUri}/{nameof(GetChanges)}/{sessionId}", request);
		}

		/// <inheritdoc />
		public ServiceResult<SyncObject> GetCorrections(Guid sessionId, ServiceRequest<SyncIssue> issues)
		{
			return WebClient.Post<ServiceRequest<SyncIssue>, ServiceResult<SyncObject>>($"{_syncUri}/{nameof(GetCorrections)}/{sessionId}", issues);
		}

		/// <inheritdoc />
		public ISyncableDatabase GetDatabase()
		{
			return _provider.GetSyncableDatabase();
		}

		/// <inheritdoc />
		public T GetDatabase<T>() where T : class, ISyncableDatabase
		{
			return (T) _provider.GetSyncableDatabase();
		}

		#endregion
	}
}