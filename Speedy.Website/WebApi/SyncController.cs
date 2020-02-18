#region References

using System;
using System.Linq;
using System.Web.Http;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Samples;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;
using Speedy.Website.Services;

#endregion

namespace Speedy.Website.WebApi
{
	/// <summary>
	/// Any entity that gets modified during the life cycle of the controller will never get synced. Syncing is based on time and the client will request a sync
	/// of the server based on the time in which it started. Meaning if a controller touches a syncable entity before allowing the sync to occur will basically
	/// ensure that the modified entity will never sync.
	/// An example: When a user is authenticated we update the LastLoginDate on the UserEntity. Previously this would also update the ModifiedOn because the entity
	/// was technically modified. The problem is that then that UserEntity would never sync as a Personnel model.
	/// </summary>
	[RoutePrefix("api/Sync")]
	public class SyncController : BaseSyncController, ISyncClient
	{
		#region Constants

		private const string SyncAccountChangedKey = "SyncAccountChanged";

		#endregion

		#region Fields

		private static readonly string _accountAssemblyName;
		private static readonly SyncClientOutgoingConverter _outgoingConverter;

		#endregion

		#region Constructors

		public SyncController(IDatabaseProvider<IContosoDatabase> provider, IAuthenticationService authenticationService)
			: base(provider, authenticationService)
		{
			Name = "Sync Controller";
			Options = new SyncClientOptions();
			MinimumSyncSystemSupported = new Version(0, 0, 0, 0);
			MaximumSyncSystemSupported = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
			Statistics = new SyncStatistics();
			SyncOptions = new SyncOptions();
		}

		static SyncController()
		{
			_accountAssemblyName = typeof(Account).ToAssemblyName();
			_outgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<AccountEntity, int, Account, int>(),
				new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
				new SyncObjectOutgoingConverter<LogEventEntity, long, LogEvent, long>()
			);
		}

		#endregion

		#region Properties

		/// <summary>
		/// This is not used but part of the interface. We must have the current authorized user for processing.
		/// </summary>
		public SyncClientIncomingConverter IncomingConverter { get; set; }

		/// <summary>
		/// Maximum sync system supported.
		/// </summary>
		public Version MaximumSyncSystemSupported { get; set; }

		/// <summary>
		/// Minimum sync system supported.
		/// </summary>
		public Version MinimumSyncSystemSupported { get; set; }

		public string Name { get; }

		public SyncClientOptions Options { get; }

		public SyncClientOutgoingConverter OutgoingConverter
		{
			get => _outgoingConverter;
			set => throw new NotImplementedException();
		}

		public SyncStatistics Statistics { get; }

		public SyncOptions SyncOptions { get; }

		#endregion

		#region Methods

		/// <inheritdoc />
		[HttpPost]
		[Route("ApplyChanges/{id}")]
		public ServiceResult<SyncIssue> ApplyChanges(Guid id, [FromBody] ServiceRequest<SyncObject> changes)
		{
			// Must define the incoming filter for each sync because we need the current authenticated user for processing
			var client = GetSyncClient(id);
			client.IncomingConverter = GetIncomingFilter(GetCurrentAccount);
			client.SyncOptions.Values.AddOrUpdate(SyncAccountChangedKey, changes.Collection.Any(x => x.TypeName == _accountAssemblyName).ToString());
			return client.ApplyChanges(id, changes);
		}

		/// <inheritdoc />
		[HttpPost]
		[Route("ApplyCorrections/{id}")]
		public ServiceResult<SyncIssue> ApplyCorrections(Guid id, [FromBody] ServiceRequest<SyncObject> corrections)
		{
			// Must define the incoming filter for each sync because we need the current authenticated user for processing
			var client = GetSyncClient(id);
			client.IncomingConverter = GetIncomingFilter(GetCurrentAccount);
			client.SyncOptions.Values.AddOrUpdate(SyncAccountChangedKey, corrections.Collection.Any(x => x.TypeName == _accountAssemblyName).ToString());
			return client.ApplyCorrections(id, corrections);
		}

