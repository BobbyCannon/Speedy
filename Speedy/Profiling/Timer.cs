#region References

using System;

#endregion

namespace Speedy.Profiling
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
		/// Adds the average timer elapsed value to this timer.
		/// </summary>
		/// <param name="timer"> The timer to be added. </param>
		public void Add(AverageTimer timer)
		{
			Add(timer.Elapsed);
		}

		/// <summary>
		/// Adds the time value to this timer.
		/// </summary>
		/// <param name="time"> The time to be added. </param>
		public void Add(TimeSpan time)
		{
			_elapsed = _elapsed.Add(time);

			OnPropertyChanged(nameof(Elapsed));
		}

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

			OnPropertyChanged(nameof(Elapsed));
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

			OnPropertyChanged(nameof(Elapsed));
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

			OnPropertyChanged(nameof(Elapsed));
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

			OnPropertyChanged(nameof(Elapsed));
		}

		/// <summary>
		/// Start the timer, performs the action, then stops the timer.
		/// </summary>
		/// <param name="action"> The action to be timed. </param>
		public void Time(Action action)
		{
			try
			{
				Start();
				action();
			}
			finally
			{
				Stop();
			}
		}

		/// <summary>
		/// Start the timer, performs the function, then stops the timer, then returns the value from the function.
		/// </summary>
		/// <param name="function"> The action to be timed. </param>
		/// <returns> The value return from the function. </returns>
		public T Time<T>(Func<T> function)
		{
			try
			{
				Start();
				return function();
			}
			finally
			{
				Stop();
			}
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