#region References

using System;
using System.Collections.ObjectModel;
using System.Linq;

#endregion

namespace Speedy.Profiling
{
	/// <summary>
	/// Average timer for tracking the average processing time of work.
	/// </summary>
	public class AverageTimer : Bindable
	{
		#region Fields

		private readonly Collection<long> _collection;
		private readonly int _limit;
		private readonly Timer _timer;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiate the average service.
		/// </summary>
		public AverageTimer() : this(0, new DefaultDispatcher())
		{
		}

		/// <summary>
		/// Instantiate the average service.
		/// </summary>
		/// <param name="limit"> The maximum amount of values to average. </param>
		public AverageTimer(int limit) : this(limit, new DefaultDispatcher())
		{
		}

		/// <summary>
		/// Instantiate the average service.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher. </param>
		public AverageTimer(IDispatcher dispatcher) : this(0, dispatcher)
		{
		}

		/// <summary>
		/// Instantiate the average service.
		/// </summary>
		/// <param name="limit"> The maximum amount of values to average. </param>
		/// <param name="dispatcher"> The dispatcher. </param>
		public AverageTimer(int limit, IDispatcher dispatcher) : base(dispatcher)
		{
			_collection = new Collection<long>();
			_limit = limit;
			_timer = new Timer();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Returns the Average value as TimeSpan. This expects the Average values to be "Ticks".
		/// </summary>
		public TimeSpan Average { get; private set; }

		/// <summary>
		/// Number of times this timer has been called.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		/// The amount of time that has elapsed.
		/// </summary>
		public TimeSpan Elapsed => _timer?.Elapsed ?? TimeSpan.Zero;

		/// <summary>
		/// Indicates if the timer is running;
		/// </summary>
		public bool IsRunning => _timer?.IsRunning ?? false;

		/// <summary>
		/// Number of samples currently being averaged.
		/// </summary>
		public int Samples { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Cancel the timer.
		/// </summary>
		public void Cancel()
		{
			_timer.Stop();

			OnPropertyChanged(nameof(Elapsed));
			OnPropertyChanged(nameof(IsRunning));
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
		/// Reset the average timer.
		/// </summary>
		public void Reset()
		{
			_collection.Clear();
			_timer.Reset();

			Average = TimeSpan.Zero;
			Samples = 0;
			Count = 0;

			OnPropertyChanged(nameof(Elapsed));
			OnPropertyChanged(nameof(IsRunning));
		}

		/// <summary>
		/// Start the timer.
		/// </summary>
		public void Start()
		{
			_timer.Reset();
			_timer.Restart();

			OnPropertyChanged(nameof(Elapsed));
			OnPropertyChanged(nameof(IsRunning));
		}

		/// <summary>
		/// Stop the timer then update the average.
		/// </summary>
		public void Stop()
		{
			try
			{
				if (!IsRunning)
				{
					// The timer is not running so do not mess up the average
					return;
				}

				_timer.Stop();

				if (_limit == 0)
				{
					Average = Count <= 0 ? _timer.Elapsed : TimeSpan.FromTicks((Average.Ticks + Elapsed.Ticks) / 2);
					Count++;
					return;
				}

				_collection.Add(_timer.Elapsed.Ticks);

				while (_collection.Count > _limit)
				{
					_collection.RemoveAt(0);
				}

				Samples = _collection.Count;
				Average = new TimeSpan((long) _collection.Average());
				Count++;
			}
			finally
			{
				OnPropertyChanged(nameof(Elapsed));
				OnPropertyChanged(nameof(IsRunning));
			}
		}

		#endregion
	}
}