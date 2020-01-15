#region References

using System;
using System.Collections.Concurrent;
using Speedy.Exceptions;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseSyncController : BaseController
	{
		#region Fields

		private static readonly ConcurrentDictionary<SessionKey, SyncClient> _sessions;

		#endregion

		#region Constructors

		protected BaseSyncController(IDatabaseProvider<IContosoDatabase> provider, IAuthenticationService authenticationService)
			: base(provider.GetDatabase(), authenticationService)
		{
			DatabaseProvider = new SyncDatabaseProvider(provider.GetDatabase, provider.Options);
		}

		static BaseSyncController()
		{
			_sessions = new ConcurrentDictionary<SessionKey, SyncClient>();
		}

		#endregion

		#region Properties

		public ISyncableDatabaseProvider DatabaseProvider { get; }

		#endregion

		#region Methods

		protected (ISyncClient client, SyncSession session) BeginSyncSession(Guid sessionId, SyncOptions options)
		{
			// Limit items per request
			if (options.ItemsPerSyncRequest > 300)
			{
				options.ItemsPerSyncRequest = 300;
			}

			// Do not allow clients to permanently delete entities
			options.PermanentDeletions = false;

			// The server should always maintain dates as they are the "Master" dataset
			var client = _sessions.GetOrAdd(GetSessionKey(sessionId), key => new SyncClient("Web Client", DatabaseProvider) { Options = { MaintainModifiedOn = true } });
			var session = client.BeginSync(sessionId, options);

			return (client, session);
		}

		protected SyncClient EndSyncSession(Guid sessionId)
		{
			_sessions.TryRemove(GetSessionKey(sessionId), out var client);
			return client;
		}

		protected SyncClient GetSyncClient(Guid sessionId)
		{
			return _sessions.TryGetValue(GetSessionKey(sessionId), out var client) ? client : throw new SpeedyException("Could not find the sync session.");
		}

		private SessionKey GetSessionKey(Guid sessionId)
		{
			var account = GetCurrentAccount();
			return new SessionKey(account.Id, sessionId);
		}

		#endregion
	}
}