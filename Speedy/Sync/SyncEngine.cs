#region References

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Extensions;
using Speedy.Logging;
using Speedy.Net;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync engine.
	/// </summary>
	public class SyncEngine
	{
		#region Fields

		private readonly List<SyncIssue> _syncIssues;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate an instance of the sync engine.
		/// </summary>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <param name="source"> An optional cancellation token source. </param>
		public SyncEngine(ISyncClient client, ISyncClient server, SyncOptions options, CancellationTokenSource source = null)
			: this(Guid.NewGuid(), client, server, options, source)
		{
		}

		/// <summary>
		/// Instantiate an instance of the sync engine.
		/// </summary>
		/// <param name="sessionId"> The ID of the session. </param>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <param name="source"> An optional cancellation token source. </param>
		public SyncEngine(Guid sessionId, ISyncClient client, ISyncClient server, SyncOptions options, CancellationTokenSource source = null)
		{
			_syncIssues = new List<SyncIssue>();

			SessionId = sessionId;
			Client = client;
			Server = server;
			State = new SyncEngineState();
			Options = options;
			CancellationSource = source ?? new CancellationTokenSource();
		}

		#endregion

		#region Properties

		/// <summary>
		/// An optional cancellation token.
		/// </summary>
		public CancellationTokenSource CancellationSource { get; }

		/// <summary>
		/// The client.
		/// </summary>
		public ISyncClient Client { get; }

		/// <summary>
		/// Gets the options for the sync engine.
		/// </summary>
		public SyncOptions Options { get; set; }

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
		public SyncEngineState State { get; set; }

		/// <summary>
		/// Gets the list of issues that happened during syncing.
		/// </summary>
		public IReadOnlyList<SyncIssue> SyncIssues => new ReadOnlyCollection<SyncIssue>(_syncIssues);

		#endregion

		#region Methods

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public void Run()
		{
			_syncIssues.Clear();

			Server.Statistics.Reset();
			Client.Statistics.Reset();

			var serverSession = Server.BeginSync(SessionId, Options);
			var clientSession = Client.BeginSync(SessionId, Options);

			OnSyncStateChanged($"{clientSession.StartedOn:hh:mm:ss tt}.", SyncEngineStatus.Starting);

			var incoming = new Dictionary<Guid, DateTime>();

			if (!CancellationSource.IsCancellationRequested)
			{
				OnSyncStateChanged(status: SyncEngineStatus.Pulling);
				incoming = Process(Server, Client, Options.LastSyncedOnServer, serverSession.StartedOn, incoming);
			}

			if (!CancellationSource.IsCancellationRequested)
			{
				OnSyncStateChanged(status: SyncEngineStatus.Pushing);
				Process(Client, Server, Options.LastSyncedOnClient, clientSession.StartedOn, incoming);
			}

			Client.EndSync(SessionId);
			Server.EndSync(SessionId);

			SortLocalDatabases();

			Options.LastSyncedOnClient = clientSession.StartedOn;
			Options.LastSyncedOnServer = serverSession.StartedOn;

			OnSyncStateChanged($"{TimeService.UtcNow:hh:mm:ss tt}", CancellationSource.IsCancellationRequested ? SyncEngineStatus.Cancelled : SyncEngineStatus.Completed);
		}

		/// <summary>
		/// Instantiate and run an instance of the sync engine.
		/// </summary>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <param name="source"> An optional cancellation token source. </param>
		/// <returns> A list of issues that occurred during sync. </returns>
		public static SyncEngine Run(ISyncClient client, ISyncClient server, SyncOptions options, CancellationTokenSource source = null)
		{
			return Run(Guid.NewGuid(), client, server, options, source);
		}

		/// <summary>
		/// Instantiate and run an instance of the sync engine.
		/// </summary>
		/// <param name="sessionId"> The ID of the session. </param>
		/// <param name="client"> The client to sync from. </param>
		/// <param name="server"> The server to sync to. </param>
		/// <param name="options"> The options for the sync engine. </param>
		/// <param name="source"> An optional cancellation token source. </param>
		/// <returns> A list of issues that occurred during sync. </returns>
		public static SyncEngine Run(Guid sessionId, ISyncClient client, ISyncClient server, SyncOptions options, CancellationTokenSource source = null)
		{
			var engine = new SyncEngine(sessionId, client, server, options, source);
			engine.Run();
			return engine;
		}

		/// <summary>
		/// Start to sync process.
		/// </summary>
		public async Task RunAsync()
		{
			await Task.Factory.StartNew(Run, CancellationSource.Token);
		}
		
		/// <summary>
		/// Stops the sync process.
		/// </summary>
		public void Stop(TimeSpan? timeout = null)
		{
			CancellationSource.Cancel(true);

			var timeOut = timeout != null ? TimeService.UtcNow.Add(timeout.Value) : TimeService.UtcNow.AddSeconds(30);

			while (TimeService.UtcNow <= timeOut
				&& State.Status != SyncEngineStatus.Stopped
				&& State.Status != SyncEngineStatus.Cancelled)
			{
				Thread.Sleep(10);
			}
		}

		private void OnSyncStateChanged(string message = null, SyncEngineStatus? status = null, int? count = null, int? total = null)
		{
			if (message != null)
			{
				Logger.Instance.Write(SessionId, message, EventLevel.Verbose);
			}

			if (status.HasValue)
			{
				Logger.Instance.Write(SessionId, $"Changing status to {status.Value}.", EventLevel.Verbose);
				State.Status = status.Value;
			}

			State.Message = message;
			State.Count = count ?? 0;
			State.Total = total ?? 0;

			SyncStateChanged?.Invoke(this, State.DeepClone());
		}

		/// <summary>
		/// Get changes from one client and apply them to another client.
		/// </summary>
		/// <param name="sourceClient"> The source to get changes from. </param>
		/// <param name="destinationClient"> The destination to apply changes to. </param>
		/// <param name="since"> The start date and time to get changes for. </param>
		/// <param name="until"> The end date and time to get changes for. </param>
		/// <param name="exclude"> The optional collection of items to exclude. </param>
		private Dictionary<Guid, DateTime> Process(ISyncClient sourceClient, ISyncClient destinationClient, DateTime since, DateTime until, IDictionary<Guid, DateTime> exclude)
		{
			var issues = new ServiceRequest<SyncIssue>();
			var request = new SyncRequest { Since = since, Until = until, Skip = 0 };
			var response = new Dictionary<Guid, DateTime>();
			bool hasMore;

			OnSyncStateChanged($"{sourceClient.Name} to {destinationClient.Name}.");

			do
			{
				// Get changes and move the request forward
				var changes = sourceClient.GetChanges(SessionId, request);
				request.Skip += changes.Collection.Count;
				hasMore = changes.HasMore;

				// Filter out any existing items that have been synced already (must have been excluded with the same modified on date/time)
				request.Collection = changes.Collection.Where(x => !exclude.ContainsKey(x.SyncId) || exclude[x.SyncId] != x.ModifiedOn).ToList();

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

				OnSyncStateChanged(count: request.Skip, total: changes.TotalCount);
			} while (!CancellationSource.IsCancellationRequested && hasMore);

			_syncIssues.AddRange(issues.Collection);

			if (!CancellationSource.IsCancellationRequested && issues.Collection.Any())
			{
				var issuesToProcess = new ServiceRequest<SyncIssue>
				{
					Collection = issues.Collection.Take(Options.ItemsPerSyncRequest).ToList()
				};

				OnSyncStateChanged($"Processing {issuesToProcess.Collection.Count} sync issues.");

				try
				{
					var results = sourceClient.GetCorrections(SessionId, issuesToProcess);

					if (results != null && results.Collection.Any())
					{
						RemoveIssues(_syncIssues, results.Collection);
						request.Collection = results.Collection;
						_syncIssues.AddRange(destinationClient.ApplyCorrections(SessionId, request).Collection);
					}

					results = destinationClient.GetCorrections(SessionId, issuesToProcess);

					if (results != null && results.Collection.Any())
					{
						RemoveIssues(_syncIssues, results.Collection);
						request.Collection = results.Collection;
						_syncIssues.AddRange(sourceClient.ApplyCorrections(SessionId, request).Collection);
					}
				}
				catch (Exception ex)
				{
					OnSyncStateChanged($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
					throw;
				}
			}

			return response;
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

		#endregion

		#region Events

		/// <summary>
		/// Notifies when the sync status changes.
		/// </summary>
		public event EventHandler<SyncEngineState> SyncStateChanged;

		#endregion
	}
}