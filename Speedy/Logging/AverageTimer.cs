#region References

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

#endregion

namespace Speedy.Logging
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
		/// <param name="limit"> The maximum amount of values to average. </param>
		public AverageTimer(int limit) : this(limit, new DefaultDispatcher())
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
			_timer.PropertyChanged += TimerOnPropertyChanged;
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
		public TimeSpan Elapsed => _timer.Elapsed;

		/// <summary>
		/// Indicates if the timer is running;
		/// </summary>
		public bool IsRunning => _timer.IsRunning;

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
			_timer.Reset();
		}

		/// <summary>
		/// Reset the average timer.
		/// </summary>
		public void Reset()
		{
			_collection.Clear();

			Average = TimeSpan.Zero;
			Samples = 0;
			Count = 0;
		}

		/// <summary>
		/// Start the timer.
		/// </summary>
		public void Start()
		{
			_timer.Restart();
		}

		/// <summary>
		/// Stop the timer then update the average.
		/// </summary>
		public void Stop()
		{
			if (!IsRunning)
			{
				// The timer is not running so do not mess up the average
				return;
			}

			_timer.Stop();
			_collection.Add(Elapsed.Ticks);

			while (_collection.Count > _limit)
			{
				_collection.RemoveAt(0);
			}

			Samples = _collection.Count;
			Average = new TimeSpan((long) _collection.Average());
			Count++;
		}

		private void TimerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Timer.Elapsed):
					OnPropertyChanged(nameof(Elapsed));
					break;

				case nameof(Timer.IsRunning):
					OnPropertyChanged(nameof(IsRunning));
					break;
			}
		}

		#endregion
	}
}