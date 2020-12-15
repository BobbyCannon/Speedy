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

namespace Speedy.Logging
{
	/// <summary>
	/// A tracker to track events and exceptions. Each tracker instance represents a new sessions.
	/// </summary>
	public class EventTracker : IDisposable
	{
		#region Fields

		private static AssemblyName _assemblyName;
		private readonly IEventRepository _eventRepository;
		private BackgroundWorker _eventProcessor;
		private readonly IKeyValueRepositoryProvider<Event> _cacheProvider;
		private IKeyValueRepository<Event> _currentCacheRepository;
		private Event _session;

		#endregion

		#region Constructors

		/// <summary>
		/// A tracker to capture, store, and transmit events to a data channel.
		/// </summary>
		/// <param name="eventRepository"> The location used to store the data. </param>
		/// <param name="cacheProvider"> The repository used to store the data locally. </param>
		protected EventTracker(IEventRepository eventRepository, IKeyValueRepositoryProvider<Event> cacheProvider)
		{
			_eventRepository = eventRepository;
			_cacheProvider = cacheProvider;
			_eventProcessor = new BackgroundWorker { WorkerSupportsCancellation = true };
			_eventProcessor.DoWork += EventProcessorOnDoWork;

			EventProcessingDelay = 250;
			EventProcessorRunning = false;
		}

