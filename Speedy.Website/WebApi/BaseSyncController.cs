#region References

using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Speedy.Samples;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	public abstract class BaseSyncController : ControllerBase
	{
		#region Fields

		private static readonly ConcurrentDictionary<Guid, SyncClient> _sessions;

		#endregion

		#region Constructors

		protected BaseSyncController(IDatabaseProvider<IContosoDatabase> provider)
		{
			DatabaseProvider = new SyncDatabaseProvider(provider.GetDatabase);
		}

		static BaseSyncController()
		{
			_sessions = new ConcurrentDictionary<Guid, SyncClient>();
		}

		#endregion

		#region Properties

		public ISyncableDatabaseProvider DatabaseProvider { get; }

		#endregion

		#region Methods

		protected ISyncClient BeginSyncSession(Guid sessionId, SyncOptions options)
		{
			// Limit items per request
			if (options.ItemsPerSyncRequest > 300)
			{
				options.ItemsPerSyncRequest = 300;
			}

			// Do not allow clients to permanently delete entities.
			options.PermanentDeletions = false;

			return _sessions.GetOrAdd(sessionId, key =>
			{
				// The server should always maintain dates as they are the "Master" dataset
				var client = new SyncClient("Web Client", DatabaseProvider) { Options = { MaintainModifiedOn = true } };
				client.BeginSync(sessionId, options);
				return client;
			});
		}

		protected ISyncClient EndSyncSession(Guid sessionId)
		{
			_sessions.TryRemove(sessionId, out var client);
			return client;
		}

		protected ISyncClient GetSyncClient(Guid sessionId)
		{
			return _sessions.TryGetValue(sessionId, out var client) ? client : throw new Exception("Could not find the sync session.");
		}

		#endregion
	}
}