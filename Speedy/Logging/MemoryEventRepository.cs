#region References

using System.Collections.Generic;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// In memory event repository.
	/// </summary>
	public class MemoryEventRepository : IEventRepository
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the event repository.
		/// </summary>
		public MemoryEventRepository()
		{
			Events = new List<Event>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The events for this repository.
		/// </summary>
		public List<Event> Events { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Clears the events from the repository.
		/// </summary>
		public void Clear()
		{
			Events.Clear();
		}

		/// <summary>
		/// Writes events to this repository.
		/// </summary>
		/// <param name="events"> The events to be added. </param>
		public void WriteEvents(params Event[] events)
		{
			Events.AddRange(events);
		}

		#endregion
	}
}