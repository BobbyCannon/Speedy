#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Data.Client;
using Speedy.Data.WebApi;
using Speedy.Exceptions;
using Speedy.Net;
using Speedy.Sync;

#endregion

namespace Speedy.Data
{
	public class SyncManager : Bindable
	{
		#region Constants

		public const string AccountValueKey = "Account";
		public const string DefaultWebsiteUri = "https://speedy.local";
		public const string SyncKey = "SyncKey";

		#endregion

		#region Fields

		private readonly SyncOptions _accountSyncOptions, _accountsSyncOptions, _syncOptions;
		private CancellationTokenSource _cancellationToken;
		private readonly Func<NetworkCredential> _credentialProvider;
		private readonly ISyncableDatabaseProvider _databaseProvider;
		private readonly SyncClientProvider _syncClientProvider;
		private readonly Stopwatch _watch;

		#endregion

		#region Constructors

		public SyncManager(Func<NetworkCredential> credentialProvider, ISyncableDatabaseProvider databaseProvider, SyncClientProvider syncClientProvider, IDispatcher dispatcher) : base(dispatcher)
		{
			ProcessTimeout = TimeSpan.FromMilliseconds(60000);
			SyncIssues = new List<SyncIssue>();
			SyncState = new SyncEngineState();

			_credentialProvider = credentialProvider;
			_databaseProvider = databaseProvider;
			_syncClientProvider = syncClientProvider;
			_watch = new Stopwatch();

			// These options are just the default, other required parameters will be setup before sync
			// NOTE: do not forget to update Reset().
			_accountSyncOptions = GetDefaultSyncOptions(SyncType.Account);

			// These options are for syncing full collections
			_accountsSyncOptions = GetDefaultSyncOptions(SyncType.Accounts, options => options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>()));

			_syncOptions = GetDefaultSyncOptions(SyncType.All, options => { options.AddSyncableFilter(new SyncRepositoryFilter<ClientAccount>()); });
		}

		static SyncManager()
		{
			// Update this version any time the sync system changed dramatically
			SyncSystemVersion = new Version(1, 0, 0, 0);
		}

		#endregion

		#region Properties

		public bool IsRunning => _watch.IsRunning;

		public bool IsSyncSuccessful { get; set; }

		public DateTime LastSyncedOnClient { get; set; }

		public DateTime LastSyncedOnServer { get; set; }

		public TimeSpan ProcessTimeout { get; }

		public bool ShowProgress => _watch.IsRunning && _watch.Elapsed.TotalMilliseconds > 1000;

		public IList<SyncIssue> SyncIssues { get; }

		public SyncEngineState SyncState { get; private set; }

		public static Version SyncSystemVersion { get; }

		protected SyncClientIncomingConverter IncomingConverter { get; set; }

		protected SyncClientOutgoingConverter OutgoingConverter { get; set; }

		#endregion

		#region Methods

		public static SyncClientProvider GetWebSyncProvider()
		{
			var nullDatabaseProvider = new SyncDatabaseProvider(x => null);
			var syncClientProvider = new SyncClientProvider((name, credential) => new WebSyncClient(name, nullDatabaseProvider, DefaultWebsiteUri, "api/Sync", credential, 60000));
			return syncClientProvider;
		}

		public virtual void Initialize()
		{
			_syncOptions.LastSyncedOnClient = LastSyncedOnClient;
			_syncOptions.LastSyncedOnServer = LastSyncedOnServer;

			IncomingConverter = new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, ClientAccount, int>(),
				new SyncObjectIncomingConverter<Address, long, ClientAddress, long>()
			);

			OutgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<ClientAccount, int, Account, int>(),
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

		public bool WaitForSyncToComplete(TimeSpan timeout)
		{
			if (!IsRunning)
			{
				return true;
			}

			var watch = Stopwatch.StartNew();

			while (IsRunning)
			{
				if (watch.Elapsed >= timeout)
				{
					return false;
				}

				Thread.Sleep(25);
			}

			return true;
		}

		protected virtual void OnSyncCompleted(SyncOptions options)
		{
			SyncCompleted?.Invoke(this, options);
		}

