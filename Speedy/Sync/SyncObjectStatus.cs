namespace Speedy.Sync
{
	/// <summary>
	/// Represents the sync state of an entity
	/// </summary>
	public enum SyncObjectStatus
	{
		/// <summary>
		/// This entity was added.
		/// </summary>
		Added = 0,

		/// <summary>
		/// This entity was last modified.
		/// </summary>
		Modified = 1,

		/// <summary>
		/// This entity was delete.
		/// </summary>
		Deleted = 2
	}
}