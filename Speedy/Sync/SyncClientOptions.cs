#region References

using Speedy.Storage;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the options for a sync client
	/// </summary>
	public class SyncClientOptions : IUpdatable<SyncClientOptions>
	{
		#region Properties

		/// <summary>
		/// Indicates this client should maintain dates, meaning as you save data the ModifiedOn will be updated to the current time.
		/// This should really only be set for the "Master" sync client that represents the master dataset.
		/// </summary>
		public bool MaintainModifiedOn { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public void UpdateWith(SyncClientOptions value, bool excludeVirtuals = true, params string[] exclusions)
		{
			if (value is null)
			{
				return;
			}

			MaintainModifiedOn = value.MaintainModifiedOn;
		}

		/// <inheritdoc />
		public void UpdateWith(object value, bool excludeVirtuals = true, params string[] exclusions)
		{
			UpdateWith(value as SyncClientOptions, excludeVirtuals, exclusions);
		}

		#endregion
	}
}