#region References

using System;
using Microsoft.AspNetCore.Mvc;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	public class BaseEntitySyncController : BaseSyncController, ISyncClient
	{
		#region Constructors

		public BaseEntitySyncController(IDatabaseProvider<IContosoDatabase> provider) : base(provider)
		{
		}

		#endregion

		#region Properties

		public SyncClientConverter IncomingConverter { get; set; }

		public string Name { get; set; }

		public SyncClientConverter OutgoingConverter { get; set; }

		public SyncClientOptions Options { get; set; }

		public SyncStatistics Statistics { get; set; }

		public SyncOptions SyncOptions { get; set; }

		#endregion

		#region Methods

		[HttpPost("ApplyChanges/{id}")]
		public ServiceResult<SyncIssue> ApplyChanges(Guid id, [FromBody] ServiceRequest<SyncObject> changes)
		{
			var client = GetSyncClient(id);
			return client.ApplyChanges(id, changes);
		}

		[HttpPost("ApplyCorrections/{id}")]
		public ServiceResult<SyncIssue> ApplyCorrections(Guid id, [FromBody] ServiceRequest<SyncObject> corrections)
		{
			var client = GetSyncClient(id);
			return client.ApplyCorrections(id, corrections);
		}

		[HttpPost("BeginSync/{id}")]
		public void BeginSync(Guid id, SyncOptions options)
		{
			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			BeginSyncSession(id, options);
		}

		public void EndSync(Guid id)
		{
			// Not actually used, just implement due to the interface
			// See method EndSyncAndReturnStatistics below
		}
		
		[HttpPost("EndSync/{id}")]
		public SyncStatistics EndSyncAndReturnStatistics(Guid id)
		{
			var client = EndSyncSession(id);
			return client?.Statistics ?? new SyncStatistics();
		}

		[HttpPost("GetChanges/{id}")]
		public ServiceResult<SyncObject> GetChanges(Guid id, [FromBody] SyncRequest request)
		{
			var client = GetSyncClient(id);
			return client.GetChanges(id, request);
		}

		[HttpPost("GetCorrections/{id}")]
		public ServiceResult<SyncObject> GetCorrections(Guid id, [FromBody] ServiceRequest<SyncIssue> issues)
		{
			var client = GetSyncClient(id);
			return client.GetCorrections(id, issues);
		}

		public ISyncableDatabase GetDatabase()
		{
			throw new NotImplementedException();
		}

		public T GetDatabase<T>() where T : class, ISyncableDatabase
		{
			throw new NotImplementedException();
		}

		[HttpPost("UpdateOptions/{id}")]
		public void UpdateOptions(Guid id, SyncClientOptions options)
		{
			var client = GetSyncClient(id);
			client.UpdateOptions(id, options);
		}

		#endregion
	}
}