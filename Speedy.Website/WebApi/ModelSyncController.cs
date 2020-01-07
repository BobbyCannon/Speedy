#region References

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Speedy.Client.Samples.Models;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Sync;

#endregion

namespace Speedy.Website.WebApi
{
	/// <summary>
	/// Represent how to sync model database to a server where it then syncs to an entity database.
	/// </summary>
	[AllowAnonymous]
	[ApiController]
	[Route("api/ModelSync")]
	public class ModelSyncController : BaseSyncController, ISyncClient
	{
		#region Fields

		private static readonly SyncClientIncomingConverter _incomingConverter;
		private static readonly SyncClientOutgoingConverter _outgoingConverter;

		#endregion

		#region Constructors

		public ModelSyncController(IDatabaseProvider<IContosoDatabase> provider) : base(provider)
		{
		}

		static ModelSyncController()
		{
			// Converts entities to public models using outgoing converters.
			_outgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
				new SyncObjectOutgoingConverter<PersonEntity, int, Person, int>()
			);

			// Converts public models to entities as the data come into the sync client when data is not trusted.
			_incomingConverter = new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Address, long, AddressEntity, long>(),
				new SyncObjectIncomingConverter<Person, int, PersonEntity, int>()
			);
		}

		#endregion

		#region Properties

		public SyncClientIncomingConverter IncomingConverter { get; set; }

		public string Name { get; set; }

		public SyncClientOutgoingConverter OutgoingConverter { get; set; }

		public SyncClientOptions Options { get; set; }

		public SyncStatistics Statistics { get; set; }
		
		public SyncOptions SyncOptions { get; set; }

		#endregion

		#region Methods

		[HttpPost]
		[Route("ApplyChanges/{id}")]
		public ServiceResult<SyncIssue> ApplyChanges(Guid id, [FromBody] ServiceRequest<SyncObject> changes)
		{
			var client = GetSyncClient(id);
			return client.ApplyChanges(id, changes);
		}

		[HttpPost]
		[Route("ApplyCorrections/{id}")]
		public ServiceResult<SyncIssue> ApplyCorrections(Guid id, [FromBody] ServiceRequest<SyncObject> corrections)
		{
			var client = GetSyncClient(id);
			return client.ApplyCorrections(id, corrections);
		}

		[HttpPost]
		[Route("BeginSync/{id}")]
		public SyncSession BeginSync(Guid id, [FromBody] SyncOptions options)
		{
			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			var sessionOptions = new SyncOptions
			{
				ItemsPerSyncRequest = options.ItemsPerSyncRequest > 300 ? 300 : options.ItemsPerSyncRequest,
				PermanentDeletions = false,
				LastSyncedOnClient = options.LastSyncedOnClient,
				LastSyncedOnServer = options.LastSyncedOnServer
			};

			sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
			sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<PersonEntity>());

			var result = BeginSyncSession(id, sessionOptions);
			result.client.OutgoingConverter = _outgoingConverter;
			result.client.IncomingConverter = _incomingConverter;

			return result.session;
		}

		public void EndSync(SyncSession session)
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

		[HttpPost]
		[Route("GetChanges/{id}")]
		public ServiceResult<SyncObject> GetChanges(Guid id, [FromBody] SyncRequest request)
		{
			var client = GetSyncClient(id);
			return client.GetChanges(id, request);
		}

		[HttpPost]
		[Route("GetCorrections/{id}")]
		public ServiceResult<SyncObject> GetCorrections(Guid id, [FromBody] ServiceRequest<SyncIssue> issues)
		{
			var client = GetSyncClient(id);
			return client.GetCorrections(id, issues);
		}

		public ISyncableDatabase GetDatabase()
		{
			throw new NotImplementedException();
		}

		T ISyncClient.GetDatabase<T>()
		{
			throw new NotImplementedException();
		}

		[HttpPost("UpdateOptions/{id}")]
		public void UpdateOptions(Guid id, SyncClientOptions options)
		{
			var client = GetSyncClient(id);
			client.Options.UpdateWith(options);
		}

		#endregion
	}
}