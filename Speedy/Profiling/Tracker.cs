#region References

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Threading;
using Speedy.Extensions;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.Profiling
{
	/// <summary>
	/// A tracker to track paths and exceptions. Each tracker instance represents a new sessions.
	/// </summary>
	public class Tracker : Bindable, IDisposable
	{
		#region Fields

		private static AssemblyName _assemblyName;
		private static Version _assemblyVersion;
		private readonly IKeyValueRepositoryProvider<TrackerPath> _cacheProvider;
		private IKeyValueRepository<TrackerPath> _currentCacheRepository;
		private BackgroundWorker _pathProcessor;
		private TrackerPath _session;

		#endregion

		#region Constructors

		/// <summary>
		/// A tracker to capture, store, and transmit paths to a path repository.
		/// </summary>
		/// <param name="pathRepository"> The final repository used to store the data. </param>
		/// <param name="cacheProvider"> The repository used to cache data until it can be stored. </param>
		public Tracker(ITrackerPathRepository pathRepository, IKeyValueRepositoryProvider<TrackerPath> cacheProvider) : this(pathRepository, cacheProvider, new DefaultDispatcher())
		{
		}

		/// <summary>
		/// A tracker to capture, store, and transmit paths to a path repository.
		/// </summary>
		/// <param name="pathRepository"> The final repository used to store the data. </param>
		/// <param name="cacheProvider"> The repository used to cache data until it can be stored. </param>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		public Tracker(ITrackerPathRepository pathRepository, IKeyValueRepositoryProvider<TrackerPath> cacheProvider, IDispatcher dispatcher) : base(dispatcher)
		{
			_cacheProvider = cacheProvider;
			_pathProcessor = new BackgroundWorker { WorkerSupportsCancellation = true };
			_pathProcessor.DoWork += PathProcessorOnDoWork;

			PathProcessingDelay = 250;
			PathProcessorRunning = false;
			PathRepository = pathRepository;
			ProcessRepositoryChunk = 300;
		}

		static Tracker()
		{
			Assembly = Assembly.GetExecutingAssembly();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the assembly for this item.
		/// </summary>
		public static Assembly Assembly { get; }

		/// <summary>
		/// Gets the assembly name. Gets cached on call.
		/// </summary>
		public static AssemblyName AssemblyName => _assemblyName ??= Assembly.GetName();

		/// <summary>
		/// Gets the version of the assembly.
		/// </summary>
		public static Version AssemblyVersion => _assemblyVersion ??= Assembly.GetName().Version;

		/// <summary>
		/// Gets or sets the delay in milliseconds between processing paths. The path processor will
		/// delay this time between processing of paths. There will be a delay 4x this amount when an
		/// error occurs during processing.
		/// </summary>
		public int PathProcessingDelay { get; set; }

		/// <summary>
		/// Gets the running status of the path processor.
		/// </summary>
		public bool PathProcessorRunning { get; private set; }

		/// <summary>
		/// Gets the repository for paths that are tracked.
		/// </summary>
		public ITrackerPathRepository PathRepository { get; }

		/// <summary>
		/// Gets the chunk size for saving data to the final storage location.
		/// </summary>
		public int ProcessRepositoryChunk { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds an exception to the tracking session.
		/// </summary>
		/// <param name="exception"> The exception to be added. </param>
		/// <param name="values"> Optional values for this path. </param>
		public void AddException(Exception exception, params TrackerPathValue[] values)
		{
			ValidateTrackerState();

			_currentCacheRepository.WriteAndSave(TrackerPath.CreatePath(_session.Id, exception, values));

			if (exception.InnerException != null)
			{
				AddException(exception.InnerException);
			}
		}

		/// <summary>
		/// Adds an path to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the path. </param>
		/// <param name="values"> Optional values for this path. </param>
		public void AddPath(string name, params TrackerPathValue[] values)
		{
			ValidateTrackerState();

			_currentCacheRepository.WriteAndSave(new TrackerPath { ParentId = _session.Id, Name = name, Values = values.ToList() });
		}

		/// <summary>
		/// Adds an path with an existing timespan to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the path. </param>
		/// <param name="elapsedTime"> The elapsed time of the path. </param>
		/// <param name="values"> Optional values for this path. </param>
		public void AddPath(string name, TimeSpan elapsedTime, params TrackerPathValue[] values)
		{
			ValidateTrackerState();

			var currentTime = TimeService.UtcNow;
			_currentCacheRepository.WriteAndSave(new TrackerPath
			{
				ParentId = _session.Id,
				Name = name,
				CompletedOn = currentTime,
				StartedOn = currentTime.Subtract(elapsedTime),
				Values = values.ToList()
			});
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
		/// Initialize the tracker before using it.
		/// </summary>
		/// <param name="values"> The values to associate with this session. </param>
		public void Initialize(params TrackerPathValue[] values)
		{
			if (_session != null)
			{
				_session.Values.AddRange(values);
				return;
			}

			_session = NewSession(AssemblyName, TimeSpan.Zero, values);
			_currentCacheRepository = _cacheProvider.OpenRepository(_session);
		}

		/// <summary>
		/// A tracker to capture, store, and transmit paths to a data channel.
		/// </summary>
		/// <param name="repository"> The channel used to store the data remotely. </param>
		/// <param name="provider"> The repository used to store the data locally. </param>
		/// <param name="values"> The values to associate with this session. </param>
		public static Tracker Start(ITrackerPathRepository repository, IKeyValueRepositoryProvider<TrackerPath> provider, params TrackerPathValue[] values)
		{
			var tracker = new Tracker(repository, provider);
			tracker.Start(values);
			return tracker;
		}

		/// <summary>
		/// Start the tracker for the provided application.
		/// </summary>
		/// <param name="values"> The initial path values. </param>
		public void Start(params TrackerPathValue[] values)
		{
			Initialize(values);

			_pathProcessor.RunWorkerAsync(this);

			UtilityExtensions.Wait(() => PathProcessorRunning, 5000, 10);
		}

		/// <summary>
		/// Starts a new path. Once the path is done be sure to call <seealso cref="TrackerPath.Complete" />.
		/// </summary>
		/// <param name="name"> The name of the path. </param>
		/// <param name="values"> Optional values for this path. </param>
		/// <returns> The path for tracking an path. </returns>
		public TrackerPath StartNewPath(string name, params TrackerPathValue[] values)
		{
			ValidateTrackerState();
			var response = new TrackerPath { ParentId = _session.Id, Name = name, Values = values.ToList(), Type = name };
			response.Completed += x => _currentCacheRepository.WriteAndSave(x);
			return response;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> A flag determining if we are currently disposing. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
			{
				return;
			}

			try
			{
				if (PathProcessorRunning && !_pathProcessor.CancellationPending)
				{
					_pathProcessor.CancelAsync();
					UtilityExtensions.Wait(() => !PathProcessorRunning, 5000, 10);
				}

				ProcessSession();

				if (_currentCacheRepository != null)
				{
					_currentCacheRepository.Flush();
					_currentCacheRepository.Delete();
					_currentCacheRepository.Dispose();
				}
			}
			catch
			{
				// Ignore any issues
			}
			finally
			{
				_session = null;
				_currentCacheRepository = null;
				_pathProcessor = null;
			}
		}

		/// <summary>
		/// Log a message.
		/// </summary>
		/// <param name="message"> The message to log. </param>
		/// <param name="level"> The level of the message. </param>
		protected virtual void OnLog(string message, EventLevel level = EventLevel.Informational)
		{
			Log?.Invoke(message, level);
		}

		private static TrackerPath NewSession(AssemblyName application, TimeSpan elapsedTime, params TrackerPathValue[] values)
		{
			var response = new TrackerPath
			{
				CompletedOn = TimeService.UtcNow,
				Name = "Session",
				Id = Guid.NewGuid(),
				Type = "Session",
				Values = values.ToList()
			};

			response.StartedOn = response.CompletedOn.Subtract(elapsedTime);
			response.Values.AddOrUpdate(new TrackerPathValue(".NET Version", Environment.Version),
				new TrackerPathValue("Application Bitness", Environment.Is64BitProcess ? "64" : "32"),
				new TrackerPathValue("Application Name", application.Name),
				new TrackerPathValue("Application Version", application.Version?.ToString() ?? "0.0.0.0")
			);

			return response;
		}

		private static void PathProcessorOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var tracker = (Tracker) args.Argument;
			var delayWatch = new Stopwatch();
			var shutdownWatch = new Stopwatch();
			var shutdownDelay = tracker.PathProcessingDelay * 4;
			var pathProcessingDelay = tracker.PathProcessingDelay;

			tracker.PathProcessorRunning = true;
			tracker.OnLog("Path processor started.", EventLevel.Verbose);

			while (shutdownWatch.Elapsed.TotalMilliseconds < shutdownDelay)
			{
				try
				{
					if (!shutdownWatch.IsRunning && worker.CancellationPending)
					{
						tracker.OnLog("Path processor shutting down...", EventLevel.Verbose);
						shutdownWatch.Start();
					}

					if (tracker._currentCacheRepository?.Name == null)
					{
						continue;
					}

					tracker.OnLog("Path processor loop...", EventLevel.Verbose);

					if (tracker.ProcessSession() > 0)
					{
						continue;
					}

					// No data to process for our current session so try and send old session data 
					// that may not been able to transmit earlier.
					if (tracker.ProcessOldSessions() > 0)
					{
						continue;
					}

					if (shutdownWatch.IsRunning)
					{
						break;
					}

					delayWatch.Restart();

					while (delayWatch.Elapsed.TotalMilliseconds < pathProcessingDelay && !worker.CancellationPending)
					{
						Thread.Sleep(50);
					}
				}
				catch (Exception ex)
				{
					// Do not log connection issues.
					var message = ex.ToDetailedString();
					tracker.OnLog(message, EventLevel.Critical);

					// An issue occurred and we need to log it and delay.
					Thread.Sleep(shutdownDelay / 2);
				}
			}

			tracker.OnLog("Path processor stopped.", EventLevel.Verbose);
			tracker.PathProcessorRunning = false;
		}

		private int ProcessOldSessions()
		{
			OnLog("Processing old sessions...", EventLevel.Verbose);

			using var repository = _cacheProvider.OpenAvailableRepository(_currentCacheRepository?.Name);

			if (repository == null)
			{
				return 0;
			}

			OnLog($"Processing old session: {repository.Name}.", EventLevel.Verbose);

			try
			{
				var result = ProcessRepository(this, repository, PathRepository);
				if (result > 0)
				{
					return result;
				}

				if (repository.Count <= 0)
				{
					repository.Delete();
				}

				return 0;
			}
			catch (Exception ex)
			{
				// Get the detailed issue with the processing.
				var message = ex.ToDetailedString();

				// Determine if the issue was a connection issue
				if (!message.Contains("Unable to connect to the remote server"))
				{
					// Delete the repository on any issue other than connection issue.
					OnLog($"Error processing repository {repository.Name}: {message}.", EventLevel.Critical);
					AddException(ex, new TrackerPathValue("Repository Name", repository.Name));
					repository.Archive();
					return 0;
				}

				OnLog($"Error processing repository {repository.Name}: {message}.", EventLevel.Verbose);
				return 0;
			}
		}

		private static int ProcessRepository(Tracker tracker, IKeyValueRepository<TrackerPath> repository, ITrackerPathRepository client)
		{
			if (string.IsNullOrWhiteSpace(repository?.Name))
			{
				return 0;
			}

			tracker.OnLog($"Processing repository: {repository.Name}.", EventLevel.Verbose);

			var count = 0;
			var chunk = tracker.ProcessRepositoryChunk;
			var data = repository.Read().Take(chunk).ToList();

			while (data.Any())
			{
				client.Write(data.Select(x => x.Value).ToArray());
				var keys = new HashSet<string>(data.Select(x => x.Key));
				repository.Remove(keys);
				repository.Save();
				count += data.Count;
				tracker.OnLog($"Wrote {data.Count} from {repository.Name}...", EventLevel.LogAlways);
				data = repository.Read().Take(chunk).ToList();
			}

			tracker.OnLog($"Processed repository with total of {count} from {repository.Name}.", EventLevel.Verbose);
			return count;
		}

		/// <summary>
		/// Give access to process the session for the path processor worker.
		/// </summary>
		/// <returns> The number of paths processed. </returns>
		private int ProcessSession()
		{
			return ProcessRepository(this, _currentCacheRepository, PathRepository);
		}

		/// <summary>
		/// Check to see if the tracker is in a good working state.
		/// </summary>
		private void ValidateTrackerState()
		{
			if (_session != null)
			{
				return;
			}

			const string message = "You must first start the tracker before using it.";
			OnLog(message, EventLevel.Warning);
			throw new InvalidOperationException(message);
		}

		#endregion

		#region Events

		/// <summary>
		/// Event for when the tracker needs to write information.
		/// </summary>
		public event Action<string, EventLevel> Log;

		#endregion
	}
}