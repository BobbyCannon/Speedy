#region References

using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;

#endregion

namespace Speedy.Data
{
	public class ClientSyncManager : SyncManager<SyncType>
	{
		#region Constants

		public const string AccountValueKey = "Account";
		public const string AddressValueKey = "Address";
		public const string DefaultWebsiteUri = "https://speedy.local";

		#endregion

		#region Fields

		private readonly Func<Credential> _credentialProvider;
		private readonly ISyncableDatabaseProvider _databaseProvider;
		private static readonly SyncClientOutgoingConverter _outgoingConverter;
		private readonly SyncClientProvider _serverProvider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync manager for syncing two clients.
		/// </summary>
		/// <param name="credentialProvider"> A provider for network credentials. Used for the sync client provider. </param>
		/// <param name="databaseProvider"> The database provider. </param>
		/// <param name="serverProvider"> The server provider to get a sync client. </param>
		/// <param name="profiler"> The profiler to use when syncing. </param>
		/// <param name="dispatcher"> An optional dispatcher to update with. </param>
		public ClientSyncManager(Func<Credential> credentialProvider, ISyncableDatabaseProvider databaseProvider,
			SyncClientProvider serverProvider, ProfileService profiler, IDispatcher dispatcher)
			: base(dispatcher)
		{
			_credentialProvider = credentialProvider;
			_databaseProvider = databaseProvider;
			_serverProvider = serverProvider;

			// These options are for syncing full collections
			GetOrAddSyncOptions(SyncType.Accounts, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>()));
			GetOrAddSyncOptions(SyncType.Addresses, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientAddress>()));
			GetOrAddSyncOptions(SyncType.LogEvents, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientLogEvent>()));
			GetOrAddSyncOptions(SyncType.All, options =>
			{
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientAddress>());
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>());
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientLogEvent>());
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientSetting>());
			});

			IncomingConverter = GetIncomingConverter();
			OutgoingConverter = _outgoingConverter;

			Profiler = profiler;
		}

		static ClientSyncManager()
		{
			_outgoingConverter = GetOutgoingConverter();
		}

		#endregion

		#region Properties

		public DateTime LastSyncedOn { get; set; }

		public ProfileService Profiler { get; }

		#endregion

		#region Methods

		public static SyncClientIncomingConverter GetIncomingConverter()
		{
			return new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, ClientAccount, int>((update, entity) => { entity.Roles = ClientAccount.CombineRoles(update.Roles); },
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								// We have to manually convert the ignored roles.
								entity.Roles = update.Roles;
								processUpdate();
								return true;
							}
							case SyncObjectStatus.Deleted:
							default:
							{
								processUpdate();
								return true;
							}
						}
					}
				),
				new SyncObjectIncomingConverter<Address, long, ClientAddress, long>(),
				new SyncObjectIncomingConverter<LogEvent, long, ClientLogEvent, long>(),
				new SyncObjectIncomingConverter<Setting, long, ClientSetting, long>()
			);
		}

		public static SyncClientOutgoingConverter GetOutgoingConverter()
		{
			return new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<ClientAccount, int, Account, int>((clientAccount, account) => { account.Roles = ClientAccount.SplitRoles(clientAccount.Roles); }),
				new SyncObjectOutgoingConverter<ClientAddress, long, Address, long>(),
				new SyncObjectOutgoingConverter<ClientLogEvent, long, LogEvent, long>(),
				new SyncObjectOutgoingConverter<ClientSetting, long, Setting, long>()
			);
		}

		public virtual void Initialize()
		{
			Profiler.AverageSyncTimeForAll = GetOrAddSyncTimer(SyncType.All);
			Profiler.AverageSyncTimeForAccounts = GetOrAddSyncTimer(SyncType.Accounts);
			Profiler.AverageSyncTimeForAddress = GetOrAddSyncTimer(SyncType.Address);
			Profiler.AverageSyncTimeForAddresses = GetOrAddSyncTimer(SyncType.Addresses);
			Profiler.AverageSyncTimeForLogEvents = GetOrAddSyncTimer(SyncType.LogEvents);
			Profiler.RuntimeTimer.Start();
		}

		public SyncResults<SyncType> Sync(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			return WaitOnTask(SyncAsync(waitFor, postAction), timeout);
		}

		public SyncResults<SyncType> SyncAccounts(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			return WaitOnTask(SyncAccountsAsync(waitFor, postAction), timeout);
		}

		public Task<SyncResults<SyncType>> SyncAccountsAsync(TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			OnLogEvent("Starting to sync accounts...", EventLevel.Verbose);

			return ProcessAsync(SyncType.Accounts, options => { OnLogEvent("Sync accounts started", EventLevel.Verbose); },
				waitFor,
				postAction);
		}

		public SyncResults<SyncType> SyncAddresses(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			return WaitOnTask(SyncAddressesAsync(waitFor, postAction), timeout);
		}

		public Task<SyncResults<SyncType>> SyncAddressesAsync(TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			OnLogEvent("Starting to sync addresses...", EventLevel.Verbose);

			return ProcessAsync(SyncType.Addresses, options => { OnLogEvent("Sync addresses started", EventLevel.Verbose); },
				waitFor,
				postAction);
		}

		public Task<SyncResults<SyncType>> SyncAsync(TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			OnLogEvent("Starting to sync all...", EventLevel.Verbose);

			return ProcessAsync(SyncType.All, options =>
				{
					options.ResetFilters();
					options.AddSyncableFilter(new SyncRepositoryFilter<ClientAddress>());
					options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>());
					options.AddSyncableFilter(new SyncRepositoryFilter<ClientLogEvent>());
					options.AddSyncableFilter(new SyncRepositoryFilter<ClientSetting>());

					OnLogEvent("Sync all started", EventLevel.Verbose);
				},
				waitFor,
				postAction);
		}

		public SyncResults<SyncType> SyncLogEvents(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			return WaitOnTask(SyncLogEventsAsync(waitFor, postAction), timeout);
		}

		public Task<SyncResults<SyncType>> SyncLogEventsAsync(TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			OnLogEvent("Starting to sync log events...", EventLevel.Verbose);

			return ProcessAsync(SyncType.LogEvents, options => { OnLogEvent("Sync logs events started", EventLevel.Verbose); },
				waitFor,
				postAction);
		}

		public void UpdateLastSyncedOn(SyncType type, DateTime lastSyncedOnClient, DateTime lastSyncedOnServer)
		{
			var syncOptions = GetSyncOptions(type);
			if (syncOptions != null)
			{
				syncOptions.LastSyncedOnClient = lastSyncedOnClient;
				syncOptions.LastSyncedOnServer = lastSyncedOnServer;
			}
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForClient()
		{
			return new SyncClient("Client (local)", _databaseProvider)
			{
				IncomingConverter = IncomingConverter,
				OutgoingConverter = OutgoingConverter,
				Options = { EnablePrimaryKeyCache = true, IsServerClient = false }
			};
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForServer()
		{
			var client = _serverProvider.GetClient("Server (remote)");
			if (client != null)
			{
				client.Options.IsServerClient = true;
			}
			return client;
		}

		protected override SyncOptions GetSyncOptions(SyncType syncType)
		{
			var options = base.GetSyncOptions(syncType);
			if (!options.Values.ContainsKey(AccountValueKey))
			{
				options.Values.AddOrUpdate(AccountValueKey, Guid.Empty.ToString());
			}
			return options;
		}

		protected override void OnSyncCompleted(SyncResults<SyncType> results)
		{
			// If no issues we'll store last synced on
			if (results.SyncSuccessful)
			{
				// Update the last synced on times (client/server)
				UpdateLastSyncedOn(results.SyncType, results.Options.LastSyncedOnClient, results.Options.LastSyncedOnServer);
				LastSyncedOn = results.Options.LastSyncedOnClient;
			}

			base.OnSyncCompleted(results);
		}

		#endregion
	}
}