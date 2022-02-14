#region References

using System;
using System.Collections.Generic;
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
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync engine.
	/// </summary>
	public class SyncEngine : IDisposable
	{
		#region Fields

		private readonly CancellationTokenSource _cancellationSource;
		private readonly List<SyncIssue> _syncIssues;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the sync engine.
		/// </summary>
		/// <param name="sessionId"> The ID of the session. </param>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		public SyncEngine(Guid sessionId, ISyncClient client, ISyncClient server, SyncOptions options)
		{
			_cancellationSource = new CancellationTokenSource();
			_syncIssues = new List<SyncIssue>();

			IsRunning = false;
			SessionId = sessionId;
			Client = client;
			Server = server;
			State = new SyncEngineState();
			Options = options;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The client.
		/// </summary>
		public ISyncClient Client { get; }

		/// <summary>
		/// Gets a value indicating the running sync is requesting to be cancelled.
		/// </summary>
		public bool IsCancellationPending => _cancellationSource?.IsCancellationRequested ?? false;

		/// <summary>
		/// Gets a value indicating the running status of the sync engine.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Gets the options for the sync engine.
		/// </summary>
		public SyncOptions Options { get; }

		/// <summary>
		/// The server.
		/// </summary>
		public ISyncClient Server { get; }

		/// <summary>
		/// Gets the unique identifier for this sync session.
		/// </summary>
		public Guid SessionId { get; }

		/// <summary>
		/// Current state of the sync engine.
		/// </summary>
		public SyncEngineState State { get; }

		/// <summary>
		/// Gets the list of issues that happened during syncing.
		/// </summary>
		public IReadOnlyList<SyncIssue> SyncIssues => new ReadOnlyCollection<SyncIssue>(_syncIssues);

		#endregion

		#region Methods

		/// <summary>
		/// Cancels the sync process.
		/// </summary>
		public void Cancel()
		{
			_cancellationSource.Cancel(true);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public void Run()
		{
			try
			{
				IsRunning = true;
				_syncIssues.Clear();

				Server.Statistics.Reset();
				Client.Statistics.Reset();

				UpdateSyncState(status: SyncEngineStatus.Starting);

				var serverSession = Server.BeginSync(SessionId, Options);
				var clientSession = Client.BeginSync(SessionId, Options);
				var incoming = new Dictionary<Guid, DateTime>();

				if (!IsCancellationPending && Options.SyncDirection.HasFlag(SyncDirection.PullDown))
				{
					UpdateSyncState(status: SyncEngineStatus.Pulling);
					Process(Server, Client, Options.LastSyncedOnServer, serverSession.StartedOn, incoming);
				}

				if (!IsCancellationPending && Options.SyncDirection.HasFlag(SyncDirection.PushUp))
				{
					UpdateSyncState(status: SyncEngineStatus.Pushing);
					Process(Client, Server, Options.LastSyncedOnClient, clientSession.StartedOn, incoming);
				}

				Client.EndSync(SessionId);
				Server.EndSync(SessionId);

				SortLocalDatabases();

				Options.LastSyncedOnClient = clientSession.StartedOn;
				Options.LastSyncedOnServer = serverSession.StartedOn;

				UpdateSyncState(status: IsCancellationPending ? SyncEngineStatus.Cancelled : SyncEngineStatus.Completed);
			}
			catch (WebClientException ex)
			{
				_syncIssues.Add(new SyncIssue
				{
					IssueType = ex.Code == HttpStatusCode.Unauthorized ? SyncIssueType.Unauthorized : SyncIssueType.Unknown,
					Message = ex.Message
				});

				UpdateSyncState($"{TimeService.UtcNow:hh:mm:ss tt} {ex.Message}", SyncEngineStatus.Failed);
			}
			catch (Exception ex)
			{
				_syncIssues.Add(new SyncIssue { IssueType = SyncIssueType.Unknown, Message = ex.Message });

				UpdateSyncState($"{TimeService.UtcNow:hh:mm:ss tt} {ex.Message}", SyncEngineStatus.Failed);
			}
			finally
			{
				IsRunning = false;
			}
		}

		/// <summary>
		/// Instantiate and run an instance of the sync engine.
		/// </summary>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <returns> A list of issues that occurred during sync. </returns>
		public static SyncEngine Run(ISyncClient client, ISyncClient server, SyncOptions options)
		{
			return Run(Guid.NewGuid(), client, server, options);
		}

		/// <summary>
		/// Instantiate and run an instance of the sync engine.
		/// </summary>
		/// <param name="sessionId"> The ID of the session. </param>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <returns> A list of issues that occurred during sync. </returns>
		public static SyncEngine Run(Guid sessionId, ISyncClient client, ISyncClient server, SyncOptions options)
		{
			var engine = new SyncEngine(sessionId, client, server, options);
			engine.Run();
			return engine;
		}

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public async Task RunAsync()
		{
			await Task.Factory.StartNew(Run, _cancellationSource?.Token ?? CancellationToken.None);
		}

		/// <summary>
		/// Cancels the sync process and waits for it to stop.
		/// </summary>
		public void Stop(TimeSpan? timeout = null)
		{
			Cancel();
			WaitForRunToStop(timeout);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> True if disposing and false if otherwise. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			// bug: what happens if a sync engine is disposed while running?

			_cancellationSource?.Dispose();
		}

		/// <summary>
		/// Get changes from one client and apply them to another client.
		/// </summary>
		/// <param name="sourceClient"> The source to get changes from. </param>
		/// <param name="destinationClient"> The destination to apply changes to. </param>
		/// <param name="since"> The start date and time to get changes for. </param>
		/// <param name="until"> The end date and time to get changes for. </param>
		/// <param name="exclude"> The optional collection of items to exclude. </param>
		private void Process(ISyncClient sourceClient, ISyncClient destinationClient, DateTime since, DateTime until, IDictionary<Guid, DateTime> exclude)
		{
			var issues = new ServiceRequest<SyncIssue>();
			var request = new SyncRequest { Since = since, Until = until, Skip = 0 };
			var response = new Dictionary<Guid, DateTime>();
			bool hasMore;

			UpdateSyncState($"{sourceClient.Name} to {destinationClient.Name}.");

			do
			{
				// Get changes and move the request forward
				var changes = sourceClient.GetChanges(SessionId, request);
				request.Skip += changes.Collection.Count;
				hasMore = changes.HasMore;

				// Filter out any existing items that have been synced already (must have been excluded with the same modified on date/time)
				request.Collection = changes.Collection
					.Where(x => !exclude.ContainsKey(x.SyncId) || (exclude[x.SyncId] != x.ModifiedOn))
					.ToList();

				if (!request.Collection.Any())
				{
					continue;
				}

				// Apply changes and track any sync issues returned
				issues.Collection.AddRange(destinationClient.ApplyChanges(SessionId, request).Collection);

				// Capture all items that were synced without issue
				foreach (var syncObject in request.Collection.Where(x => issues.Collection.All(i => i.Id != x.SyncId)))
				{
					response.AddOrUpdate(syncObject.SyncId, syncObject.ModifiedOn);
				}

				UpdateSyncState(count: request.Skip, total: changes.TotalCount);
			} while (!IsCancellationPending && hasMore);

			_syncIssues.AddRange(issues.Collection);

			if (!IsCancellationPending && issues.Collection.Any())
			{
				var issuesToProcess = new ServiceRequest<SyncIssue>
				{
					Collection = issues.Collection.Take(Options.ItemsPerSyncRequest).ToList()
				};

				UpdateSyncState($"Processing {issuesToProcess.Collection.Count} sync issues.");

				try
				{
					var results = sourceClient.GetCorrections(SessionId, issuesToProcess);

					if ((results != null) && results.Collection.Any())
					{
						RemoveIssues(_syncIssues, results.Collection);
						request.Collection = results.Collection;
						_syncIssues.AddRange(destinationClient.ApplyCorrections(SessionId, request).Collection);
					}

					results = destinationClient.GetCorrections(SessionId, issuesToProcess);

					if ((results != null) && results.Collection.Any())
					{
						RemoveIssues(_syncIssues, results.Collection);
						request.Collection = results.Collection;
						_syncIssues.AddRange(sourceClient.ApplyCorrections(SessionId, request).Collection);
					}
				}
				catch (Exception ex)
				{
					UpdateSyncState($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
					throw;
				}
			}
		}

		private void RemoveIssues(ICollection<SyncIssue> syncIssues, IList<SyncObject> collection)
		{
			// Remove any issue that will be processed because we'll read add any issues during processing
			syncIssues.Where(x => collection.Any(y => y.SyncId == x.Id)).ToList()
				.ForEach(x => syncIssues.Remove(syncIssues.FirstOrDefault(y => y.Id == x.Id)));
		}

		/// <summary>
		/// Will attempt to sort local repositories after sync to order primary key.
		/// </summary>
		private void SortLocalDatabases()
		{
			foreach (var database in new[] { Server.GetDatabase(), Client.GetDatabase() })
			{
				using (database)
				{
					switch (database)
					{
						case Database d:
						{
							d.Repositories.ForEach(x => x.Value.Sort());
							break;
						}
					}
				}
			}
		}

		private void UpdateSyncState(string message = null, SyncEngineStatus? status = null, int? count = null, int? total = null)
		{
			if (message != null)
			{
				Logger.Instance.Write(SessionId, message, EventLevel.Verbose);
				State.Message = message;
			}

			if (status != null)
			{
				Logger.Instance.Write(SessionId, $"Changing status to {status.Value}.", EventLevel.Verbose);
				State.Status = status.Value;
			}

			if (count != null)
			{
				State.Count = count.Value;
			}

			if (total != null)
			{
				State.Total = total.Value;
			}

			SyncStateChanged?.Invoke(this, (SyncEngineState) State.DeepClone());
		}

		private void WaitForRunToStop(TimeSpan? timeout)
		{
			var watch = Stopwatch.StartNew();
			timeout ??= TimeSpan.FromSeconds(1);

			while ((watch.Elapsed < timeout) && IsRunning)
			{
				Thread.Sleep(10);
			}
		}

		#endregion

		#region Events

		/// <summary>
		/// Notifies when the sync status changes.
		/// </summary>
		public event EventHandler<SyncEngineState> SyncStateChanged;

		#endregion
	}
}