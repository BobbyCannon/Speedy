#region References

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Profiling;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a sync manager for syncing two clients.
	/// </summary>
	public abstract class SyncManager<T> : Bindable where T : Enum
	{
		#region Fields

		private CancellationTokenSource _cancellationToken;
		private readonly ConcurrentDictionary<T, SyncOptions> _syncOptions;
		private SyncTimer _syncTimer;
		private readonly ConcurrentDictionary<T, SyncTimer> _syncTimers;
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

			IsEnabled = true;
			ProcessTimeout = TimeSpan.FromMilliseconds(60000);
			SessionId = Guid.NewGuid();
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
		public bool IsCancellationPending => _cancellationToken?.IsCancellationRequested ?? false;

		/// <summary>
		/// Gets a value indicating the sync manager is enabled.
		/// </summary>
		public bool IsEnabled { get; set; }

		/// <summary>
		/// Gets a value indicating the running status of the sync manager.
		/// </summary>
		public bool IsRunning => ((_cancellationToken != null) && !_cancellationToken.IsCancellationRequested) || _watch.IsRunning;

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
		public Guid SessionId { get; set; }

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

			_cancellationToken?.Cancel();

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
		protected AverageTimer GetOrAddSyncTimer(T syncType, int limit = 10)
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
			SyncCompleted?.Invoke(this, results);
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
			if (!IsEnabled)
			{
				OnLogEvent($"Sync Manager is not enabled so Sync {syncType} not started.", EventLevel.Verbose);
				postAction?.Invoke(null);
				return Task.FromResult(new SyncResults<T>());
			}

			if (IsRunning)
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

			// Lock the sync before we start, wait until 
			var syncRunning = WaitForSyncAvailableThenStart(waitFor ?? TimeSpan.Zero);
			if (!syncRunning)
			{
				OnLogEvent($"Failed to Sync {syncType} because current Sync {SyncType} never completed while waiting.", EventLevel.Verbose);
				postAction?.Invoke(null);
				return Task.FromResult(new SyncResults<T>());
			}

			// Start the sync before we start the task
			var result = new SyncResults<T> { SyncType = syncType };
			result.Options = StartSync(result);

			// Start the sync in a background thread.
			return Task.Run(() => RunSync(result, updateOptions), _cancellationToken.Token)
				.ContinueWith(x =>
				{
					result.SyncCancelled |= x.IsCanceled;
					return StopSync(result, postAction);
				});
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
			Task.WaitAll(new Task[] { task }, timeout ?? ProcessTimeout);
			return task.Result;
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
					throw new Exception("Sync client for client or server is null.");
				}

				var engine = new SyncEngine(results.Client, results.Server, results.Options, _cancellationToken);
				engine.SyncStateChanged += async (sender, state) =>
				{
					SyncState.UpdateWith(state);
					OnSyncUpdated(state);

					if (Dispatcher != null)
					{
						await Dispatcher.RunAsync(() =>
						{
							OnPropertyChanged(nameof(IsCancellationPending));
							OnPropertyChanged(nameof(IsRunning));
							OnPropertyChanged(nameof(ShowProgress));
						});
					}
				};

				engine.Run();

				results.SyncIssues.AddRange(engine.SyncIssues);
				results.SyncSuccessful = !_cancellationToken.IsCancellationRequested && !results.SyncIssues.Any();
				results.SyncCancelled = _cancellationToken.IsCancellationRequested;
				results.SyncCompleted = true;
			}
			catch (WebClientException ex)
			{
				results.SyncSuccessful = false;
				results.SyncCancelled = false;
				results.SyncIssues.Add(new SyncIssue
				{
					Id = Guid.Empty,
					IssueType = SyncIssueType.ClientException,
					Message = ex.Message,
					TypeName = string.Empty
				});

				SyncState.Message = ex.Code switch
				{
					HttpStatusCode.Unauthorized => "Unauthorized: please update your credentials in settings or contact support.",
					_ => ex.Message
				};

				OnSyncUpdated(SyncState);
			}
			catch (Exception ex)
			{
				results.SyncSuccessful = false;
				results.SyncCancelled = false;
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

		private SyncOptions StartSync(SyncResults<T> results)
		{
			// See if we have a timer for this sync type
			if (SyncTimers.TryGetValue(results.SyncType, out _syncTimer))
			{
				_syncTimer.Start();
			}

			OnLogEvent($"Sync {results.SyncType} started", EventLevel.Verbose);

			SyncType = results.SyncType;
			_cancellationToken = new CancellationTokenSource();
			_watch.Restart();

			var options = GetSyncOptions(results.SyncType);

			OnPropertyChanged(nameof(IsCancellationPending));
			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ShowProgress));

			return options;
		}

		private SyncResults<T> StopSync(SyncResults<T> results, Action<SyncResults<T>> postAction)
		{
			if (_syncTimer != null)
			{
				if (results.SyncCancelled)
				{
					_syncTimer.CancelledSyncs++;
					_syncTimer.Reset();
				}
				else if (results.SyncSuccessful)
				{
					_syncTimer.SuccessfulSyncs++;
					_syncTimer.Stop();
				}
				else
				{
					_syncTimer.FailedSyncs++;
					_syncTimer.Stop();
				}

				results.Elapsed = _syncTimer.Elapsed;

				OnLogEvent($"Sync {results.SyncType} stopped. {_syncTimer.Average:mm\\:ss\\.fff}", EventLevel.Verbose);

				_syncTimer = null;
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

			_watch.Stop();
			_cancellationToken?.Dispose();
			_cancellationToken = null;

			OnPropertyChanged(nameof(IsCancellationPending));
			OnPropertyChanged(nameof(IsRunning));
			OnPropertyChanged(nameof(ShowProgress));

			return results;
		}

		private bool WaitForSyncAvailableThenStart(TimeSpan timeout)
		{
			// Wait for an existing sync
			while (IsRunning && (_watch.Elapsed < timeout))
			{
				Thread.Sleep(10);
			}

			// See if we have timed out, if so just return false
			if (IsRunning)
			{
				// The sync is still running so return false
				return false;
			}

			_watch.Start();

			return true;
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