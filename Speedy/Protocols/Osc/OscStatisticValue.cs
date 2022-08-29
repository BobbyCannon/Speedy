#region References

using System;

#endregion

namespace Speedy.Protocols.Osc
{
	/// <summary>
	/// Represents a value for an OSC statistic.
	/// </summary>
	public class OscStatisticValue : Bindable
	{
		#region Fields

		private long _lastTotal;
		private DateTime _lastUpdate;

		#endregion

		#region Constructors

		/// <summary>
		/// Create an instance of the OSC statistic.
		/// </summary>
		/// <param name="name"> The name of the value. </param>
		public OscStatisticValue(string name) : this(name, null)
		{
		}

		/// <summary>
		/// Create an instance of the OSC statistic.
		/// </summary>
		/// <param name="name"> The name of the value. </param>
		/// <param name="dispatcher"> The dispatcher for updates. </param>
		public OscStatisticValue(string name, IDispatcher dispatcher) : base(dispatcher)
		{
			Name = name;
			UpdateInterval = TimeSpan.FromMilliseconds(1000);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The name of the statistic.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The rate of the statistic.
		/// </summary>
		public float Rate { get; private set; }

		/// <summary>
		/// The total of the statistic.
		/// </summary>
		public long Total { get; private set; }

		/// <summary>
		/// The interval to update the statistic.
		/// </summary>
		public TimeSpan UpdateInterval { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Increment the statistic. Default amount is 1.
		/// </summary>
		/// <param name="amount"> The amount to increment. Defaults to 1. </param>
		public void Increment(int amount = 1)
		{
			Total += amount;
		}

		/// <summary>
		/// Notify the UI with property changed events.
		/// </summary>
		public void InvokePropertyChanged()
		{
			OnPropertyChanged(nameof(Total));
			OnPropertyChanged(nameof(Rate));
		}

		/// <summary>
		/// Resets the statistic to 0.
		/// </summary>
		public void Reset()
		{
			Rate = 0;
			Total = 0;

			_lastTotal = 0;
		}

		/// <summary>
		/// Updates the rate of updates.
		/// </summary>
		public void UpdateRate()
		{
			var now = TimeService.Now;
			var time = now - _lastUpdate;

			if (time < UpdateInterval)
			{
				return;
			}

			var diff = Total - _lastTotal;
			Rate = diff / (float) time.TotalSeconds;

			_lastTotal = Total;
			_lastUpdate = now;
		}

		#endregion
	}
}