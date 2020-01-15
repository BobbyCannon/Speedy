#region References

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Exceptions;

#endregion

namespace Speedy.Sync
{
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

		#region Fields

		private CancellationTokenSource _cancellationToken;
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

			ProcessTimeout = TimeSpan.FromMilliseconds(60000);
			SyncIssues = new List<SyncIssue>();
			SyncState = new SyncEngineState();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating the running status of the sync manager.
		/// </summary>
		public bool IsRunning => _watch.IsRunning;

		/// <summary>
		/// Gets a value indicating if the last sync was successful.
		/// </summary>
		public bool IsSyncSuccessful { get; set; }

		public TimeSpan ProcessTimeout { get; }

		public bool ShowProgress => _watch.IsRunning && _watch.Elapsed.TotalMilliseconds > 1000;

		public IList<SyncIssue> SyncIssues { get; }

		public SyncEngineState SyncState { get; protected set; }

		/// <summary>
		/// The version of the sync system. Update this version any time the sync system changed dramatically
		/// </summary>
		public abstract Version SyncSystemVersion { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Initialize the sync manager.
		/// </summary>
		public abstract void Initialize();

		/// <summary>
		/// Gets the default sync options for a sync manager.
		/// </summary>
		/// <param name="type"> The type of sync these options are for. </param>
		/// <param name="update"> Optional update action to change provided defaults. </param>
		/// <returns> The default set of options. </returns>
		protected SyncOptions GetDefaultSyncOptions<T>(T type, Action<SyncOptions> update = null) where T : Enum
		{
			var options = new SyncOptions
			{
				Id = type.ToString(),
				LastSyncedOnClient = DateTime.MinValue,
				LastSyncedOnServer = DateTime.MinValue,
				// note: everything below is a request, the sync clients (web sync controller)
				// has the options to override. Ex: you may request 300 items then the sync
				// client may reduce it to only 100 items.
				PermanentDeletions = false,
				ItemsPerSyncRequest = 300,
				IncludeIssueDetails = true
			};

			options.Values.Add(SyncKey, ((int) (object) type).ToString());
			options.Values.Add(SyncVersionKey, SyncSystemVersion.ToString(4));

			// optional update to modify sync options
			update?.Invoke(options);

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
		/// Processes a sync request
		/// </summary>
		/// <param name="getOptions"> The action to retrieve the options when the task starts. </param>
		/// <param name="waitFor"> Optional timeout to wait for the active sync to complete. </param>
		/// <param name="postAction"> An optional action to run after sync is completed but before notification goes out. </param>
		/// <param name="force"> An optional flag to ignore IsShuttingDown state. Defaults to false. </param>
		/// <returns> </returns>
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
}