#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Exceptions;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync manager for syncing two clients.
	/// </summary>
	public abstract class SyncManager<T> : SyncManager where T : Enum
	{
		#region Fields

		private CancellationTokenSource _cancellationToken;
		private readonly Dictionary<T, SyncOptions> _syncOptions;
		private readonly Stopwatch _watch;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync manager for syncing two clients.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected SyncManager(IDispatcher dispatcher) : base(dispatcher)
		{
			_watch = new Stopwatch();
			_syncOptions = new Dictionary<T, SyncOptions>();

			ProcessTimeout = TimeSpan.FromMilliseconds(60000);
			ShowProgressThreshold = TimeSpan.FromMilliseconds(1000);
			SyncIssues = new List<SyncIssue>();
			SyncState = new SyncEngineState();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets an optional incoming converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		public SyncClientIncomingConverter IncomingConverter { get; protected set; }

		/// <summary>
		/// Gets a value indicating the running status of the sync manager.
		/// </summary>
		public bool IsRunning => _watch.IsRunning;

		/// <summary>
		/// Gets a value indicating if the last sync was successful.
		/// </summary>
		public bool IsSyncSuccessful { get; protected set; }

		/// <summary>
		/// Gets an optional outgoing converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		public SyncClientOutgoingConverter OutgoingConverter { get; protected set; }

		/// <summary>
		/// The timeout to be used when synchronously syncing.
		/// </summary>
		public TimeSpan ProcessTimeout { get; set; }

		/// <summary>
		/// Gets a flag to indicate progress should be shown. Will only be true if sync takes longer than the <seealso cref="ShowProgressThreshold" />.
		/// </summary>
		public bool ShowProgress => _watch.IsRunning && _watch.Elapsed >= ShowProgressThreshold;

		/// <summary>
		/// Gets the value to determine when to trigger <seealso cref="ShowProgress" />. Defaults to one second.
		/// </summary>
		public TimeSpan ShowProgressThreshold { get; set; }

		/// <summary>
		/// Gets the list of issues that occurred during the last sync.
		/// </summary>
		public IList<SyncIssue> SyncIssues { get; }

		/// <summary>
		/// The configure sync options for the sync manager.
		/// </summary>
		/// <seealso cref="AddSyncOptions" />
		public IReadOnlyCollection<SyncOptions> SyncOptions => _syncOptions.Values;

		/// <summary>
		/// Gets the current sync state.
		/// </summary>
		public SyncEngineState SyncState { get; protected set; }

		/// <summary>
		/// The version of the sync system. Update this version any time the sync system changed dramatically
		/// </summary>
		public abstract Version SyncSystemVersion { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Reset the sync dates on all sync options
		/// </summary>
		/// <param name="lastSyncedOnClient"> The last time when synced on the client. </param>
		/// <param name="lastSyncedOnServer"> The last time when synced on the server. </param>
		public void ResetSyncDates(DateTime lastSyncedOnClient, DateTime lastSyncedOnServer)
		{
			foreach (var collectionOptions in SyncOptions)
			{
				collectionOptions.LastSyncedOnClient = lastSyncedOnClient;
				collectionOptions.LastSyncedOnServer = lastSyncedOnServer;
			}
		}

		/// <summary>
		/// Gets the default sync options for a sync manager.
		/// </summary>
		/// <param name="syncType"> The type of sync these options are for. </param>
		/// <param name="update"> Optional update action to change provided defaults. </param>
		/// <returns> The default set of options. </returns>
		protected SyncOptions AddSyncOptions(T syncType, Action<SyncOptions> update = null)
		{
			if (_syncOptions.ContainsKey(syncType))
			{
				return _syncOptions[syncType];
			}

			var options = new SyncOptions
			{
				Id = syncType.ToString(),
				LastSyncedOnClient = DateTime.MinValue,
				LastSyncedOnServer = DateTime.MinValue,
				// note: everything below is a request, the sync clients (web sync controller)
				// has the options to override. Ex: you may request 300 items then the sync
				// client may reduce it to only 100 items.
				PermanentDeletions = false,
				ItemsPerSyncRequest = 300,
				IncludeIssueDetails = false
			};

			options.Values.AddOrUpdate(SyncKey, ((int) (object) syncType).ToString());
			options.Values.AddOrUpdate(SyncVersionKey, SyncSystemVersion.ToString(4));

			// optional update to modify sync options
			update?.Invoke(options);

			_syncOptions.Add(syncType, options);

			return options;
		}

		/// <summary>
		/// Gets the sync client to be used in the sync engine client input.
		/// </summary>
		/// <returns> The sync client. </returns>
		protected abstract ISyncClient GetSyncClientForClient();

		/// <summary>
		/// Gets the sync client to be used in the sync engine client input.
		/// </summary>
		/// <returns> The sync client. </returns>
		protected abstract ISyncClient GetSyncClientForServer();

		/// <summary>
		/// Gets the sync options by the provide sync type.
		/// </summary>
		/// <param name="syncType"> The sync type to get options for. </param>
		/// <returns> The sync options for the type </returns>
		protected SyncOptions GetSyncOptions(T syncType)
		{
			return _syncOptions.ContainsKey(syncType) ? _syncOptions[syncType] : null;
		}

		/// <summary>
		/// Indicate that there is an event to being logged by the sync manager.
		/// </summary>
		/// <param name="message"> The message to be logged. </param>
		protected virtual void OnLogEvent(string message)
		{
			LogEvent?.Invoke(this, message);
		}

		/// <summary>
		/// Indicate the sync is complete.
		/// </summary>
		/// <param name="options"> The options of the completed sync. </param>
		protected virtual void OnSyncCompleted(SyncOptions options)
		{
			SyncCompleted?.Invoke(this, options);
		}

		/// <summary>
		/// Indicate the sync is being updated.
		/// </summary>
		/// <param name="state"> The state of the sync. </param>
		protected virtual void OnSyncUpdated(SyncEngineState state)
		{
			SyncUpdated?.Invoke(this, state);
		}

		/// <summary>
		/// Processes a sync request.
		/// </summary>
		/// <param name="getOptions"> The action to retrieve the options when the task starts. </param>
		/// <param name="waitFor"> Optional timeout to wait for the active sync to complete. </param>
		/// <param name="postAction"> An optional action to run after sync is completed but before notification goes out. </param>
		/// <param name="force"> An optional flag to ignore IsShuttingDown state. Defaults to false. </param>
		/// <returns> The task for the process. </returns>
		protected Task ProcessAsync(Func<SyncOptions> getOptions, TimeSpan? waitFor = null, Action<SyncOptions> postAction = null, bool force = false)
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

					var client = GetSyncClientForClient();
					var server = GetSyncClientForServer();
					var engine = new SyncEngine(client, server, options, _cancellationToken);

					engine.SyncStateChanged += async (sender, state) =>
					{
						SyncState = state;
						OnSyncUpdated(state);

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
					OnSyncUpdated(SyncState);
				}
				catch (Exception ex)
				{
					IsSyncSuccessful = false;
					SyncState.Message = ex.Message;
					OnSyncUpdated(SyncState);
				}
				finally
				{
					OnSyncCompleted(options);
					StopSync();
				}
			}, _cancellationToken.Token);
		}

		/// <summary>
		/// Wait on a task to be completed.
		/// </summary>
		/// <param name="task"> The task to wait for. </param>
		/// <param name="timeout">
		/// A TimeSpan that represents the number of milliseconds to wait, or
		/// a TimeSpan that represents -1 milliseconds to wait indefinitely.
		/// </param>
		protected void WaitOnTask(Task task, TimeSpan? timeout)
		{
			Task.WaitAll(new[] { task }, timeout ?? ProcessTimeout);
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

		#endregion

		#region Events

		/// <summary>
		/// Indicates that there is an event to be logged by the sync manager.
		/// </summary>
		public event EventHandler<string> LogEvent;

		/// <summary>
		/// Indicates the sync is completed.
		/// </summary>
		public event EventHandler<SyncOptions> SyncCompleted;

		/// <summary>
		/// Indicates the sync is being updated.
		/// </summary>
		public event EventHandler<SyncEngineState> SyncUpdated;

		#endregion
	}

	/// <summary>
	/// Represents a sync manager for syncing two clients.
	/// </summary>
	public abstract class SyncManager : Bindable
	{
		#region Constants

		/// <summary>
		/// The sync key value. This will be included in the default sync options values.
		/// </summary>
		public const string SyncKey = "SyncKey";

		/// <summary>
		/// The sync version key value. This will be included in the default sync options values.
		/// </summary>
		public const string SyncVersionKey = "SyncVersionKey";

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync manager for syncing two clients.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		internal SyncManager(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion
	}
}