		private SyncOptions GetDefaultSyncOptions(SyncType type, Action<SyncOptions> update = null)
		{
			var options = new SyncOptions
			{
				Id = type.ToString(),
				LastSyncedOnClient = DateTime.MinValue,
				LastSyncedOnServer = DateTime.MinValue,
				PermanentDeletions = false,
				ItemsPerSyncRequest = 300,
				IncludeIssueDetails = true
			};

			options.Values.Add(SyncKey, ((int) type).ToString());
			update?.Invoke(options);

			return options;
		}

		private void OnLogEvent(string message)
		{
			LogEvent?.Invoke(this, message);
		}

		/// <summary>
		/// Processes a sync request
		/// </summary>
		/// <param name="getOptions"> The action to retrieve the options when the task starts. </param>
		/// <param name="waitFor"> Optional timeout to wait for the active sync to complete. </param>
		/// <param name="postAction"> An optional action to run after sync is completed but before notification goes out. </param>
		/// <param name="force"> An optional flag to ignore IsShuttingDown state. Defaults to false. </param>
		/// <returns> </returns>
		private Task ProcessAsync(Func<SyncOptions> getOptions, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
		{
			if (IsRunning)
			{
				if (waitFor == null)
				{
					return Task.CompletedTask;
				}

				OnLogEvent("Waiting for active sync to complete...");

				var wait = Stopwatch.StartNew();

				if (IsRunning && wait.Elapsed < waitFor.Value)
				{
					Thread.Sleep(25);
				}

				// See if we are still running and if we just timed out waiting for the active sync to complete
				if (IsRunning)
				{
					if (!force)
					{
						OnLogEvent("Failed to sync because active sync never completed.");
						return Task.CompletedTask;
					}

					OnLogEvent("Cancelling active sync...");

					_cancellationToken.Cancel();

					while (IsRunning)
					{
						Thread.Sleep(25);
					}

					OnLogEvent("Active sync cancelled");
				}
			}

			_cancellationToken = new CancellationTokenSource();

			return Task.Run(() =>
			{
				SyncOptions options = null;

				// Start with sync failed so only successful sync will work
				IsSyncSuccessful = false;

				try
				{
					// Will ensure the Hub is connected
					StartSync();

					options = getOptions();

					var client = new SyncClient("Client (local)", _databaseProvider) { IncomingConverter = IncomingConverter, OutgoingConverter = OutgoingConverter };
					var server = _syncClientProvider.GetClient("Server (web)", _credentialProvider());
					var engine = new SyncEngine(client, server, options, _cancellationToken);

					engine.SyncStateChanged += async (sender, state) =>
					{
						SyncState = state;
						SyncUpdated?.Invoke(this, state);

						if (Dispatcher != null)
						{
							await Dispatcher.RunAsync(() =>
							{
								OnPropertyChanged(nameof(IsRunning));
								OnPropertyChanged(nameof(ShowProgress));
							});
						}
					};

					engine.Run();

					SyncIssues.AddRange(engine.SyncIssues);
					IsSyncSuccessful = !SyncIssues.Any();

					postAction?.Invoke(options);
				}
				catch (WebClientException ex)
				{
					switch (ex.Code)
					{
						case HttpStatusCode.Unauthorized:
							SyncState.Message = "Unauthorized: please update your credentials in settings or contact support.";
							break;

						default:
							SyncState.Message = ex.Message;
							break;
					}

					IsSyncSuccessful = false;
					SyncUpdated?.Invoke(this, SyncState);
				}
				catch (Exception ex)
				{
					IsSyncSuccessful = false;
					SyncState.Message = ex.Message;
					SyncUpdated?.Invoke(this, SyncState);
				}
				finally
				{
					OnSyncCompleted(options);
					StopSync();
				}
			}, _cancellationToken.Token);
		}

		private void StartSync()
		{
			SyncIssues.Clear();

			_watch.Restart();

			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ShowProgress));
		}

		private void StopSync()
		{
			_watch.Stop();

			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ShowProgress));
		}

		private void WaitOnTask(Task task, TimeSpan? timeout)
		{
			Task.WaitAll(new[] { task }, timeout ?? ProcessTimeout);
		}

		#endregion

		#region Events

		public event EventHandler<string> LogEvent;
		public event EventHandler<SyncOptions> SyncCompleted;
		public event EventHandler<SyncEngineState> SyncUpdated;

		#endregion
	}
}