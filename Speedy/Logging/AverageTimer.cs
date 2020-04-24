#region References

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

#endregion

namespace Speedy.Logging
{
	/// <summary>
	/// Average timer for tracking the average processing time of work.
	/// </summary>
	public class AverageTimer : Timer
	{
		#region Fields

		private readonly Collection<long> _collection;
		private readonly int _limit;

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
		}

		#endregion

		#region Properties

		/// <summary>
		/// Number of samples currently being averaged.
		/// </summary>
		public int Samples { get; private set; }

		/// <summary>
		/// Returns the Average value as TimeSpan. This expects the Average values to be "Ticks".
		/// </summary>
		public TimeSpan Average { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Stop a timed interval then update the average.
		/// </summary>
		public override void Stop()
		{
			if (!IsRunning)
			{
				// The timer is not running so do not mess up the average
				return;
			}

			base.Stop();

			_collection.Add(Elapsed.Ticks);

			while (_collection.Count > _limit)
			{
				_collection.RemoveAt(0);
			}

			Samples = _collection.Count;
			Average = new TimeSpan((long) _collection.Average());
		}

		#endregion
	}
}