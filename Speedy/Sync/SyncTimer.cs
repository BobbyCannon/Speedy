#region References

using Speedy.Profiling;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents a timer for tracking a sync session.
	/// </summary>
	public class SyncTimer : AverageTimer
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		public SyncTimer() : base(10, null)
		{
		}

		/// <summary>
		/// Instantiates an instance of the class.
		/// </summary>
		/// <param name="limit"> Optional limit of syncs to average. </param>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public SyncTimer(int limit, IDispatcher dispatcher) : base(limit, dispatcher)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Tracks the number of cancelled syncs.
		/// </summary>
		public int CancelledSyncs { get; set; }

		/// <summary>
		/// Tracks the number of failed syncs.
		/// </summary>
		public int FailedSyncs { get; set; }

		/// <summary>
		/// Tracks the number of successful syncs.
		/// </summary>
		public int SuccessfulSyncs { get; set; }

		#endregion
	}
}