		static EventTracker()
		{
			Assembly = Assembly.GetExecutingAssembly();
			Version = Assembly.GetName().Version;
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
		/// Gets or sets the delay in milliseconds between processing events. The event processor will
		/// delay this time between processing of events. There will be a delay 4x this amount when an
		/// error occurs during processing.
		/// </summary>
		public int EventProcessingDelay { get; }

		/// <summary>
		/// Gets the running status of the event processor.
		/// </summary>
		public bool EventProcessorRunning { get; private set; }

		/// <summary>
		/// Gets the version of the assembly.
		/// </summary>
		public static Version Version { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds an event to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, params EventValue[] values)
		{
			ValidateTrackerState();
			_currentCacheRepository.WriteAndSave(new Event { ParentId = _session.Id, Name = name, Values = values.ToList() });
		}

		/// <summary>
		/// Adds an event with an existing timespan to the tracking session.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="elapsedTime"> The elapsed time of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, TimeSpan elapsedTime, params EventValue[] values)
		{
			ValidateTrackerState();

			var currentTime = TimeService.UtcNow;
			_currentCacheRepository.WriteAndSave(new Event
			{
				ParentId = _session.Id,
				Name = name,
				CompletedOn = currentTime,
				StartedOn = currentTime.Subtract(elapsedTime),
				Values = values.ToList()
			});
		}

		/// <summary>
		/// Adds an exception to the tracking session.
		/// </summary>
		/// <param name="exception"> The exception to be added. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddException(Exception exception, params EventValue[] values)
		{
			ValidateTrackerState();

			_currentCacheRepository.WriteAndSave(Event.FromException(_session.Id, exception, values));

			if (exception.InnerException != null)
			{
				AddException(exception.InnerException);
			}
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
		/// A tracker to capture, store, and transmit events to a data channel.
		/// </summary>
		/// <param name="channel"> The channel used to store the data remotely. </param>
		/// <param name="provider"> The repository used to store the data locally. </param>
		/// <param name="values"> The values to associate with this session. </param>
		public static EventTracker Start(IEventRepository channel, IKeyValueRepositoryProvider<Event> provider, params EventValue[] values)
		{
			var tracker = new EventTracker(channel, provider);
			tracker.Start(AssemblyName, TimeSpan.Zero, values);
			return tracker;
		}

		/// <summary>
		/// Starts a new event. Once the event is done be sure to call <seealso cref="Event.Complete" />.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public Event StartEvent(string name, params EventValue[] values)
		{
			ValidateTrackerState();
			var response = new Event { ParentId = _session.Id, Name = name, Values = values.ToList(), Type = name };
			response.Completed += x => _currentCacheRepository.WriteAndSave(x);
			return response;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> A flag determining if we are currently disposing. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || _currentCacheRepository == null)
			{
				return;
			}

			try
			{
				if (EventProcessorRunning && !_eventProcessor.CancellationPending)
				{
					_eventProcessor.CancelAsync();
					UtilityExtensions.Wait(() => !EventProcessorRunning, 5000, 10);
				}

				_currentCacheRepository.Flush();

				if (_currentCacheRepository.Count <= 0)
				{
					_currentCacheRepository.Delete();
				}

				_currentCacheRepository.Dispose();
			}
			finally
			{
				_session = null;
				_currentCacheRepository = null;
				_eventProcessor = null;
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

		private static void EventProcessorOnDoWork(object sender, DoWorkEventArgs args)
		{
			var worker = (BackgroundWorker) sender;
			var tracker = (EventTracker) args.Argument;
			var delayWatch = new Stopwatch();
			var shutdownWatch = new Stopwatch();
			var shutdownDelay = tracker.EventProcessingDelay * 4;

			tracker.EventProcessorRunning = true;
			tracker.OnLog("Event processor started.", EventLevel.Verbose);

			while (shutdownWatch.Elapsed.TotalMilliseconds < shutdownDelay)
			{
				try
				{
					if (!shutdownWatch.IsRunning && worker.CancellationPending)
					{
						tracker.OnLog("Event processor shutting down...", EventLevel.Verbose);
						shutdownWatch.Start();
					}

					if (tracker._currentCacheRepository?.Name == null)
					{
						continue;
					}

					tracker.OnLog("Event processor loop...", EventLevel.LogAlways);

					if (tracker.ProcessSession() <= 0)
					{
						// No data to process for our current session so try and send old session data 
						// that may not been able to transmit earlier.
						if (tracker.ProcessOldSessions() <= 0)
						{
							if (shutdownWatch.IsRunning)
							{
								break;
							}

							delayWatch.Restart();
							while (delayWatch.Elapsed.TotalMilliseconds < tracker.EventProcessingDelay && !worker.CancellationPending)
							{
								Thread.Sleep(50);
							}
						}
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

			tracker.OnLog("Event processor stopped.", EventLevel.Verbose);
			tracker.EventProcessorRunning = false;
		}

		private static Event NewSession(AssemblyName application, TimeSpan elapsedTime, params EventValue[] values)
		{
			var response = new Event
			{
				CompletedOn = TimeService.UtcNow,
				Name = "Session",
				Id = Guid.NewGuid(),
				Type = "Session",
				Values = values.ToList()
			};

			response.StartedOn = response.CompletedOn.Subtract(elapsedTime);
			response.Values.AddOrUpdate(new EventValue(".NET Version", Environment.Version),
				new EventValue("Application Bitness", Environment.Is64BitProcess ? "64" : "32"),
				new EventValue("Application Name", application.Name),
				new EventValue("Application Version", application.Version.ToString())
			);

			return response;
		}

		private int ProcessOldSessions()
		{
			OnLog("Processing old sessions...", EventLevel.Verbose);

			using (var repository = _cacheProvider.OpenAvailableRepository(_currentCacheRepository?.Name))
			{
				if (repository == null)
				{
					return 0;
				}

				OnLog($"Processing old session: {repository.Name}.", EventLevel.Verbose);

				try
				{
					var result = ProcessRepository(this, repository, _eventRepository);
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
						AddException(ex, new EventValue("Repository Name", repository.Name));
						repository.Archive();
						return 0;
					}

					OnLog($"Error processing repository {repository.Name}: {message}.", EventLevel.Verbose);
					return 0;
				}
			}
		}

		private static int ProcessRepository(EventTracker eventTracker, IKeyValueRepository<Event> repository, IEventRepository client)
		{
			if (string.IsNullOrWhiteSpace(repository?.Name))
			{
				return 0;
			}

			eventTracker.OnLog($"Processing repository: {repository.Name}.", EventLevel.Verbose);

			var count = 0;
			var chunk = 300;
			var data = repository.Read().Take(chunk).ToList();

			while (data.Any())
			{
				client.WriteEvents(data.Select(x => x.Value).ToArray());
				var keys = new HashSet<string>(data.Select(x => x.Key));
				repository.Remove(keys);
				repository.Save();
				count += data.Count;
				eventTracker.OnLog($"Wrote {data.Count} from {repository.Name}...", EventLevel.LogAlways);
				data = repository.Read().Take(chunk).ToList();
			}

			eventTracker.OnLog($"Processed repository with total of {count} from {repository.Name}.", EventLevel.Verbose);
			return count;
		}

		/// <summary>
		/// Give access to process the session for the event processor worker.
		/// </summary>
		/// <returns> The number of events processed. </returns>
		private int ProcessSession()
		{
			return ProcessRepository(this, _currentCacheRepository, _eventRepository);
		}

		/// <summary>
		/// Start the tracker for the provided application.
		/// </summary>
		/// <param name="assembly"> The calling assembly. </param>
		/// <param name="elapsedTime"> The existing elapsed time. </param>
		/// <param name="values"> The initial event values. </param>
		private void Start(AssemblyName assembly, TimeSpan elapsedTime, params EventValue[] values)
		{
			_session = NewSession(assembly, elapsedTime, values);
			_currentCacheRepository = _cacheProvider.OpenRepository(_session);
			_eventProcessor.RunWorkerAsync(this);
			UtilityExtensions.Wait(() => EventProcessorRunning, 5000, 10);
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