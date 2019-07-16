#region References

using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Speedy.Net;
using Speedy.Samples;
using Speedy.Samples.Entities;
using Speedy.Sync;
using Speedy.Website.Samples.Models;

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

		private static readonly SyncClientConverter _incomingConverter;
		private static readonly SyncClientConverter _outgoingConverter;

		#endregion

		#region Constructors

		public ModelSyncController(IDatabaseProvider<IContosoDatabase> provider) : base(provider)
		{
		}

		static ModelSyncController()
		{
			// Converts entities to public models, allows exclusions because source data is trusted.
			_outgoingConverter = new SyncClientConverter(false, false,
				new SyncObjectConverter<AddressEntity, long, Address, long>(),
				new SyncObjectConverter<PersonEntity, int, Person, int>()
			);

			// Converts public models to entities as the data come into the sync client, ensure exclusion because data is not trusted.
			_incomingConverter = new SyncClientConverter(true, true,
				new SyncObjectConverter<Address, long, AddressEntity, long>(),
				new SyncObjectConverter<Person, int, PersonEntity, int>()
			);
		}

		#endregion

		#region Properties

		public SyncClientConverter IncomingConverter { get; set; }

		public string Name { get; set; }

		public SyncClientConverter OutgoingConverter { get; set; }

		public SyncClientOptions Options { get; set; }

		public SyncStatistics Statistics { get; set; }

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
		public void BeginSync(Guid id, [FromBody] SyncOptions options)
		{
			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			var sessionOptions = new SyncOptions
			{
				ItemsPerSyncRequest = options.ItemsPerSyncRequest > 300 ? 300 : options.ItemsPerSyncRequest,
				PermanentDeletions = false,
				LastSyncedOn = options.LastSyncedOn
			};

			sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
			sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<PersonEntity>());

			var client = BeginSyncSession(id, sessionOptions);
			client.OutgoingConverter = _outgoingConverter;
			client.IncomingConverter = _incomingConverter;
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