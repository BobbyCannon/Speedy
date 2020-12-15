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

		private TimeSpan _elapsed;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates an instance of the timer.
		/// </summary>
		public Timer() : this(new DefaultDispatcher())
		{
		}

		/// <summary>
		/// Instantiates an instance of the timer.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher for handling property changes. </param>
		public Timer(IDispatcher dispatcher) : base(dispatcher)
		{
			// Assign the current incident that is active.
			_elapsed = TimeSpan.Zero;

			// Reset the started on
			StartedOn = DateTime.MinValue;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The time elapsed for the timer.
		/// </summary>
		public TimeSpan Elapsed => IsRunning ? _elapsed + RunningElapsed() : _elapsed;

		/// <summary>
		/// Indicates the timer is running or not.
		/// </summary>
		public bool IsRunning => StartedOn > DateTime.MinValue;

		/// <summary>
		/// The last time the timer was started on.
		/// </summary>
		public DateTime StartedOn { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Reset the timer.
		/// </summary>
		public virtual void Reset()
		{
			Reset(TimeSpan.Zero);
		}

		/// <summary>
		/// Reset the time while provided an elapsed timer.
		/// </summary>
		/// <param name="elapsed"> The value to set elapsed to. </param>
		public virtual void Reset(TimeSpan elapsed)
		{
			_elapsed = elapsed;

			StartedOn = DateTime.MinValue;
		}

		/// <summary>
		/// Restarts the timer.
		/// </summary>
		public virtual void Restart()
		{
			Restart(GetCurrentTime());
		}

		/// <summary>
		/// Restarts the timer with a specific time. The elapsed time will be reset.
		/// </summary>
		/// <param name="dateTime"> The time the timer was started. </param>
		public virtual void Restart(DateTime dateTime)
		{
			_elapsed = TimeSpan.Zero;

			StartedOn = dateTime;
		}

		/// <summary>
		/// Start the timer.
		/// </summary>
		public virtual void Start()
		{
			Start(GetCurrentTime());
		}

		/// <summary>
		/// Starts the timer with a specific time.
		/// </summary>
		/// <param name="dateTime"> The time the timer was started. </param>
		public virtual void Start(DateTime dateTime)
		{
			if (IsRunning)
			{
				return;
			}

			StartedOn = dateTime;
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public virtual void Stop()
		{
			Stop(GetCurrentTime());
		}

		/// <summary>
		/// Stops the timer at a specific time.
		/// </summary>
		/// <param name="dateTime"> The time the timer was stopped. </param>
		public virtual void Stop(DateTime dateTime)
		{
			if (!IsRunning)
			{
				return;
			}

			var elapsed = dateTime - StartedOn;
			if (elapsed.Ticks > 0)
			{
				_elapsed += elapsed;
			}

			StartedOn = DateTime.MinValue;
		}

		/// <summary>
		/// Gets the current time for the timer.
		/// </summary>
		/// <returns> The current time. </returns>
		protected virtual DateTime GetCurrentTime()
		{
			return TimeService.UtcNow;
		}

		/// <summary>
		/// The current running elapsed time.
		/// </summary>
		/// <returns> The running elapsed time. </returns>
		private TimeSpan RunningElapsed()
		{
			return GetCurrentTime() - StartedOn;
		}

		#endregion
	}
}