		/// <inheritdoc />
		[HttpPost]
		[Route("BeginSync/{id}")]
		public SyncSession BeginSync(Guid id, [FromBody] SyncOptions options)
		{
			var account = GetCurrentAccount();
			var validSyncVersion = options.Values.TryGetValue(SyncManager.SyncVersionKey, out var versionString)
				| Version.TryParse(versionString ?? string.Empty, out var version)
				| (version >= MinimumSyncSystemSupported)
				| (version <= MaximumSyncSystemSupported);

			if (!validSyncVersion)
			{
				throw new SpeedyException(Constants.SyncClientInvalid);
			}

			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			var sessionOptions = new SyncOptions
			{
				IncludeIssueDetails = account.InRole(AccountRole.Administrator),
				ItemsPerSyncRequest = options.ItemsPerSyncRequest > 300 ? 300 : options.ItemsPerSyncRequest,
				PermanentDeletions = false,
				LastSyncedOnClient = options.LastSyncedOnClient,
				LastSyncedOnServer = options.LastSyncedOnServer
			};

			var syncKey = options.Values.TryGetValue(SyncManager.SyncKey, out var value) ? Enum.TryParse<SyncType>(value, true, out var sync) ? sync : SyncType.All : SyncType.All;

			switch (syncKey)
			{
				case SyncType.Account:
				{
					if (options.Values.ContainsKey(ClientSyncManager.AccountValueKey))
					{
						// We want to sync a single account
						Guid.TryParse(options.Values[ClientSyncManager.AccountValueKey], out var accountSyncId);
						sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>(x => x.SyncId == accountSyncId));
					}
					else
					{
						throw new InvalidOperationException(Constants.InvalidSyncOptions);
					}
					break;
				}
				case SyncType.Accounts:
				{
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					break;
				}
				case SyncType.Address:
				{
					if (options.Values.ContainsKey(ClientSyncManager.AddressValueKey))
					{
						// We want to sync a single address
						Guid.TryParse(options.Values[ClientSyncManager.AddressValueKey], out var addressSyncId);
						sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.SyncId == addressSyncId));
					}
					else
					{
						throw new InvalidOperationException(Constants.InvalidSyncOptions);
					}
					break;
				}
				case SyncType.Addresses:
				{
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					break;
				}
				case SyncType.LogEvents:
				{
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<LogEventEntity>());
					break;
				}
				case SyncType.All:
				default:
				{
					// Default to sync all items (addresses, accounts)
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>());
					sessionOptions.AddSyncableFilter(new SyncRepositoryFilter<LogEventEntity>());
					break;
				}
			}

			var (client, session) = BeginSyncSession(id, sessionOptions);
			client.OutgoingConverter = OutgoingConverter;

			// client.IncomingConverter is defined per incoming request
			return session;
		}

		/// <inheritdoc />
		public void EndSync(SyncSession session)
		{
			// Not actually used, just implement due to the interface and unit testing
			// See method EndSyncAndReturnStatistics below	
			EndSyncAndReturnStatistics(session.Id);
		}

		[HttpPost]
		[Route("EndSync/{id}")]
		public SyncStatistics EndSyncAndReturnStatistics(Guid id)
		{
			var client = EndSyncSession(id);
			return client.Statistics;
		}

		/// <inheritdoc />
		[HttpPost]
		[Route("GetChanges/{id}")]
		public ServiceResult<SyncObject> GetChanges(Guid id, [FromBody] SyncRequest request)
		{
			var client = GetSyncClient(id);
			var results = client.GetChanges(id, request);
			return results;
		}

		/// <inheritdoc />
		[HttpPost]
		[Route("GetCorrections/{id}")]
		public ServiceResult<SyncObject> GetCorrections(Guid id, [FromBody] ServiceRequest<SyncIssue> issues)
		{
			var client = GetSyncClient(id);
			return client.GetCorrections(id, issues);
		}

		[NonAction]
		public ISyncableDatabase GetDatabase()
		{
			return DatabaseProvider.GetSyncableDatabase();
		}

		[NonAction]
		public T GetDatabase<T>() where T : class, ISyncableDatabase
		{
			return (T) DatabaseProvider.GetSyncableDatabase();
		}

		[HttpPost]
		[Route("UpdateOptions/{id}")]
		public void UpdateOptions(Guid id, [FromBody] SyncClientOptions options)
		{
			var client = GetSyncClient(id);
			client.UpdateOptions(id, options);
		}

		private static SyncClientIncomingConverter GetIncomingFilter(Func<AccountEntity> getAccount)
		{
			return new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, AccountEntity, int>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Deleted:
							{
								// Do not allow deletes unless you are administrator
								var account = getAccount();
								return account.InRole(AccountRole.Administrator);
							}
							default:
							{
								processUpdate();
								return true;
							}
						}
					}),
				new SyncObjectIncomingConverter<Address, long, AddressEntity, long>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Modified:
							{
								processUpdate();
								return true;
							}
							case SyncObjectStatus.Deleted:
							default:
							{
								return true;
							}
						}
					}),
				new SyncObjectIncomingConverter<LogEvent, long, LogEventEntity, long>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Modified:
							case SyncObjectStatus.Deleted:
							default:
							{
								return false;
							}
						}
					})
			);
		}

		#endregion
	}
}