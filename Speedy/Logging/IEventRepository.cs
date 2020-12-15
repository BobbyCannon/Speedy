namespace Speedy.Logging
{
	/// <summary>
	/// Interface for storing event data.
	/// </summary>
	public interface IEventRepository
	{
		#region Methods

		/// <summary>
		/// Writes a collection of events.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		void WriteEvents(params Event[] events);

		#endregion
	}
}