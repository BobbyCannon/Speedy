#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Represents an event.
	/// </summary>
	public class Event : IDisposable
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		public Event()
		{
			var currentTime = TimeService.UtcNow;
			Children = new List<Event>();
			CompletedOn = currentTime;
			Id = Guid.NewGuid();
			Name = string.Empty;
			StartedOn = currentTime;
			Values = new List<EventValue>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or set the child events.
		/// </summary>
		public ICollection<Event> Children { get; set; }

		/// <summary>
		/// Gets or set the date and time the event was completed.
		/// </summary>
		public DateTime CompletedOn { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// Gets or sets the elapsed time between the started on and completed on.
		/// </summary>
		public TimeSpan ElapsedTime => CompletedOn - StartedOn;

		/// <summary>
		/// Gets or sets the unique ID.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// Returns true if the event has been completed.
		/// </summary>
		public bool IsCompleted { get; set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the parent ID.
		/// </summary>
		public Guid ParentId { get; set; }

		/// <summary>
		/// Gets or set the date and time the event was started.
		/// </summary>
		public DateTime StartedOn { get; set; }

		/// <summary>
		/// Gets or sets the event type.
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// Gets or sets the values.
		/// </summary>
		public ICollection<EventValue> Values { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Adds a child event to this event.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		public void AddEvent(string name, params EventValue[] values)
		{
			Children.Add(new Event { ParentId = Id, Name = name, Values = values.ToList() });
		}

		/// <summary>
		/// Adds an exception to this event.
		/// </summary>
		/// <param name="exception"> The exception to be added to the event. </param>
		/// <param name="values"> Optional values for this exception. </param>
		public void AddException(Exception exception, params EventValue[] values)
		{
			Children.Add(FromException(Id, exception, values));
		}

		/// <summary>
		/// Adds a value to this event.
		/// </summary>
		/// <param name="name"> The name of this value. </param>
		/// <param name="value"> The value of this value. </param>
		public void AddValue(string name, string value)
		{
			Values.Add(new EventValue { Name = name, Value = value });
		}

		/// <summary>
		/// Completes the event and adds it to the event or tracker.
		/// </summary>
		public Event Complete()
		{
			IsCompleted = true;
			CompletedOn = TimeService.UtcNow;
			Completed?.Invoke(this);
			Completed = null;
			return this;
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
		/// Starts a new event. The event will need to be completed or disposed before it will be added to the tracker.
		/// </summary>
		/// <param name="parentId"> The ID of the parent for this event. </param>
		/// <param name="ex"> The exception to be turned into an event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public static Event FromException(Guid parentId, Exception ex, params EventValue[] values)
		{
			if (ex == null)
			{
				throw new ArgumentNullException(nameof(ex), "The exception cannot be null.");
			}

			var eventValues = new List<EventValue>(values);
			eventValues.AddOrUpdate(new EventValue("Message", ex.Message), new EventValue("Stack Trace", ex.StackTrace ?? string.Empty));

			var response = new Event
			{
				ParentId = parentId,
				Name = ex.GetType().Name,
				Values = eventValues.ToList(),
				Type = "Exception"
			};

			if (ex.InnerException == null)
			{
				return response;
			}

			var childException = FromException(parentId, ex.InnerException);
			response.Children.Add(childException);
			return response;
		}

		/// <summary>
		/// Process an action and then add the event.
		/// </summary>
		/// <param name="key"> The key for the event. </param>
		/// <param name="action"> The action to process. </param>
		public void Process(Func<string> key, Action<Event> action)
		{
			if (IsCompleted)
			{
				action(this);
				return;
			}

			using var result = new Event { Name = key(), StartedOn = TimeService.UtcNow };
			action(result);
			Children.Add(result);
		}

		/// <summary>
		/// Process an action and then add the event.
		/// </summary>
		/// <typeparam name="T"> The type of the response for the action. </typeparam>
		/// <param name="key"> The key for the event. </param>
		/// <param name="action"> The action to process. </param>
		/// <returns> The result of the action. </returns>
		public T Process<T>(Func<string> key, Func<Event, T> action)
		{
			if (IsCompleted)
			{
				return action(this);
			}

			using var result = new Event { Name = key(), StartedOn = TimeService.UtcNow };
			var response = action(result);
			Children.Add(result);
			return response;
		}

		/// <summary>
		/// Starts a new event. Once the event is done be sure to call <seealso cref="Complete" />.
		/// </summary>
		/// <param name="name"> The name of the event. </param>
		/// <param name="values"> Optional values for this event. </param>
		/// <returns> The event for tracking an event. </returns>
		public Event StartEvent(Func<string> name, params EventValue[] values)
		{
			var response = new Event { ParentId = Id, Name = name(), Values = values.ToList() };
			response.Completed += x => { Children.Add(x); };
			return response;
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

			Complete();
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the event is completed.
		/// </summary>
		internal event Action<Event> Completed;

		#endregion
	}
}