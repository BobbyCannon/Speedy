#region References

using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the communication statistics for a sync client.
	/// </summary>
	public class SyncStatistics : CloneableBindable<SyncStatistics>
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
		/// Represents how many times the sync client had to process applied changes individually.
		/// This means at some point one of the synced items had issues saving so we have to process items
		/// individually so we can determine which item is having issues.
		/// </summary>
		public int IndividualProcessCount { get; set; }

		/// <summary>
		/// Returns true if the statistics are all zero.
		/// </summary>
		public bool IsReset => AppliedChanges == 0 && AppliedCorrections == 0 && Changes == 0 && Corrections == 0 && IndividualProcessCount == 0;

		#endregion

		#region Methods

		/// <inheritdoc />
		public override SyncStatistics DeepClone(int levels = -1)
		{
			return new()
			{
				AppliedChanges = AppliedChanges,
				AppliedCorrections = AppliedCorrections,
				Changes = Changes,
				Corrections = Corrections,
				IndividualProcessCount = IndividualProcessCount
			};
		}

		/// <summary>
		/// Allows resetting of the sync statistics.
		/// </summary>
		public void Reset()
		{
			AppliedChanges = 0;
			AppliedCorrections = 0;
			Changes = 0;
			Corrections = 0;
			IndividualProcessCount = 0;
		}

		/// <inheritdoc />
		public override void UpdateWith(SyncStatistics update, params string[] exclusions)
		{
			if (update == null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(AppliedChanges)), x => x.AppliedChanges = update.AppliedChanges);
			this.IfThen(x => !exclusions.Contains(nameof(AppliedCorrections)), x => x.AppliedCorrections = update.AppliedCorrections);
			this.IfThen(x => !exclusions.Contains(nameof(Changes)), x => x.Changes = update.Changes);
			this.IfThen(x => !exclusions.Contains(nameof(Corrections)), x => x.Corrections = update.Corrections);
			this.IfThen(x => !exclusions.Contains(nameof(IndividualProcessCount)), x => x.IndividualProcessCount = update.IndividualProcessCount);
		}

		#endregion
	}
}