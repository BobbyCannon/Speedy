﻿#region References

using System;
using System.Diagnostics.Tracing;
using System.Net;
using System.Threading.Tasks;
using Speedy.Data.Client;
using Speedy.Data.WebApi;
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

			IncomingConverter = new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, ClientAccount, int>((account, clientAccount) => clientAccount.Roles = ClientAccount.CombineRoles(account.Roles)),
				new SyncObjectIncomingConverter<Address, long, ClientAddress, long>(),
				new SyncObjectIncomingConverter<LogEvent, long, ClientLogEvent, long>(),
				new SyncObjectIncomingConverter<Setting, long, ClientSetting, long>()
			);

			OutgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<ClientAccount, int, Account, int>((clientAccount, account) => account.Roles = ClientAccount.SplitRoles(clientAccount.Roles)),
				new SyncObjectOutgoingConverter<ClientAddress, long, Address, long>(),
				new SyncObjectOutgoingConverter<ClientLogEvent, long, LogEvent, long>(),
				new SyncObjectOutgoingConverter<ClientSetting, long, Setting, long>()
			);
		}

		#endregion

		#region Methods

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

			return ProcessAsync(SyncType.Accounts, options =>
				{
					OnLogEvent("Sync accounts started", EventLevel.Verbose);
				},
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

			return ProcessAsync(SyncType.Addresses, options =>
				{
					OnLogEvent("Sync addresses started", EventLevel.Verbose);
				},
				waitFor,
				postAction);
		}

		public Task<SyncResults<SyncType>> SyncAsync(TimeSpan? waitFor = null, Action<SyncResults<SyncType>> postAction = null)
		{
			OnLogEvent("Starting to sync all...", EventLevel.Verbose);

			return ProcessAsync(SyncType.All, options =>
				{
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

			return ProcessAsync(SyncType.LogEvents, options =>
				{
					OnLogEvent("Sync logs events started", EventLevel.Verbose);
				},
				waitFor,
				postAction);
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForClient()
		{
			return new SyncClient("Client (local)", _databaseProvider) { IncomingConverter = IncomingConverter, OutgoingConverter = OutgoingConverter };
		}

		/// <inheritdoc />
		protected override ISyncClient GetSyncClientForServer()
		{
			return _serverProvider.GetClient("Server (remote)", _credentialProvider());
		}

		#endregion
	}
}