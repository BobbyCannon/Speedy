#region References

using System;
using System.Net;
using System.Threading.Tasks;
using Speedy.Data.Client;
using Speedy.Data.WebApi;
using Speedy.Net;
using Speedy.Sync;

#endregion

namespace Speedy.Data
{
	public class ClientSyncManager : SyncManager
	{
		#region Constants

		public const string AccountValueKey = "Account";
		public const string AddressValueKey = "Address";
		public const string DefaultWebsiteUri = "https://speedy.local";

		#endregion

		#region Fields

		private readonly SyncOptions _accountsSyncOptions, _addressesSyncOptions, _syncOptions;
		private readonly Func<NetworkCredential> _credentialProvider;
		private readonly ISyncableDatabaseProvider _databaseProvider;
		private readonly SyncClientProvider _serverProvider;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync manager for syncing two clients.
		/// </summary>
		/// <param name="credentialProvider"> A provider for network credentials. Used for the sync client provider. </param>
		/// <param name="databaseProvider"> The database provider. </param>
		/// <param name="serverProvider"> The server provider to get a sync client. </param>
		/// <param name="dispatcher"> An optional dispatcher to update with. </param>
		public ClientSyncManager(Func<NetworkCredential> credentialProvider, ISyncableDatabaseProvider databaseProvider, SyncClientProvider serverProvider, IDispatcher dispatcher) : base(dispatcher)
		{
			_credentialProvider = credentialProvider;
			_databaseProvider = databaseProvider;
			_serverProvider = serverProvider;

			// These options are for syncing full collections
			_accountsSyncOptions = GetDefaultSyncOptions(SyncType.Accounts, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>()));
			_addressesSyncOptions = GetDefaultSyncOptions(SyncType.Addresses, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientAddress>()));

			_syncOptions = GetDefaultSyncOptions(SyncType.All, options =>
			{
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientAddress>());
				options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>());
			});
		}

		#endregion

		#region Properties

		public DateTime LastSyncedOnClient { get; set; }

		public DateTime LastSyncedOnServer { get; set; }

		/// <inheritdoc />
		public override Version SyncSystemVersion => new Version(1, 0, 0, 0);

		/// <summary>
		/// Gets an optional incoming converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		private SyncClientIncomingConverter IncomingConverter { get; set; }

		/// <summary>
		/// Gets an optional outgoing converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		private SyncClientOutgoingConverter OutgoingConverter { get; set; }

		#endregion

		#region Methods

		public static SyncClientProvider GetWebSyncProvider()
		{
			var nullDatabaseProvider = new SyncDatabaseProvider(x => null);
			var syncClientProvider = new SyncClientProvider((name, credential) => new WebSyncClient(name, nullDatabaseProvider, DefaultWebsiteUri, "api/Sync", credential, 60000));
			return syncClientProvider;
		}

		public override void Initialize()
		{
			_syncOptions.LastSyncedOnClient = LastSyncedOnClient;
			_syncOptions.LastSyncedOnServer = LastSyncedOnServer;

			IncomingConverter = new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, ClientAccount, int>((account, clientAccount) => clientAccount.Roles = ClientAccount.CombineRoles(account.Roles)),
				new SyncObjectIncomingConverter<Address, long, ClientAddress, long>()
			);

			OutgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<ClientAccount, int, Account, int>((clientAccount, account) => account.Roles = ClientAccount.SplitRoles(clientAccount.Roles)),
				new SyncObjectOutgoingConverter<ClientAddress, long, Address, long>()
			);
		}

		public virtual void Reset()
		{
			LastSyncedOnClient = DateTime.MinValue;
			LastSyncedOnServer = DateTime.MinValue;

			_accountsSyncOptions.LastSyncedOnClient = LastSyncedOnClient;
			_accountsSyncOptions.LastSyncedOnServer = LastSyncedOnServer;
		}

		public void ResetSyncDates()
		{
			_syncOptions.LastSyncedOnClient = DateTime.MinValue;
			_syncOptions.LastSyncedOnServer = DateTime.MinValue;
		}

		public void SyncAccounts(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
		{
			WaitOnTask(SyncAccountsAsync(waitFor, postAction, force), timeout);
		}

		public Task SyncAccountsAsync(TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
		{
			OnLogEvent("Starting to sync accounts...");

			return ProcessAsync(() =>
				{
					OnLogEvent("Sync accounts started");
					return _accountsSyncOptions;
				},
				waitFor,
				postAction,
				force);
		}
		
		public void SyncAddresses(TimeSpan? timeout = null, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
		{
			WaitOnTask(SyncAddressesAsync(waitFor, postAction, force), timeout);
		}

		public Task SyncAddressesAsync(TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
		{
			OnLogEvent("Starting to sync addresses...");

			return ProcessAsync(() =>
				{
					OnLogEvent("Sync addresses started");
					return _addressesSyncOptions;
				},
				waitFor,
				postAction,
				force);
		}

		public void Sync(TimeSpan? timeout = null, TimeSpan? waitFor = null, bool force = false)
		{
			WaitOnTask(SyncAsync(waitFor, force), timeout);
		}

		public Task SyncAsync(TimeSpan? waitFor = null, bool force = false)
		{
			_syncOptions.LastSyncedOnClient = LastSyncedOnClient;
			_syncOptions.LastSyncedOnServer = LastSyncedOnServer;

			return ProcessAsync(() => _syncOptions, waitFor, null, force);
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForClient()
		{
			return new SyncClient("Client (local)", _databaseProvider) { IncomingConverter = IncomingConverter, OutgoingConverter = OutgoingConverter };
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForServer()
		{
			return _serverProvider.GetClient("Server (web)", _credentialProvider());
		}

		#endregion
	}
}