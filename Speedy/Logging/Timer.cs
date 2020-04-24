#region References

using System;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Timer that uses the time service.
	/// </summary>
	public class Timer : Bindable
	{
		#region Fields

		private DateTime _start;
		private TimeSpan _elapsed;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the timer.
		/// </summary>
		public Timer() : base(new DefaultDispatcher())
		{
		}
		
		/// <summary>
		/// Instantiates an instance of the timer.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher for the timer. </param>
		public Timer(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Incidates if the timer is running.
		/// </summary>
		public bool IsRunning => _start != DateTime.MinValue;

		/// <summary>
		/// The elapsed time the timer has been running.
		/// </summary>
		public TimeSpan Elapsed => IsRunning ? TimeService.UtcNow - _start : _elapsed;

		#endregion

		#region Methods

		/// <summary>
		/// Starts the timer.
		/// </summary>
		public void Start()
		{
			_start = TimeService.UtcNow;
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public virtual void Stop()
		{
			_elapsed = TimeService.UtcNow - _start;
			_start = DateTime.MinValue;
		}

		#endregion
	}
}