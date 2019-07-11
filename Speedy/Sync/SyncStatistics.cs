#region References

using Speedy.Storage;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the communication statistics for a sync client.
	/// </summary>
	public class SyncStatistics : IUpdatable<SyncStatistics>
	{
		#region Properties

		/// <summary>
		/// Represents changes written (incoming) to this client. 
		/// </summary>
		public int AppliedChanges { get; set; }
		
		/// <summary>
		/// Represents corrections written (incoming) to this client. 
		/// </summary>
		public int AppliedCorrections { get; set; }
		
		/// <summary>
		/// Represents changes sent (outgoing) from this client.
		/// </summary>
		public int Changes { get; set; }

		/// <summary>
		/// Represents corrections sent (outgoing) from this client.
		/// </summary>
		public int Corrections { get; set; }

		/// <summary>
		/// Returns true if the statistics are all zero.
		/// </summary>
		public bool IsReset => AppliedChanges == 0 && AppliedCorrections == 0 && Changes == 0 && Corrections == 0;

		#endregion

		#region Methods

		/// <summary>
		/// Allows resetting of the sync statistics.
		/// </summary>
		public void Reset()
		{
			AppliedChanges = 0;
			AppliedCorrections = 0;
			Changes = 0;
			Corrections = 0;
		}

		/// <inheritdocs />
		public void Update(SyncStatistics value)
		{
			if (value == null)
			{
				return;
			}

			AppliedChanges = value.AppliedChanges;
			AppliedCorrections = value.AppliedCorrections;
			Changes = value.Changes;
			Corrections = value.Corrections;
		}

		/// <inheritdocs />
		public void Update(object value)
		{
			Update(value as SyncStatistics);
		}

		#endregion
	}
}