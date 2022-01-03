#region References

using System;
using System.Diagnostics;

#endregion

namespace Speedy.Profiling
{
	/// <summary>
	/// A service for debouncing an action.
	/// </summary>
	/// <typeparam name="T"> The type to pass to the action. </typeparam>
	public class DebounceService<T> : DebounceService
	{
		#region Constructors

		/// <summary>
		/// Create an instance of the service for debouncing an action.
		/// </summary>
		/// <param name="timeSpan"> The amount of time before the action will trigger. </param>
		/// <param name="enabled"> A flag to enable the service. Defaults to true. </param>
		public DebounceService(TimeSpan timeSpan, bool enabled = true) : base(timeSpan, enabled)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Trigger the service. Will be trigger after the timespan or immediately if force is true.
		/// </summary>
		/// <param name="value"> The value to trigger with. </param>
		/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
		public void Trigger(T value, bool force = false)
		{
			base.Trigger(value, force);
		}

		#endregion
	}

	/// <summary>
	/// A service for debouncing an action.
	/// </summary>
	public class DebounceService : IDisposable
	{
		#region Fields

		private System.Timers.Timer _timer;
		private readonly Stopwatch _watch;

		#endregion

		#region Constructors

		/// <summary>
		/// Create an instance of the service for debouncing an action.
		/// </summary>
		/// <param name="timeSpan"> The amount of time before the action will trigger. </param>
		/// <param name="enabled"> A flag to enable the service. Defaults to true. </param>
		public DebounceService(TimeSpan timeSpan, bool enabled = true)
		{
			_watch = new Stopwatch();
			_timer = new System.Timers.Timer { Interval = timeSpan.TotalMilliseconds, Enabled = false };
			_timer.Elapsed += TimerTick;

			Enabled = enabled;
		}

		#endregion

		#region Properties

		/// <summary>
		/// The data to be store for the latest trigger.
		/// </summary>
		public object Data { get; protected set; }

		/// <summary>
		/// A flag to enable the service. Defaults to true.
		/// </summary>
		public bool Enabled { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Trigger the service. Will be trigger after the timespan or immediately if force is true.
		/// </summary>
		/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
		public void Trigger(bool force = false)
		{
			Trigger(default, force);
		}

		/// <summary>
		/// Trigger the service. Will be trigger after the timespan or immediately if force is true.
		/// </summary>
		/// <param name="value"> The value to trigger with. </param>
		/// <param name="force"> An optional flag to immediately trigger if true. Defaults to false. </param>
		public void Trigger(object value, bool force = false)
		{
			var timer = _timer;
			if (timer == null)
			{
				return;
			}

			timer.Stop();

			if (!_watch.IsRunning)
			{
				_watch.Start();
			}

			Data = value;

			if (_watch.Elapsed.TotalMilliseconds >= timer.Interval)
			{
				// Enough time has passed since our first trigger so we need to go ahead and force a trigger
				force = true;
			}

			// if the debounce service is not enabled
			if (!Enabled)
			{
				// then just restart the timer
				force = false;
			}

			if (force)
			{
				TimerTick(this, value);
			}
			else
			{
				timer.Start();
			}
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

			_watch.Stop();

			var timer = _timer;
			_timer = null;
			
			if (timer != null)
			{
				timer.Elapsed -= TimerTick;
				timer.Dispose();
			}
		}

		private void TimerTick(object sender, object e)
		{
			_timer?.Stop();
			_watch?.Reset();

			if (!Enabled)
			{
				// Just restart the trigger, it's not ready
				Trigger(Data);
				return;
			}

			Action?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Events

		/// <summary>
		/// The action to be triggered.
		/// </summary>
		public event EventHandler Action;

		#endregion
	}
}
