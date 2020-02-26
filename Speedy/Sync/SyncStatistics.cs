namespace Speedy.Sync
{
	/// <summary>
	/// Represents the communication statistics for a sync client.
	/// </summary>
	public class SyncStatistics : Bindable<SyncStatistics>
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

		/// <inheritdoc />
		public override void UpdateWith(SyncStatistics update, params string[] exclusions)
		{
			if (update == null)
			{
				return;
			}

			AppliedChanges = update.AppliedChanges;
			AppliedCorrections = update.AppliedCorrections;
			Changes = update.Changes;
			Corrections = update.Corrections;
		}

		#endregion
	}
}