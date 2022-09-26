#region References

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Logging;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync manager for syncing two clients.
	/// </summary>
	public abstract class SyncManager<T> : Bindable where T : Enum
	{
		#region Fields

		private SyncEngine _engine;
		private readonly object _processLock;
		private readonly T[] _supportedSyncTypes;
		private readonly ConcurrentDictionary<T, SyncOptions> _syncOptions;
		private readonly ConcurrentDictionary<T, SyncTimer> _syncTimers;
		private readonly Stopwatch _watch;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a sync manager for syncing two clients.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		/// <param name="supportedSyncTypes"> </param>
		protected SyncManager(IDispatcher dispatcher, params T[] supportedSyncTypes) : base(dispatcher)
		{
			_supportedSyncTypes = supportedSyncTypes.Length <= 0 ? EnumExtensions.GetValues<T>() : supportedSyncTypes;
			_processLock = new object();
			_watch = new Stopwatch();

			IsEnabled = true;
			ProcessTimeout = TimeSpan.FromMilliseconds(60000);
			ShowProgressThreshold = TimeSpan.FromMilliseconds(1000);
			SyncState = new SyncEngineState(Dispatcher);
			SyncType = default;

			_syncOptions = new ConcurrentDictionary<T, SyncOptions>();
			_syncTimers = new ConcurrentDictionary<T, SyncTimer>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets an optional incoming converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		public SyncClientIncomingConverter IncomingConverter { get; protected set; }

		/// <summary>
		/// Gets a value indicating the running sync is requesting to be cancelled.
		/// </summary>
		public bool IsCancellationPending => _engine?.IsCancellationPending ?? false;

		/// <summary>
		/// Gets a value indicating the sync manager is enabled.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// Gets a value indicating the running status of the sync manager.
		/// </summary>
		public bool IsRunning => _watch.IsRunning && (_engine?.IsRunning == true);

		/// <summary>
		/// Gets a value indicating the running status of the sync manager.
		/// </summary>
		public bool IsStarted => _watch.IsRunning || (_engine != null);

		/// <summary>
		/// Gets an optional outgoing converter to convert incoming sync data. The converter is applied to the local sync client.
		/// </summary>
		public SyncClientOutgoingConverter OutgoingConverter { get; protected set; }

		/// <summary>
		/// The timeout to be used when synchronously syncing.
		/// </summary>
		public TimeSpan ProcessTimeout { get; set; }

		/// <summary>
		/// The session ID of the sync manager.
		/// </summary>
		public Guid SessionId { get; private set; }

		/// <summary>
		/// Gets a flag to indicate progress should be shown. Will only be true if sync takes longer than the <seealso cref="ShowProgressThreshold" />.
		/// </summary>
		public bool ShowProgress => _watch.IsRunning && (_watch.Elapsed >= ShowProgressThreshold);

		/// <summary>
		/// Gets the value to determine when to trigger <seealso cref="ShowProgress" />. Defaults to one second.
		/// </summary>
		public TimeSpan ShowProgressThreshold { get; set; }

		/// <summary>
		/// The configure sync options for the sync manager.
		/// </summary>
		/// <seealso cref="GetOrAddSyncOptions" />
		public ReadOnlyDictionary<T, SyncOptions> SyncOptions => new ReadOnlyDictionary<T, SyncOptions>(_syncOptions);

		/// <summary>
		/// Gets the current sync state.
		/// </summary>
		public SyncEngineState SyncState { get; }

		/// <summary>
		/// The configure sync timers for the sync manager.
		/// </summary>
		public ReadOnlyDictionary<T, SyncTimer> SyncTimers => new ReadOnlyDictionary<T, SyncTimer>(_syncTimers);

		/// <summary>
		/// The type of the sync.
		/// </summary>
		public T SyncType { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Cancels the current running sync.
		/// </summary>
		/// <remarks>
		/// See <seealso cref="WaitForSyncToComplete" /> if you want to wait for the sync to complete.
		/// </remarks>
		public void CancelSync()
		{
			OnLogEvent($"Cancelling running Sync {SyncType}...", EventLevel.Verbose);

			_engine?.Cancel();

			OnPropertyChanged(nameof(IsCancellationPending));
		}

		/// <summary>
		/// Reset the sync dates on all sync options
		/// </summary>
		/// <param name="lastSyncedOnClient"> The last time when synced on the client. </param>
		/// <param name="lastSyncedOnServer"> The last time when synced on the server. </param>
		public void ResetSyncDates(DateTime lastSyncedOnClient, DateTime lastSyncedOnServer)
		{
			foreach (var collectionOptions in _syncOptions.Values)
			{
				collectionOptions.LastSyncedOnClient = lastSyncedOnClient;
				collectionOptions.LastSyncedOnServer = lastSyncedOnServer;
			}
		}

		/// <summary>
		/// Cancel the current running sync and wait for it to stop.
		/// </summary>
		public void StopSync(TimeSpan? timeout = null)
		{
			OnLogEvent($"Stopping running Sync {SyncType}...", EventLevel.Verbose);

			_engine?.Stop(timeout);

			OnPropertyChanged(nameof(IsCancellationPending));
		}

		/// <summary>
		/// Wait for the sync to complete.
		/// </summary>
		/// <param name="timeout"> An optional max amount of time to wait. ProcessTimeout will be used it no timeout provided. </param>
		/// <returns> True if the sync completed otherwise false if timed out waiting. </returns>
		public bool WaitForSyncToComplete(TimeSpan? timeout = null)
		{
			if (!IsRunning)
			{
				return true;
			}

			var watch = Stopwatch.StartNew();
			timeout ??= ProcessTimeout;

			while (IsRunning)
			{
				if (watch.Elapsed >= timeout)
				{
					return false;
				}

				Thread.Sleep(10);
			}

			return true;
		}

		/// <summary>
		/// Wait for the sync to start.
		/// </summary>
		/// <param name="timeout"> An optional max amount of time to wait. ProcessTimeout will be used it no timeout provided. </param>
		/// <returns> True if the sync was started otherwise false if timed out waiting. </returns>
		public bool WaitForSyncToStart(TimeSpan? timeout = null)
		{
			if (IsStarted)
			{
				return true;
			}

			var watch = Stopwatch.StartNew();
			timeout ??= ProcessTimeout;

			while (!IsStarted)
			{
				if (watch.Elapsed >= timeout)
				{
					return false;
				}

				Thread.Sleep(10);
			}

			return true;
		}

		/// <summary>
		/// Wait for the sync to start running.
		/// </summary>
		/// <param name="timeout"> An optional max amount of time to wait. ProcessTimeout will be used it no timeout provided. </param>
		/// <returns> True if the sync was started to process otherwise false if timed out waiting. </returns>
		public bool WaitForSyncToStartRunning(TimeSpan? timeout = null)
		{
			if (IsRunning)
			{
				return true;
			}

			var watch = Stopwatch.StartNew();
			timeout ??= ProcessTimeout;

			while (!IsRunning)
			{
				if (watch.Elapsed >= timeout)
				{
					return false;
				}

				Thread.Sleep(10);
			}

			return true;
		}

		/// <summary>
		/// Gets the default sync options for a sync manager.
		/// </summary>
		/// <param name="syncType"> The type of sync these options are for. </param>
		/// <param name="update"> Optional update action to change provided defaults. </param>
		/// <returns> The default set of options. </returns>
		/// <remarks>
		/// This should only be use in the sync manager constructor.
		/// </remarks>
		protected SyncOptions GetOrAddSyncOptions(T syncType, Action<SyncOptions> update = null)
		{
			return _syncOptions.GetOrAdd(syncType, key =>
			{
				if (SyncOptions.ContainsKey(syncType))
				{
					return SyncOptions[syncType];
				}

				var options = new SyncOptions
				{
					LastSyncedOnClient = DateTime.MinValue,
					LastSyncedOnServer = DateTime.MinValue,
					// note: everything below is a request, the sync clients (web sync controller)
					// has the options to override. Ex: you may request 600 items then the sync
					// client may reduce it to only 100 items.
					PermanentDeletions = false,
					ItemsPerSyncRequest = 600,
					IncludeIssueDetails = false
				};

				options.Values.AddOrUpdate(Sync.SyncOptions.SyncKey, ((int) (object) syncType).ToString());

				// optional update to modify sync options
				update?.Invoke(options);

				_syncOptions.GetOrAdd(syncType, options);

				return options;
			});
		}

		/// <summary>
		/// Gets or adds an average sync timer for a sync type. This will track the average time spent syncing for the provided type.
		/// </summary>
		/// <param name="syncType"> The type of sync these options are for. </param>
		/// <param name="limit"> Optional limit of syncs to average. </param>
		/// <returns> The timer for tracking the time spent syncing. </returns>
		/// <remarks>
		/// This should only be use in the sync manager constructor.
		/// </remarks>
		protected SyncTimer GetOrAddSyncTimer(T syncType, int limit = 10)
		{
			return _syncTimers.GetOrAdd(syncType, new SyncTimer(limit, Dispatcher));
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
		protected virtual SyncOptions GetSyncOptions(T syncType)
		{
			return SyncOptions.ContainsKey(syncType) ? SyncOptions[syncType] : null;
		}

		/// <summary>
		/// Write a message to the log.
		/// </summary>
		/// <param name="message"> The message to be written. </param>
		/// <param name="level"> The level of this message. </param>
		protected virtual void OnLogEvent(string message, EventLevel level)
		{
			Logger.Instance.Write(SessionId, message, level);
		}

		/// <summary>
		/// Indicate the sync is complete.
		/// </summary>
		/// <param name="results"> The results of the completed sync. </param>
		protected virtual void OnSyncCompleted(SyncResults<T> results)
		{
			// If no issues we'll store last synced on
			if (results.SyncSuccessful)
			{
				// Update the last synced on times (client/server)
				var syncOptions = GetSyncOptions(results.SyncType);
				if (syncOptions != null)
				{
					syncOptions.LastSyncedOnClient = results.Options.LastSyncedOnClient;
					syncOptions.LastSyncedOnServer = results.Options.LastSyncedOnServer;
				}
			}

			SyncCompleted?.Invoke(this, results);
		}

		/// <summary>
		/// Indicate the sync is running
		/// </summary>
		protected virtual void OnSyncRunning(SyncResults<T> results)
		{
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
		/// <param name="syncType"> The type of the sync to process. </param>
		/// <param name="updateOptions"> The action to possibly update options when the sync starts. </param>
		/// <param name="waitFor"> Optional timeout to wait for the active sync to complete. </param>
		/// <param name="postAction">
		/// An optional action to run after sync is completed but before notification goes out. If the sync cannot
		/// start then the options will be null as they were never read or set.
		/// </param>
		/// <returns> The task for the process. </returns>
		protected Task<SyncResults<T>> ProcessAsync(T syncType, Action<SyncOptions> updateOptions, TimeSpan? waitFor = null, Action<SyncResults<T>> postAction = null)
		{
			ValidateSyncType(syncType);

			if (!IsEnabled)
			{
				OnLogEvent($"Sync Manager is not enabled so Sync {syncType} not started.", EventLevel.Verbose);
				postAction?.Invoke(null);
				return Task.FromResult(new SyncResults<T>());
			}

			if (IsStarted)
			{
				if (waitFor == null)
				{
					OnLogEvent($"Sync {SyncType} is already running so Sync {syncType} not started.", EventLevel.Verbose);
					postAction?.Invoke(null);
					return Task.FromResult(new SyncResults<T>());
				}

				// See if we are going to force current sync to stop
				OnLogEvent($"Waiting for Sync {SyncType} to complete...", EventLevel.Verbose);
			}

			// Start the sync before we start the task
			var result = StartSync(syncType, waitFor, postAction);
			if (result == null)
			{
				return Task.FromResult(new SyncResults<T>());
			}

			// Start the sync in a background thread.
			return Task.Run(() => RunSync(result, updateOptions))
				.ContinueWith(x => StopSync(x.Result, postAction));
		}

		/// <summary>
		/// Validates the provided sync type is supported by this sync manager.
		/// </summary>
		/// <param name="syncType"> The type of the sync to validate. </param>
		/// <exception cref="ConstraintException"> The sync type is not supported by this sync manager. </exception>
		protected void ValidateSyncType(T syncType)
		{
			if (!_supportedSyncTypes.Contains(syncType))
			{
				throw new ConstraintException("The sync type is not supported by this sync manager.");
			}
		}

		/// <summary>
		/// Wait on a task to be completed.
		/// </summary>
		/// <param name="task"> The task to wait for. </param>
		/// <param name="timeout">
		/// A TimeSpan that represents the number of milliseconds to wait, or
		/// a TimeSpan that represents -1 milliseconds to wait indefinitely.
		/// </param>
		protected SyncResults<T> WaitOnTask(Task<SyncResults<T>> task, TimeSpan? timeout)
		{
			return task.AwaitResults(timeout ?? ProcessTimeout);
		}

		private void OnEngineOnSyncStateChanged(object sender, SyncEngineState state)
		{
			SyncState.UpdateWith(state);
			OnSyncUpdated(state);
			TriggerPropertyChanges();
		}

		/// <summary>
		/// Run the sync. This should only be called by ProcessAsync.
		/// </summary>
		/// <param name="results"> The results for the sync. </param>
		/// <param name="updateOptions"> Update options before running sync. </param>
		private SyncResults<T> RunSync(SyncResults<T> results, Action<SyncOptions> updateOptions)
		{
			try
			{
				updateOptions?.Invoke(results.Options);

				OnLogEvent($"Syncing {results.SyncType} for {results.Options.LastSyncedOnClient}, {results.Options.LastSyncedOnServer}", EventLevel.Verbose);

				results.Client = GetSyncClientForClient();
				results.Server = GetSyncClientForServer();

				if ((results.Client == null) || (results.Server == null))
				{
					throw new SpeedyException("Sync client for client or server is null.");
				}

				_engine = new SyncEngine(results.SessionId, results.Client, results.Server, results.Options);

				OnSyncRunning(results);

				try
				{
					_engine.SyncStateChanged += OnEngineOnSyncStateChanged;
					_engine.Run();
				}
				finally
				{
					_engine.SyncStateChanged -= OnEngineOnSyncStateChanged;
				}

				results.SyncIssues.AddRange(_engine.SyncIssues);
				results.SyncCancelled = IsCancellationPending;
				results.SyncCompleted = _engine.State.Status == SyncEngineStatus.Completed;
				results.SyncSuccessful = results.SyncCompleted
					&& !results.SyncCancelled
					&& !results.SyncIssues.Any();
			}
			catch (WebClientException ex)
			{
				results.SyncSuccessful = false;
				results.SyncCancelled = false;
				results.SyncCompleted = false;
				results.SyncIssues.Add(new SyncIssue
				{
					Id = Guid.Empty,
					IssueType = SyncIssueType.ClientException,
					Message = ex.Message,
					TypeName = string.Empty
				});

				switch (ex.Code)
				{
					case HttpStatusCode.Unauthorized:
					{
						SyncState.Message = "Unauthorized: please update your credentials in settings or contact support.";
						break;
					}
					default:
					{
						SyncState.Message = ex.Message;
						break;
					}
				}

				OnSyncUpdated(SyncState);
			}
			catch (Exception ex)
			{
				results.SyncSuccessful = false;
				results.SyncCancelled = false;
				results.SyncCompleted = false;
				results.SyncIssues.Add(new SyncIssue
				{
					Id = Guid.Empty,
					IssueType = SyncIssueType.ClientException,
					Message = ex.Message,
					TypeName = string.Empty
				});

				SyncState.Message = ex.Message;
				OnSyncUpdated(SyncState);
			}

			return results;
		}

		private SyncResults<T> StartSync(T syncType, TimeSpan? waitFor, Action<SyncResults<T>> postAction)
		{
			// Lock the sync before we start, wait until 
			var syncStarted = WaitForSyncAvailableThenStart(syncType, waitFor ?? TimeSpan.Zero);
			if (!syncStarted)
			{
				OnLogEvent($"Failed to Sync {syncType} because current Sync {SyncType} never completed while waiting.", EventLevel.Verbose);
				postAction?.Invoke(null);
				return null;
			}

			var results = new SyncResults<T>
			{
				SessionId = SessionId,
				SyncStarted = true,
				SyncType = syncType
			};

			SyncType = syncType;

			// See if we have a timer for this sync type
			if (SyncTimers.TryGetValue(results.SyncType, out var syncTimer))
			{
				syncTimer.Start();
			}

			results.Options = GetSyncOptions(results.SyncType);

			TriggerPropertyChanges();

			return results;
		}

		private SyncResults<T> StopSync(SyncResults<T> results, Action<SyncResults<T>> postAction)
		{
			// See if we have a timer for this sync type
			if (SyncTimers.TryGetValue(results.SyncType, out var syncTimer))
			{
				if (results.SyncCancelled)
				{
					syncTimer.CancelledSyncs++;
					syncTimer.Reset();
				}
				else if (results.SyncSuccessful)
				{
					syncTimer.SuccessfulSyncs++;
					syncTimer.Stop();
				}
				else
				{
					syncTimer.FailedSyncs++;
					syncTimer.Stop();
				}

				results.Elapsed = syncTimer.Elapsed;

				OnLogEvent($"Sync {results.SyncType} stopped. {syncTimer.Average:mm\\:ss\\.fff}", EventLevel.Verbose);
			}
			else
			{
				OnLogEvent($"Sync {results.SyncType} stopped", EventLevel.Verbose);
			}

			try
			{
				postAction?.Invoke(results);
			}
			catch (Exception ex)
			{
				OnLogEvent(ex.Message, EventLevel.Error);
			}

			try
			{
				OnSyncCompleted(results);
			}
			catch (Exception ex)
			{
				OnLogEvent(ex.Message, EventLevel.Error);
			}

			_engine?.Dispose();
			_engine = null;
			_watch.Stop();

			TriggerPropertyChanges();

			return results;
		}

		private void TriggerPropertyChanges()
		{
			Dispatcher?.RunAsync(() =>
			{
				OnPropertyChanged(nameof(IsCancellationPending));
				OnPropertyChanged(nameof(IsRunning));
				OnPropertyChanged(nameof(IsStarted));
				OnPropertyChanged(nameof(ShowProgress));
			});
		}

		private bool WaitForSyncAvailableThenStart(T syncType, TimeSpan timeout)
		{
			var watch = Stopwatch.StartNew();

			do
			{
				// Lock to see if we can start a sync
				if (!Monitor.TryEnter(_processLock))
				{
					Thread.Sleep(10);
					continue;
				}

				try
				{
					// Check to see if a sync is already running
					if (!IsStarted)
					{
						// No sync running, start a new sync by starting the watch
						_watch.Restart();
						SessionId = Guid.NewGuid();
						OnLogEvent($"Sync {syncType} started", EventLevel.Verbose);
						return true;
					}
				}
				finally
				{
					// Free up the lock
					Monitor.Exit(_processLock);
				}

				// Pause to allow locks to be handed out
				Thread.Sleep(10);

				// Wait for an existing sync to completed until the provided timeout
			} while (watch.Elapsed < timeout);

			// The sync is still running so return false
			return false;
		}

		#endregion

		#region Events

		/// <summary>
		/// Indicates the sync is completed.
		/// </summary>
		public event EventHandler<SyncResults<T>> SyncCompleted;

		/// <summary>
		/// Indicates the sync is being updated.
		/// </summary>
		public event EventHandler<SyncEngineState> SyncUpdated;

		#endregion
	}
}