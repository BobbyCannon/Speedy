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
		public bool IsReset => (AppliedChanges == 0) && (AppliedCorrections == 0) && (Changes == 0) && (Corrections == 0) && (IndividualProcessCount == 0);

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
			IndividualProcessCount = 0;
		}

		/// <summary>
		/// Update the SyncStatistics with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public override void UpdateWith(SyncStatistics update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				AppliedChanges = update.AppliedChanges;
				AppliedCorrections = update.AppliedCorrections;
				Changes = update.Changes;
				Corrections = update.Corrections;
				IndividualProcessCount = update.IndividualProcessCount;
			}
			else
			{
				this.IfThen(x => !exclusions.Contains(nameof(AppliedChanges)), x => x.AppliedChanges = update.AppliedChanges);
				this.IfThen(x => !exclusions.Contains(nameof(AppliedCorrections)), x => x.AppliedCorrections = update.AppliedCorrections);
				this.IfThen(x => !exclusions.Contains(nameof(Changes)), x => x.Changes = update.Changes);
				this.IfThen(x => !exclusions.Contains(nameof(Corrections)), x => x.Corrections = update.Corrections);
				this.IfThen(x => !exclusions.Contains(nameof(IndividualProcessCount)), x => x.IndividualProcessCount = update.IndividualProcessCount);
			}
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case SyncStatistics options:
				{
					UpdateWith(options, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
		}

		#endregion
	}
}